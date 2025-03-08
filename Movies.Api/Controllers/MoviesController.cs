using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;

        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
        {
            Movie movie = request.MapToMovie();            
            var result = await _movieRepository.CreateAsync(movie);
            var response = new MovieResponse()
            { 
                Genres = movie.Genres, 
                Id = movie.Id, 
                Slug = movie.GetSlug(),
                Title = movie.Title, 
                YearOfRelease = movie.YearOfRelease 
            };

            return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie);
        }

        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<IActionResult> Get([FromRoute] string idOrSlug)
        {
            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieRepository.GetByIdAsync(id)
                : await _movieRepository.GetBySlugAsync(idOrSlug);
                
            if (movie is null)
            {
                return NotFound();
            }

            var response = movie.MapToMovieResponse();
            return Ok(response);
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var movies = await _movieRepository.GetAllAsync();
            var response = movies.MapToMoviesResponse();
            return Ok(response);
        }

        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
        {
            var movie = request.MapToMovie(id);
            var updatedMovie = await _movieRepository.UpdateAsync(movie);

            if (!updatedMovie)
            {
                return NotFound();
            }

            var response = movie.MapToMovieResponse();
            return Ok(response);
        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deleted = await _movieRepository.DeleteByIdAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
