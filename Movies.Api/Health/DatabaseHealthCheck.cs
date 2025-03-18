using Microsoft.Extensions.Diagnostics.HealthChecks;
using Movies.Application.Database;

namespace Movies.Api.Health
{
    public class DatabaseHealthCheck(IDbConnectionFactory connectionFactory,
        ILogger<DatabaseHealthCheck> logger) : IHealthCheck
    {
        public const string Name = "Database";

        private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
        private readonly ILogger<DatabaseHealthCheck> _logger = logger;
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
			try
			{
                _ = await _connectionFactory.CreateConnectionAsync(cancellationToken);
                return HealthCheckResult.Healthy();
			}
			catch (Exception ex)
			{
                const string errorMessage = "Database is unhealthy";
                _logger.LogError(ex, errorMessage);
                return HealthCheckResult.Unhealthy(errorMessage, ex);
			}
        }
    }
}
