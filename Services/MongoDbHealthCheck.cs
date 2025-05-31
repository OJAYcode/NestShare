using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;
using Thrift.Data;

namespace Thrift.Services
{
    public class MongoDbHealthCheck : IHealthCheck
    {
        private readonly MongoDbContext _mongoContext;

        public MongoDbHealthCheck(MongoDbContext mongoContext)
        {
            _mongoContext = mongoContext;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (_mongoContext.IsConnected)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("MongoDB connection is healthy."));
                }
                else
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy("MongoDB connection failed."));
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy($"MongoDB health check failed: {ex.Message}"));
            }
        }
    }
}
