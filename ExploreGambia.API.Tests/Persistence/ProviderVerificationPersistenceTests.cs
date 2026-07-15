using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ExploreGambia.API.Tests.Persistence;

public class ProviderVerificationPersistenceTests
{
    [Fact]
    public async Task TourGuide_CanBePersistedWithoutVerification()
    {
        await using var database = await CreateDatabaseAsync();
        var guide = CreateTourGuide();

        database.Context.TourGuides.Add(guide);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var persistedGuide = await database.Context.TourGuides
            .Include(candidate => candidate.Verification)
            .SingleAsync(candidate => candidate.TourGuideId == guide.TourGuideId);

        Assert.Null(persistedGuide.Verification);
        Assert.Empty(database.Context.ProviderVerifications);
    }

    [Fact]
    public async Task Verification_IsPersistedAsOneToOneGuideRelationship()
    {
        await using var database = await CreateDatabaseAsync();
        var guide = CreateTourGuide();
        var verification = new ProviderVerification
        {
            ProviderVerificationId = Guid.NewGuid(),
            TourGuide = guide,
            DocumentType = "NationalId",
            IssuingCountry = "GM",
            MaskedDocumentNumber = "****1234",
            DocumentExpiryDate = new DateOnly(2030, 12, 31)
        };

        guide.Verification = verification;
        database.Context.TourGuides.Add(guide);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var persistedGuide = await database.Context.TourGuides
            .Include(candidate => candidate.Verification)
            .SingleAsync(candidate => candidate.TourGuideId == guide.TourGuideId);

        Assert.NotNull(persistedGuide.Verification);
        Assert.Equal(verification.ProviderVerificationId, persistedGuide.Verification.ProviderVerificationId);
        Assert.Equal(VerificationStatus.NotStarted, persistedGuide.Verification.Status);
        Assert.Equal(EvidenceDeletionStatus.NotRequired, persistedGuide.Verification.EvidenceDeletionStatus);
        Assert.Equal(0, persistedGuide.Verification.EvidenceDeletionAttempts);
    }

    [Fact]
    public async Task TwoVerifications_ForTheSameGuide_ViolateUniqueConstraint()
    {
        await using var database = await CreateDatabaseAsync();
        var guide = CreateTourGuide();

        database.Context.TourGuides.Add(guide);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        database.Context.ProviderVerifications.AddRange(
            new ProviderVerification
            {
                ProviderVerificationId = Guid.NewGuid(),
                TourGuideId = guide.TourGuideId
            },
            new ProviderVerification
            {
                ProviderVerificationId = Guid.NewGuid(),
                TourGuideId = guide.TourGuideId
            });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public void NewVerification_HasSafeDefaults()
    {
        var beforeCreation = DateTime.UtcNow;

        var verification = new ProviderVerification();

        Assert.Equal(VerificationStatus.NotStarted, verification.Status);
        Assert.Equal(EvidenceDeletionStatus.NotRequired, verification.EvidenceDeletionStatus);
        Assert.Equal(0, verification.EvidenceDeletionAttempts);
        Assert.InRange(verification.CreatedAt, beforeCreation, DateTime.UtcNow);
    }

    [Fact]
    public async Task Model_ContainsRequiredConstraintsDefaultsAndIndexes()
    {
        await using var database = await CreateDatabaseAsync();
        var entity = database.Context.Model.FindEntityType(typeof(ProviderVerification));

        Assert.NotNull(entity);
        Assert.Equal(
            nameof(ProviderVerification.ProviderVerificationId),
            Assert.Single(entity.FindPrimaryKey()!.Properties).Name);

        AssertMaxLength(entity, nameof(ProviderVerification.DocumentType), 50);
        AssertMaxLength(entity, nameof(ProviderVerification.IssuingCountry), 2);
        AssertMaxLength(entity, nameof(ProviderVerification.MaskedDocumentNumber), 32);
        AssertMaxLength(entity, nameof(ProviderVerification.TemporaryDocumentFrontKey), 500);
        AssertMaxLength(entity, nameof(ProviderVerification.TemporaryDocumentBackKey), 500);
        AssertMaxLength(entity, nameof(ProviderVerification.ReviewedByUserId), 450);
        AssertMaxLength(entity, nameof(ProviderVerification.ReviewReason), 1000);
        AssertMaxLength(entity, nameof(ProviderVerification.LastEvidenceDeletionError), 2000);

        Assert.Equal(
            VerificationStatus.NotStarted,
            entity.FindProperty(nameof(ProviderVerification.Status))!.GetDefaultValue());
        Assert.Equal(
            EvidenceDeletionStatus.NotRequired,
            entity.FindProperty(nameof(ProviderVerification.EvidenceDeletionStatus))!.GetDefaultValue());
        Assert.Equal(
            0,
            entity.FindProperty(nameof(ProviderVerification.EvidenceDeletionAttempts))!.GetDefaultValue());

        var relationship = Assert.Single(entity.GetForeignKeys());
        Assert.True(relationship.IsRequired);
        Assert.True(relationship.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, relationship.DeleteBehavior);
        Assert.Equal(typeof(TourGuide), relationship.PrincipalEntityType.ClrType);

        AssertIndex(entity, true, nameof(ProviderVerification.TourGuideId));
        AssertIndex(
            entity,
            false,
            nameof(ProviderVerification.Status),
            nameof(ProviderVerification.SubmittedAt));
        AssertIndex(entity, false, nameof(ProviderVerification.DocumentExpiryDate));
        AssertIndex(entity, false, nameof(ProviderVerification.EvidenceDeletionStatus));

        Assert.Null(entity.FindProperty("FullDocumentNumber"));
        Assert.Null(entity.FindProperty("DocumentImageBytes"));
        Assert.Null(entity.FindProperty("PublicDocumentUrl"));
    }

    private static void AssertMaxLength(IEntityType entity, string propertyName, int expectedLength)
    {
        Assert.Equal(expectedLength, entity.FindProperty(propertyName)!.GetMaxLength());
    }

    private static void AssertIndex(
        IEntityType entity,
        bool isUnique,
        params string[] expectedPropertyNames)
    {
        var index = entity.GetIndexes().Single(candidate =>
            candidate.Properties.Select(property => property.Name).SequenceEqual(expectedPropertyNames));

        Assert.Equal(isUnique, index.IsUnique);
    }

    private static TourGuide CreateTourGuide()
    {
        return new TourGuide
        {
            TourGuideId = Guid.NewGuid(),
            FullName = "Persistence Test Guide",
            PhoneNumber = "+2200000000",
            Email = $"guide-{Guid.NewGuid():N}@example.com",
            Bio = "Provider verification persistence test guide"
        };
    }

    private static async Task<SqliteTestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ExploreGambiaDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ExploreGambiaDbContext(options);
        await context.Database.EnsureCreatedAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE "AspNetUsers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY
            );
            """);

        return new SqliteTestDatabase(connection, context);
    }

    private sealed class SqliteTestDatabase(
        SqliteConnection connection,
        ExploreGambiaDbContext context) : IAsyncDisposable
    {
        public ExploreGambiaDbContext Context { get; } = context;

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
