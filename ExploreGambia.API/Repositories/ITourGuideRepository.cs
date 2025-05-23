﻿using ExploreGambia.API.Models.Domain;

namespace ExploreGambia.API.Repositories
{
    public interface ITourGuideRepository
    {
        Task<List<TourGuide>> GetAllAsync(string? sortBy = null, bool isAscending = true, string? searchTerm = null, int pageNumber = 1,
            int pageSize = 10);


        Task<TourGuide?> GetTourGuideByIdAsync(Guid id);


        Task<TourGuide> CreateTourGuideAsync(TourGuide tourGuide);


        Task<TourGuide?> UpdateTourGuideAsync(Guid id, TourGuide tourGuide);

        Task<TourGuide?> DeleteTourGuideAsync(Guid id);

    }
}
