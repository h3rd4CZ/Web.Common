using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RhDev.Common.Web.Core.Impl.Timer.Queue;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Test.Core
{
    public class BackgroudQueueTest : IntegrationTestBase
    {
        public BackgroudQueueTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        public async Task TestQueueProcessing()
        {
            var host = Host();

            var services = host.Services;

            var queue = services.GetRequiredService<IBackgroundTaskQueue>();

            var queueHostedService = services.GetRequiredService<QueuedHostedService>();

            await queueHostedService.StartAsync(CancellationToken.None);

            await queue.QueueBackgroundWorkItemAsync(new("1", async cancellation =>
            {
                await Task.Run(() => testOutputHelper!.WriteLine($"Task in queue run at : {DateTime.Now}"));
            }));

            Thread.Sleep(5000);
        }
    }
}
