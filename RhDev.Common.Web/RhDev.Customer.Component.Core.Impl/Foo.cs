using Microsoft.Extensions.Logging;

namespace RhDev.Customer.Component.Core.Impl
{
    public class Foo : IFoo
    {
        private readonly ILogger<Foo> logger;

        public Foo(ILogger<Foo> logger)
        {
            this.logger = logger;
        }

        public void Boo()
        {
            logger.LogInformation("Boo");
        }
    }
}
