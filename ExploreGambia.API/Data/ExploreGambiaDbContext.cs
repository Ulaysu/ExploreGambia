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

            // Booking <-> Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete payments if booking is removed          
        }
    }
}
