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

        public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
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

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from genres where MovieId = @Id
            """, new { id }, cancellationToken: token));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from movies where Id = @Id
            """, new { id }, cancellationToken: token));

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                select exists(select 1 from movies where id = @id)
            """, new { id }, cancellationToken: token));
        }

        public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var result = await connection.QueryAsync(
                new CommandDefinition("""
                SELECT 
                    m.*, 
                    STRING_AGG(distinct g.name, ',') AS Genres,
                    round(avg(r.rating), 1) as Rating, 
                    myr.rating as UserRating
                FROM movies m
                LEFT JOIN genres g ON g.MovieId = m.Id   
                LEFT JOIN ratings r ON r.MovieId = m.Id
                LEFT JOIN ratings myr ON myr.MovieId = m.Id and myr.UserId = @userId
             
                GROUP BY m.Id
             """, new { userId }, cancellationToken: token)
            );

            return result.Select(x => new Movie
            {
                Id = x.id,
                Title = x.title,
                YearOfRelease = x.yearofrelease,
                Rating = (float?)x.rating,
                UserRating = (int?)x.userrating,
                Genres = Enumerable.ToList(x.genres.Split(','))
            });
        }

        public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                select m.*, round(avg(r.rating), 1) as Rating, myr.rating as UserRating 
                from Movies m 
                left join Ratings r on r.MovieId = m.Id
                left join Ratings myr on myr.MovieId = m.Id and myr.UserId = @userId
                where Id = @id
                group by id, UserRating
            """, new { id, userId }, cancellationToken: token));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                select name from Genres where MovieId = @id
            """, new { id }, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            var movie = await connection.QueryFirstOrDefaultAsync<Movie>(new CommandDefinition("""
                select m.*, round(avg(r.rating), 1) as Rating, myr.rating as UserRating 
                from Movies m 
                left join Ratings r on r.MovieId = m.Id
                left join Ratings myr on myr.MovieId = m.Id and myr.UserId = @userId
                where Slug = @slug
                group by id, UserRating
            """, new { slug, userId }, cancellationToken: token));

            if (movie is null)
            {
                return null;
            }

            var genres = await connection.QueryAsync<string>(new CommandDefinition("""
                select name from Genres where MovieId = @id
            """, new { movie.Id }, cancellationToken: token));

            foreach (var genre in genres)
            {
                movie.Genres.Add(genre);
            }

            return movie;
        }

        public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from genres where MovieId = @Id
            """, new { movie.Id }, cancellationToken: token));

            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    insert into genres (MovieId, Name)
                    values (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
            }

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                update movies set
                    Title = @Title,
                    YearOfRelease = @YearOfRelease,
                    Slug = @Slug
                where Id = @Id
            """, movie, cancellationToken: token));

            transaction.Commit();
            return result > 0;
        }
    }
}
