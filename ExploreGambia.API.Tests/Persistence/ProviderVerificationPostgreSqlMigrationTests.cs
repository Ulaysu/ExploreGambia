using ExploreGambia.API.Data;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ExploreGambia.API.Tests.Persistence;

public class ProviderVerificationPostgreSqlMigrationTests
{
    private const string PreviousAppMigration = "20260625155747_AddReviewsTable";

    [PostgreSqlFact]
    public async Task FreshDatabase_ProductionMigrationOrder_AppliesProtectsConcurrentWritesAndRollsBack()
    {
        var serverConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlFactAttribute.ConnectionStringEnvironmentVariable)!;
        await using var database = await TemporaryPostgreSqlDatabase.CreateAsync(serverConnectionString);

        var appOptions = new DbContextOptionsBuilder<ExploreGambiaDbContext>()
            .UseNpgsql(
                database.ConnectionString,
                options => options.MigrationsHistoryTable("__EFMigrationsHistory_App"))
            .Options;
        var authOptions = new DbContextOptionsBuilder<ExploreGambiaAuthDbContext>()
            .UseNpgsql(
                database.ConnectionString,
                options => options.MigrationsHistoryTable("__EFMigrationsHistory_Auth"))
            .Options;

        await using (var authContext = new ExploreGambiaAuthDbContext(authOptions))
        {
            await authContext.Database.MigrateAsync();
        }

        var guideId = Guid.NewGuid();
        var verificationId = Guid.NewGuid();

        await using (var appContext = new ExploreGambiaDbContext(appOptions))
        {
            await appContext.Database.MigrateAsync(PreviousAppMigration);
            appContext.TourGuides.Add(CreateTourGuide(guideId));
            await appContext.SaveChangesAsync();

            await appContext.Database.MigrateAsync();

            Assert.True(await TableExistsAsync(database.ConnectionString, "ProviderVerifications"));
            Assert.True(await IndexExistsAsync(
                database.ConnectionString,
                "ProviderVerifications",
                "Status",
                "DocumentExpiryDate"));
            Assert.True(await IndexExistsAsync(
                database.ConnectionString,
                "ProviderVerifications",
                "EvidenceDeletionStatus",
                "LastEvidenceDeletionAttemptAt"));
            Assert.True(await ConstraintExistsAsync(
                database.ConnectionString,
                "CK_ProviderVerifications_Status"));
            Assert.True(await ConstraintExistsAsync(
                database.ConnectionString,
                "CK_ProviderVerifications_EvidenceDeletionStatus"));
            Assert.True(await ConstraintExistsAsync(
                database.ConnectionString,
                "CK_ProviderVerifications_EvidenceDeletionAttempts"));
            Assert.True(await RestrictDeleteForeignKeyExistsAsync(database.ConnectionString));

            appContext.ProviderVerifications.Add(new ProviderVerification
            {
                ProviderVerificationId = verificationId,
                TourGuideId = guideId
            });
            await appContext.SaveChangesAsync();
        }

        await AssertConcurrentUpdateIsRejectedAsync(appOptions, verificationId);

