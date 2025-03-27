using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Data
{
    public class DataSeeder
    {
        
        private readonly ExploreGambiaDbContext _context;

        public DataSeeder(ExploreGambiaDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync(); // Ensures database is created and migrated

            await SeedTourGuides();
            await SeedTours();
            await SeedBookings();
            await SeedPayments();


        }

        private async Task SeedTourGuides()
        {
            if (!await _context.TourGuides.AnyAsync())
            {
                var tourGuides = new List<TourGuide>
                {
                    new TourGuide { TourGuideId = Guid.NewGuid(), FullName = "John Doe", PhoneNumber = "+2201234567", Email = "johndoe@example.com", Bio = "Experienced guide in Banjul.", IsAvailable = true },
                    new TourGuide { TourGuideId = Guid.NewGuid(), FullName = "Fatou Jobe", PhoneNumber = "+2207654321", Email = "fatoujobe@example.com", Bio = "Expert in nature tours.", IsAvailable = true },
                    new TourGuide { TourGuideId = Guid.NewGuid(), FullName = "Lamin Sowe", PhoneNumber = "+2209988776", Email = "laminsowe@example.com", Bio = "Cultural tour specialist.", IsAvailable = true },
                    new TourGuide { TourGuideId = Guid.NewGuid(), FullName = "Awa Ceesay", PhoneNumber = "+2205544332", Email = "awaceesay@example.com", Bio = "Birdwatching and wildlife tours.", IsAvailable = true },
                    new TourGuide { TourGuideId = Guid.NewGuid(), FullName = "Ousman Jallow", PhoneNumber = "+2207788991", Email = "ousmanjallow@example.com", Bio = "Historical sites expert.", IsAvailable = true }
                };

                _context.TourGuides.AddRange(tourGuides);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedTours()
        {
            if (!await _context.Tours.AnyAsync())
            {
                var tourGuides = await _context.TourGuides.ToListAsync();
                if (tourGuides.Count == 0) return;

                var tours = new List<Tour>
                {
                    new Tour { TourId = Guid.NewGuid(), Title = "Banjul City Tour", Description = "Explore the capital city.", Location = "Banjul", Price = 50.00m, MaxParticipants = 20, StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(6), ImageUrl = "https://example.com/banjul.jpg", IsAvailable = true, TourGuideId = tourGuides[0].TourGuideId },
                    new Tour { TourId = Guid.NewGuid(), Title = "Kunta Kinteh Island Visit", Description = "Historical tour.", Location = "James Island", Price = 75.00m, MaxParticipants = 15, StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(11), ImageUrl = "https://example.com/kunta.jpg", IsAvailable = true, TourGuideId = tourGuides[1].TourGuideId },
                    new Tour { TourId = Guid.NewGuid(), Title = "Wildlife Safari", Description = "Visit the wildlife reserves.", Location = "Abuko Nature Reserve", Price = 100.00m, MaxParticipants = 10, StartDate = DateTime.UtcNow.AddDays(12), EndDate = DateTime.UtcNow.AddDays(13), ImageUrl = "https://example.com/safari.jpg", IsAvailable = true, TourGuideId = tourGuides[2].TourGuideId },
                    new Tour { TourId = Guid.NewGuid(), Title = "River Gambia Cruise", Description = "Boat cruise experience.", Location = "River Gambia", Price = 120.00m, MaxParticipants = 25, StartDate = DateTime.UtcNow.AddDays(15), EndDate = DateTime.UtcNow.AddDays(16), ImageUrl = "https://example.com/cruise.jpg", IsAvailable = true, TourGuideId = tourGuides[3].TourGuideId },
                    new Tour { TourId = Guid.NewGuid(), Title = "Cultural Heritage Walk", Description = "Discover local traditions.", Location = "Bakau", Price = 55.00m, MaxParticipants = 18, StartDate = DateTime.UtcNow.AddDays(8), EndDate = DateTime.UtcNow.AddDays(9), ImageUrl = "https://example.com/culture.jpg", IsAvailable = true, TourGuideId = tourGuides[4].TourGuideId }
                };

                _context.Tours.AddRange(tours);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedBookings()
        {
            if (!await _context.Bookings.AnyAsync())
            {
                var tours = await _context.Tours.ToListAsync();
                if (tours.Count == 0) return;

                var bookings = new List<Booking>
                {
                    new Booking { BookingId = Guid.NewGuid(), TourId = tours[0].TourId, BookingDate = DateTime.UtcNow, NumberOfPeople = 2, TotalAmount = 100.00m, Status = BookingStatus.Confirmed },
                    new Booking { BookingId = Guid.NewGuid(), TourId = tours[1].TourId, BookingDate = DateTime.UtcNow, NumberOfPeople = 4, TotalAmount = 300.00m, Status = BookingStatus.Pending },
                    new Booking { BookingId = Guid.NewGuid(), TourId = tours[2].TourId, BookingDate = DateTime.UtcNow, NumberOfPeople = 3, TotalAmount = 300.00m, Status = BookingStatus.Completed },
                    new Booking { BookingId = Guid.NewGuid(), TourId = tours[3].TourId, BookingDate = DateTime.UtcNow, NumberOfPeople = 1, TotalAmount = 120.00m, Status = BookingStatus.Canceled },
                    new Booking { BookingId = Guid.NewGuid(), TourId = tours[4].TourId, BookingDate = DateTime.UtcNow, NumberOfPeople = 5, TotalAmount = 275.00m, Status = BookingStatus.Confirmed }
                };

                _context.Bookings.AddRange(bookings);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPayments()
        {
            if (!await _context.Payments.AnyAsync())
            {
                var bookings = await _context.Bookings.ToListAsync();
                if (bookings.Count == 0) return;

                var payments = new List<Payment>
                {
                    new Payment { PaymentId = Guid.NewGuid(), BookingId = bookings[0].BookingId, PaymentMethod = "Credit Card", Amount = 100.00m, PaymentDate = DateTime.UtcNow, IsSuccessful = true },
                    new Payment { PaymentId = Guid.NewGuid(), BookingId = bookings[1].BookingId, PaymentMethod = "PayPal", Amount = 300.00m, PaymentDate = DateTime.UtcNow, IsSuccessful = false },
                    new Payment { PaymentId = Guid.NewGuid(), BookingId = bookings[2].BookingId, PaymentMethod = "Cash", Amount = 300.00m, PaymentDate = DateTime.UtcNow, IsSuccessful = true },
                    new Payment { PaymentId = Guid.NewGuid(), BookingId = bookings[3].BookingId, PaymentMethod = "Bank Transfer", Amount = 120.00m, PaymentDate = DateTime.UtcNow, IsSuccessful = true },
                    new Payment { PaymentId = Guid.NewGuid(), BookingId = bookings[4].BookingId, PaymentMethod = "Credit Card", Amount = 275.00m, PaymentDate = DateTime.UtcNow, IsSuccessful = true }
                };

                _context.Payments.AddRange(payments);
                await _context.SaveChangesAsync();
            }
        }


    }
}
