using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Repositories
{
    public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

        public async Task<bool> CreateAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            var result = await connection.ExecuteAsync("""
                insert into movies (Id, Slug, Title, YearOfRelease)
                values (@Id, @Slug, @Title, @YearOfRelease)
            """, movie);

            if (result > 0)
            {
                foreach (var genre in movie.Genres)
                {
                    await connection.ExecuteAsync("""
                        insert into genres (MovieId, Name)
                        values (@MovieId, @Name)
                    """, new { MovieId = movie.Id, Name = genre });
                }
            }

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from genres where MovieId = @Id
            """, new { id }));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from movies where Id = @Id
            """, new { id }));

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            return await connection.ExecuteScalarAsync<bool>("""
                select exists(select 1 from movies where id = @id)
            """, new { id });
        }

        public async Task<IEnumerable<Movie>> GetAllAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var result = await connection.QueryAsync<Movie, string, Movie>(
                new CommandDefinition("""
                    select m.*, g.name as Genre
                    from movies m
                    left join genres g on g.MovieId = m.Id
                 """),
                (movie, genre) =>
                {
                    movie.Genres.Add(genre);
                    return movie;
                },
                splitOn: "Genre"
            );

            if (result is null)
            {
                return Enumerable.Empty<Movie>();
            }

            return result;
        }


        public async Task<Movie?> GetByIdAsync(Guid id)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>("""
                select * from Movies where Id = @Id
            """, new { id});

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                select name from Genres where MovieId = @id
            """, new { id }));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<Movie?> GetBySlugAsync(string slug)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>("""
                select * from Movies where Slug = @slug
            """, new { slug });

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>("""
                select name from Genres where MovieId = @id
            """, new { movie.Id });

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from genres where MovieId = @Id
            """, new { movie.Id }));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (MovieId, Name)
                    values (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                update movies set
                    Title = @Title,
                    YearOfRelease = @YearOfRelease,
                    Slug = @Slug
                where Id = @Id
            """, movie));

            transaction.Commit();
            return result > 0;
        }
    }
}
