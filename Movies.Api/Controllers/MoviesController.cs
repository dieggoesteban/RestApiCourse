﻿using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _movieRepository;

        public MoviesController(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        [HttpPost("movies")]
        public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
        {
            Movie movie = request.MapToMovie();            
            var result = await _movieRepository.CreateAsync(movie);
            var response = new MovieResponse()
            { 
                Genres = movie.Genres, 
                Id = movie.Id, 
                Title = movie.Title, 
                YearOfRelease = movie.YearOfRelease 
            };

            return Created($"/api/movies/{movie.Id}", response);
        }
    }
}
