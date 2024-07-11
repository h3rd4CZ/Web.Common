using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RhDev.Common.Web.Core.Utils;

namespace RhDev.Common.Web.Core.Impl.Utils
{
    public class CorrelationService : ICorrelationService
    {
        private readonly IServiceProvider serviceProvider;

        public CorrelationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        public string? CurrentCorrelationId
        {
            get
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();

                    if (httpContextAccessor is not null)
                    {
                        return httpContextAccessor.HttpContext?.TraceIdentifier;
                    }
                    else return string.Empty;
                }
            }
        }
    }
}
