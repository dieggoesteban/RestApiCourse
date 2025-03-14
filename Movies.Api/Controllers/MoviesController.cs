using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, 
            CancellationToken token = default)
        {
            Movie movie = request.MapToMovie();            
            var result = await _movieService.CreateAsync(movie, token);
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
        public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken token = default)
        {
            var userId = HttpContext.GetUserId();

            var movie = Guid.TryParse(idOrSlug, out var id)
                ? await _movieService.GetByIdAsync(id, userId, token)
                : await _movieService.GetBySlugAsync(idOrSlug, userId, token);
                
            if (movie is null)
            {
                return NotFound();
            }

            var response = movie.MapToMovieResponse();
            return Ok(response);
        }

        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<IActionResult> GetAll(CancellationToken token = default)
        {
            var userId = HttpContext.GetUserId();

            var movies = await _movieService.GetAllAsync(userId, token);
            var response = movies.MapToMoviesResponse();
            return Ok(response);
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid id, 
            [FromBody] UpdateMovieRequest request, 
            CancellationToken token = default)
        {
            var userId = HttpContext.GetUserId();

            var movie = request.MapToMovie(id);
            var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);

            if (updatedMovie is null)
            {
                return NotFound();
            }

            var response = updatedMovie.MapToMovieResponse();
            return Ok(response);
        }

        [Authorize(AuthConstants.AdminUserPolicyName)]
        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken token = default)
        {
            var deleted = await _movieService.DeleteByIdAsync(id, token);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
