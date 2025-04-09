using System.ComponentModel.DataAnnotations;
using ExploreGambia.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace ExploreGambia.API.Models.DTOs
{
    public class AddBookingRequestDto
    {

        [Required(ErrorMessage = "TourId is required.")]
        //[GuidNotEmpty(ErrorMessage = "TourId cannot be an empty GUID.")]
        public Guid TourId { get; set; } 

        public int NumberOfPeople { get; set; } 

        
    }
}
