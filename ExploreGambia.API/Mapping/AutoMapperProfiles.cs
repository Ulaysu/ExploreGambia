using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Tour, TourDto>().ReverseMap();
            CreateMap<Tour, AddTourRequestDto>().ReverseMap();

            CreateMap<Tour, UpdateTourRequestDto>().ReverseMap();

            CreateMap<TourGuide, TourGuideDto>().ReverseMap();

            CreateMap<TourGuide, AddTourGuideRequestDto>().ReverseMap();

            CreateMap<TourGuide, UpdateTourGuideRequestDto>().ReverseMap();

            CreateMap<Booking, BookingDto>().ReverseMap();

            CreateMap<AddBookingRequestDto, Booking>().ReverseMap();

            CreateMap<Payment, PaymentDto>().ReverseMap();

            CreateMap<AddPaymentRequestDto, Payment>().ReverseMap();

            CreateMap<UpdatePaymentRequestDto, Payment>().ReverseMap();





        }
    }
}
