﻿namespace ExploreGambia.API.Models.DTOs
{
    public class AddTourGuideRequestDto
    {
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty; 

        public bool IsAvailable { get; set; } = true; 
    }
}
