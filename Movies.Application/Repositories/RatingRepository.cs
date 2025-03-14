using Dapper;
using Movies.Application.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        public RatingRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            return await connection.ExecuteScalarAsync<float?>(new CommandDefinition("""
                select round(avg(r.rating, 1)) from Ratings r where MovieId = @movieId
            """, new { movieId }, cancellationToken: token));
        }

        public async Task<(float? Rating, int? UserRating)> GetRatingAsync(Guid movieId, Guid? userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
                select round(avg(rating, 1)),
                    select (select rating from Ratings where MovieId = @movieId and UserId = @userId limit 1)
                from Ratings where MovieId = @movieId
            """, new { movieId, userId }, cancellationToken: token));
        }
    }
}
