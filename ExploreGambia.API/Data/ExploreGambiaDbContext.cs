using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Data
{
    public class ExploreGambiaDbContext : DbContext
    {
        public ExploreGambiaDbContext(DbContextOptions<ExploreGambiaDbContext> options):base(options)
        {
            
        }

       
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<Payment> Payments { get; set; } // Added Payments table
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProviderVerification> ProviderVerifications { get; set; }
        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // --- Review System Configuration (Newly Added) ---
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.ReviewId);

                entity.Property(r => r.Rating)
                      .IsRequired();

                entity.Property(r => r.Comment)
                      .HasMaxLength(1000)
                      .IsRequired();

                // CRITICAL: Unique index enforces the "One review per booking" rule at the DB layer
                entity.HasIndex(r => r.BookingId)
                      .IsUnique();

                // Review <-> Booking (One-to-One style constraint from Review side)
                entity.HasOne(r => r.Booking)
                      .WithMany() // No collection added to booking to avoid bloating it
                      .HasForeignKey(r => r.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Review <-> Tour (Many reviews belong to one Tour/Experience)
                entity.HasOne(r => r.Tour)
                      .WithMany()
                      .HasForeignKey(r => r.TourId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Review <-> ApplicationUser (Many reviews can be written by a User)
                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent dropping users if they have historical reviews
            });

            modelBuilder.Entity<Booking>()
                 .HasOne(b => b.User)
                 .WithMany()
                 .HasForeignKey(b => b.UserId)
                 .IsRequired(false).OnDelete(DeleteBehavior.SetNull); ;


            modelBuilder.Entity<ApplicationUser>()
                .ToTable("AspNetUsers", table => table.ExcludeFromMigrations());

            modelBuilder.Entity<Booking>()
                .Property(b => b.UserId)
                .HasMaxLength(450); // match AspNetUsers Id column length, no FK

            modelBuilder.Entity<TourGuide>()
                .HasIndex(g => g.UserId)
                .IsUnique();

            modelBuilder.Entity<TourGuide>()
                .HasOne(g => g.User)
                .WithOne(u => u.TourGuide)
                .HasForeignKey<TourGuide>(g => g.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProviderVerification>(entity =>
            {
                entity.ToTable(table =>
                {
                    table.HasCheckConstraint(
                        "CK_ProviderVerifications_Status",
                        "\"Status\" BETWEEN 0 AND 4");
                    table.HasCheckConstraint(
                        "CK_ProviderVerifications_EvidenceDeletionStatus",
                        "\"EvidenceDeletionStatus\" BETWEEN 0 AND 3");
                    table.HasCheckConstraint(
                        "CK_ProviderVerifications_EvidenceDeletionAttempts",
                        "\"EvidenceDeletionAttempts\" >= 0");
                });

                entity.HasKey(verification => verification.ProviderVerificationId);

                entity.Property(verification => verification.Status)
                    .HasDefaultValue(VerificationStatus.NotStarted);

                entity.Property(verification => verification.DocumentType)
                    .HasMaxLength(50);

                entity.Property(verification => verification.IssuingCountry)
                    .HasMaxLength(2);

                entity.Property(verification => verification.MaskedDocumentNumber)
                    .HasMaxLength(32);

                entity.Property(verification => verification.TemporaryDocumentFrontKey)
                    .HasMaxLength(500);

                entity.Property(verification => verification.TemporaryDocumentBackKey)
                    .HasMaxLength(500);

                entity.Property(verification => verification.ReviewedByUserId)
                    .HasMaxLength(450);

                entity.Property(verification => verification.ReviewReason)
                    .HasMaxLength(1000);

                entity.Property(verification => verification.EvidenceDeletionStatus)
                    .HasDefaultValue(EvidenceDeletionStatus.NotRequired);

                entity.Property(verification => verification.EvidenceDeletionAttempts)
                    .HasDefaultValue(0);

                entity.Property(verification => verification.LastEvidenceDeletionError)
                    .HasMaxLength(2000);

                var versionProperty = entity.Property(verification => verification.Version)
                    .IsConcurrencyToken();

                if (Database.IsNpgsql())
                {
                    versionProperty.IsRowVersion();
                }

                entity.HasOne(verification => verification.TourGuide)
                    .WithOne(guide => guide.Verification)
                    .HasForeignKey<ProviderVerification>(verification => verification.TourGuideId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(verification => verification.TourGuideId)
                    .IsUnique();

                entity.HasIndex(verification => new { verification.Status, verification.SubmittedAt });

                entity.HasIndex(verification => verification.DocumentExpiryDate);

                entity.HasIndex(verification => verification.EvidenceDeletionStatus);
            });

            // Tour <-> TourGuide (One-to-Many)
            modelBuilder.Entity<Tour>()
                .HasOne(t => t.TourGuide)
                .WithMany(g => g.Tours)
                .HasForeignKey(t => t.TourGuideId);

            // Booking <-> Tour (One-to-Many)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Tour)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TourId); 

            // Booking <-> Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete payments if booking is removed          
        }
    }
}
