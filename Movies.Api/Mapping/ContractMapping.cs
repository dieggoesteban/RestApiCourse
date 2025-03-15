using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping
{
    public static class ContractMapping
    {
        public static Movie MapToMovie(this CreateMovieRequest request)
        {
            return new Movie()
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                Genres = request.Genres.ToList()
            };
        }

        public static MovieResponse MapToMovieResponse(this Movie movie)
        {
            return new MovieResponse()
            {
                Genres = movie.Genres,
                Id = movie.Id,
                Rating = movie.Rating,
                UserRating = movie.UserRating,
                Slug = movie.GetSlug(),
                Title = movie.Title,
                YearOfRelease = movie.YearOfRelease
            };
        }

        public static MoviesResponse MapToMoviesResponse(this IEnumerable<Movie> movies)
        {
            return new MoviesResponse()
            {
                Items = movies.Select(m => m.MapToMovieResponse()).ToList()
            };
        }

        public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
        {
            return new Movie()
            {
                Id = id,
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                Genres = request.Genres.ToList()
            };
        }

        public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request)
        {
            return new GetAllMoviesOptions
            {
                Title = request.Title,
                YearOfRelease = request.YearOfRelease,
                SortField = request.SortBy?.Trim('+', '-'),
                SortOrder = request.SortBy is null 
                    ? SortOrder.Unsorted :
                        request.SortBy.StartsWith('-') 
                        ? SortOrder.Descending 
                        : SortOrder.Ascending
            };
        }

        public static GetAllMoviesOptions WithUser(this GetAllMoviesOptions options, Guid? userId)
        {
            options.UserId = userId;
            return options;
        }
    }
}
