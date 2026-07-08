using AutoMapper;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Models.DTOs;

namespace ExploreGambia.API.Mapping
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.ReviewId))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                // If the User relation is eagerly loaded, pull the readable FullName/Email, 
                // otherwise fallback gracefully to a standard placeholder.
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User != null ? $"{src.User.FirstName} {src.User.LastName}".Trim() : "Anonymous User"));
            
            CreateMap<Tour, TourDto>().ReverseMap();

            CreateMap<Tour, AdminTourDto>().ForMember( dest => dest.GuideName,
        opt => opt.MapFrom(src => src.TourGuide.FullName));

            CreateMap<AddTourRequestDto, Tour>()
                .ForMember(dest => dest.StartDate,
                 opt => opt.MapFrom(src => DateTime.SpecifyKind(src.StartDate, DateTimeKind.Utc)))
                .ForMember(dest => dest.EndDate,
                    opt => opt.MapFrom(src => DateTime.SpecifyKind(src.EndDate, DateTimeKind.Utc)));

            CreateMap<Tour, AddTourRequestDto>();

            CreateMap<UpdateTourRequestDto, Tour>()
               .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => DateTime.SpecifyKind(src.StartDate, DateTimeKind.Utc)))
               .ForMember(dest => dest.EndDate,
                   opt => opt.MapFrom(src => DateTime.SpecifyKind(src.EndDate, DateTimeKind.Utc)));

            CreateMap<Tour, UpdateTourRequestDto>();

            CreateMap<TourGuide, TourGuideDto>().ReverseMap();

            CreateMap<TourGuide, TourGuideProfileDto>().ReverseMap();

            CreateMap<TourGuide, AddTourGuideRequestDto>().ReverseMap();

            CreateMap<TourGuide, UpdateTourGuideRequestDto>().ReverseMap();

            CreateMap<Booking, BookingDto>().ReverseMap();

            CreateMap<AddBookingRequestDto, Booking>().ReverseMap();

            CreateMap<UpdateBookingRequestDto, Booking>().ReverseMap();

            CreateMap<Payment, PaymentDto>().ReverseMap();

            CreateMap<AddPaymentRequestDto, Payment>().ReverseMap();

            CreateMap<UpdatePaymentRequestDto, Payment>().ReverseMap();





        }
    }
}
