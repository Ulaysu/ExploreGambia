using AutoMapper;
using ExploreGambia.API.Exceptions;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;
using ExploreGambia.API.Repositories;

namespace ExploreGambia.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository bookingRepository;
        private readonly ITourRepository tourRepository;
        private readonly IMapper mapper;

        public BookingService(IBookingRepository bookingRepository, ITourRepository tourRepository, IMapper mapper)
        {
            this.bookingRepository = bookingRepository;
            this.tourRepository = tourRepository;
            this.mapper = mapper;
        }

        public async Task<Booking> CreateBookingAsync(AddBookingRequestDto request)
        {
            var tour = await tourRepository.GetTourById(request.TourId)
                ?? throw new TourNotFoundException(request.TourId);

            if (!tour.IsAvailable)
            {
                throw new BusinessRuleException("This tour is not available for booking.");
            }

            if (request.NumberOfPeople > tour.MaxParticipants)
            {
                throw new BusinessRuleException("The number of people exceeds the tour's maximum allowed participants.");
            }

            var booking = mapper.Map<Booking>(request);
            booking.BookingId = Guid.NewGuid();
            booking.BookingDate = DateTime.UtcNow;
            booking.TotalAmount = tour.Price * request.NumberOfPeople;
            booking.Status = BookingStatus.Pending;
            booking.StatusUpdatedAt = DateTime.UtcNow;

            return await bookingRepository.CreateBookingAsync(booking);
        }

        public async Task<Booking?> UpdateBookingAsync(Guid id, UpdateBookingRequestDto request)
        {
            var booking = mapper.Map<Booking>(request);

            if (booking.TourId != Guid.Empty)
            {
                var tour = await tourRepository.GetTourById(booking.TourId)
                    ?? throw new TourNotFoundException(booking.TourId);

                if (request.NumberOfPeople > tour.MaxParticipants)
                {
                    throw new BusinessRuleException("The number of people exceeds the tour's maximum allowed participants.");
                }
            }

            return await bookingRepository.UpdateBookingAsync(id, booking);
        }
    }
}
