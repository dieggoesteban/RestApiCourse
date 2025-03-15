using Movies.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Repositories
{
    public interface IMovieRepository

    {
        Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
        Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default);
        Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);
        Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
