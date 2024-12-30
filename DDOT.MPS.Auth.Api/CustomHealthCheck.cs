using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace DDOT.MPS.Auth.Api.HealthChecks
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly string _environmentName;

        public CustomHealthCheck(string environmentName)
        {
            _environmentName = environmentName;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var message = $"API is Up and Running in {_environmentName} environment";
            return Task.FromResult(HealthCheckResult.Healthy(message));
        }
    }
}