        await using (var rollbackContext = new ExploreGambiaDbContext(appOptions))
        {
            await rollbackContext.Database.MigrateAsync(PreviousAppMigration);

            Assert.False(await TableExistsAsync(database.ConnectionString, "ProviderVerifications"));
            Assert.True(await rollbackContext.TourGuides.AnyAsync(guide => guide.TourGuideId == guideId));
        }
    }

    private static async Task AssertConcurrentUpdateIsRejectedAsync(
        DbContextOptions<ExploreGambiaDbContext> options,
        Guid verificationId)
    {
        await using var firstContext = new ExploreGambiaDbContext(options);
        await using var secondContext = new ExploreGambiaDbContext(options);

        var firstCopy = await firstContext.ProviderVerifications.SingleAsync(
            verification => verification.ProviderVerificationId == verificationId);
        var staleCopy = await secondContext.ProviderVerifications.SingleAsync(
            verification => verification.ProviderVerificationId == verificationId);

        firstCopy.Status = VerificationStatus.PendingReview;
        await firstContext.SaveChangesAsync();

        staleCopy.Status = VerificationStatus.Approved;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => secondContext.SaveChangesAsync());
    }

    private static TourGuide CreateTourGuide(Guid guideId)
    {
        return new TourGuide
        {
            TourGuideId = guideId,
            FullName = "PostgreSQL Migration Test Guide",
            PhoneNumber = "+2200000000",
            Email = $"migration-{guideId:N}@example.com",
            Bio = "Provider verification PostgreSQL migration test guide"
        };
    }

    private static async Task<bool> TableExistsAsync(string connectionString, string tableName)
    {
        const string sql =
            "SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_tables WHERE schemaname = 'public' AND tablename = @tableName);";
        return await ExecuteBooleanScalarAsync(connectionString, sql, ("tableName", tableName));
    }

    private static async Task<bool> IndexExistsAsync(
        string connectionString,
        string tableName,
        params string[] orderedColumns)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM pg_catalog.pg_indexes
                WHERE schemaname = 'public'
                  AND tablename = @tableName
                  AND indexdef LIKE @columnPattern
            );
            """;
        var columnPattern = $"%({string.Join(", ", orderedColumns.Select(column => $"\"{column}\""))})%";
        return await ExecuteBooleanScalarAsync(
            connectionString,
            sql,
            ("tableName", tableName),
            ("columnPattern", columnPattern));
    }

    private static async Task<bool> ConstraintExistsAsync(string connectionString, string constraintName)
    {
        const string sql =
            "SELECT EXISTS (SELECT 1 FROM pg_catalog.pg_constraint WHERE conname = @constraintName);";
        return await ExecuteBooleanScalarAsync(connectionString, sql, ("constraintName", constraintName));
    }

    private static async Task<bool> RestrictDeleteForeignKeyExistsAsync(string connectionString)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM pg_catalog.pg_constraint
                WHERE conname = 'FK_ProviderVerifications_TourGuides_TourGuideId'
                  AND confdeltype = 'r'
            );
            """;
        return await ExecuteBooleanScalarAsync(connectionString, sql);
    }

    private static async Task<bool> ExecuteBooleanScalarAsync(
        string connectionString,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter.Name, parameter.Value);
        }

        return (bool)(await command.ExecuteScalarAsync())!;
    }

    private sealed class TemporaryPostgreSqlDatabase : IAsyncDisposable
    {
        private readonly string adminConnectionString;
        private readonly string databaseName;

        private TemporaryPostgreSqlDatabase(
            string adminConnectionString,
            string databaseName,
            string connectionString)
        {
            this.adminConnectionString = adminConnectionString;
            this.databaseName = databaseName;
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public static async Task<TemporaryPostgreSqlDatabase> CreateAsync(string serverConnectionString)
        {
            var databaseName = $"exploregambia_verification_{Guid.NewGuid():N}";
            var adminBuilder = new NpgsqlConnectionStringBuilder(serverConnectionString)
            {
                Database = "postgres",
                Pooling = false
            };

            await using (var connection = new NpgsqlConnection(adminBuilder.ConnectionString))
            {
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\";", connection);
                await command.ExecuteNonQueryAsync();
            }

            var testBuilder = new NpgsqlConnectionStringBuilder(serverConnectionString)
            {
                Database = databaseName,
                Pooling = false
            };

            return new TemporaryPostgreSqlDatabase(
                adminBuilder.ConnectionString,
                databaseName,
                testBuilder.ConnectionString);
        }

        public async ValueTask DisposeAsync()
        {
            await using var connection = new NpgsqlConnection(adminConnectionString);
            await connection.OpenAsync();

            await using (var terminateCommand = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @databaseName;",
                connection))
            {
                terminateCommand.Parameters.AddWithValue("databaseName", databaseName);
                await terminateCommand.ExecuteNonQueryAsync();
            }

            await using var dropCommand = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\";", connection);
            await dropCommand.ExecuteNonQueryAsync();
        }
    }
}
