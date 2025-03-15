﻿using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services
{
    public class RatingService(IRatingRepository ratingRepository,
        IMovieRepository movieRepository) : IRatingService
    {
        private readonly IRatingRepository _ratingRepository = ratingRepository;
        private readonly IMovieRepository _movieRepository = movieRepository;

        public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId, CancellationToken token = default)
        {
            if (rating is <= 0 or > 5)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Rating", "Rating must be between 1 and 5")
                });
            }

            var movieExists = await _movieRepository.ExistsAsync(movieId, token);

            if (!movieExists)
            {
                return false;
            }

            return await _ratingRepository.RateMovieAsync(movieId, rating, userId, token);
        }
    }
}
