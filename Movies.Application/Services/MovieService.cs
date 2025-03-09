using Movies.Application.Models;
using Movies.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Services
{
    public class MovieService(Repositories.IMovieRepository movieRepository) : IMovieService
    {
        private readonly Repositories.IMovieRepository _movieRepository = movieRepository;

        public Task<bool> CreateAsync(Movie movie)
        {
            return _movieRepository.CreateAsync(movie);
        }

        public Task<bool> DeleteByIdAsync(Guid id)
        {
            return _movieRepository.DeleteByIdAsync(id);
        }

        public Task<IEnumerable<Movie>> GetAllAsync()
        {
            return _movieRepository.GetAllAsync();
        }

        public Task<Movie?> GetByIdAsync(Guid id)
        {
            return _movieRepository.GetByIdAsync(id);
        }

        public Task<Movie?> GetBySlugAsync(string slug)
        {
            return _movieRepository.GetBySlugAsync(slug);
        }

        public async Task<Movie?> UpdateAsync(Movie movie)
        {
            var movieExists = await _movieRepository.ExistsAsync(movie.Id);

            if (!movieExists)
            {
                return null;
            }

            await _movieRepository.UpdateAsync(movie);
            return movie;
        }
    }
}
