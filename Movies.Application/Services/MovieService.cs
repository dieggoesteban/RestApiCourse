﻿using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services
{
    public class MovieService(IMovieRepository movieRepository, 
        IValidator<Movie> movieValidator,
        IRatingRepository ratingRepository,
        IValidator<GetAllMoviesOptions> optionsValidator) : IMovieService
    {
        private readonly IMovieRepository _movieRepository = movieRepository;
        private readonly IValidator<Movie> _movieValidator = movieValidator;
        private readonly IRatingRepository _ratingRepository = ratingRepository;
        private readonly IValidator<GetAllMoviesOptions> _optionsValidator = optionsValidator;

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            await _movieValidator.ValidateAndThrowAsync(movie, token);
            return await _movieRepository.CreateAsync(movie, token);
        }

        public Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            return _movieRepository.DeleteByIdAsync(id, token);
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options);
            return await _movieRepository.GetAllAsync(options, token);
        }

        public Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
        {
            return _movieRepository.GetByIdAsync(id, userId, token);
        }

        public Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
        {
            return _movieRepository.GetBySlugAsync(slug, userId, token);
        }

        public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken token = default)
        {
            await _movieValidator.ValidateAndThrowAsync(movie);
            var movieExists = await _movieRepository.ExistsAsync(movie.Id, token);

            if (!movieExists)
            {
                return null;
            }

            await _movieRepository.UpdateAsync(movie, token);

            if (!userId.HasValue)
            {
                var rating = await _ratingRepository.GetRatingAsync(movie.Id, token);
                movie.Rating = rating;
                return movie;
            }

            var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId, token);
            movie.Rating = ratings.Rating;
            movie.UserRating = ratings.UserRating;
            return movie;
        }
    }
}
