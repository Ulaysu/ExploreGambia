using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Models.Domain;
using ExploreGambia.API.Validations;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddBookingRequestDto
    {

        [Required(ErrorMessage = "TourId is required.")]
        [GuidNotEmpty(ErrorMessage = "TourId cannot be an empty GUID, an empty string or null.")]
        public Guid TourId { get; set; }

        [Required(ErrorMessage = "Number of people is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of people must be at least 1.")]
        public int NumberOfPeople { get; set; } 

        
    }
}
