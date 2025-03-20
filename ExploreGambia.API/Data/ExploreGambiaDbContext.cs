using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Data
{
    public class ExploreGambiaDbContext : DbContext
    {
        public ExploreGambiaDbContext(DbContextOptions<ExploreGambiaDbContext> options):base(options)
        {
            
        }

        public DbSet<Attraction> Attractions { get; set; }
        public DbSet<Tour> Tours { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<TourGuide> TourGuides { get; set; }
        public DbSet<Payment> Payments { get; set; } // Added Payments table
        public DbSet<Review> Reviews { get; set; } // Added Review Table


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            // Tour <-> Attraction (Many-to-Many)
            modelBuilder.Entity<TourAttraction>()
                .HasKey(ta => new { ta.TourId, ta.AttractionId }); // Composite Key

            modelBuilder.Entity<TourAttraction>()
                .HasOne(ta => ta.Tour)
                .WithMany(t => t.TourAttractions)
                .HasForeignKey(ta => ta.TourId);

            modelBuilder.Entity<TourAttraction>()
                .HasOne(ta => ta.Attraction)
                .WithMany(a => a.TourAttractions)
                .HasForeignKey(ta => ta.AttractionId);

            // Booking <-> Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete payments if booking is removed

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Tour)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TourId);

            /*modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);*/
        }
    }
}
