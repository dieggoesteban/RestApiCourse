using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Movies
                (
                    Id UUID PRIMARY KEY,
                    Slug TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    YearOfRelease INTEGER NOT NULL
                );
            """);

            // Index creation for slug
            await connection.ExecuteAsync("""
                CREATE UNIQUE INDEX IF NOT EXISTS Movies_Slug_Index
                ON Movies
                USING btree(slug)
            """);

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Genres
                (
                    MovieId UUID references Movies(Id),
                    Name TEXT NOT NULL
                );
            """);

            await connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Ratings
                (
                    UserId UUID,
                    MovieId UUID references Movies(Id),
                    Rating INTEGER NOT NULL,
                    PRIMARY KEY (UserId, MovieId)
                );
            """);
        }
    }
}
