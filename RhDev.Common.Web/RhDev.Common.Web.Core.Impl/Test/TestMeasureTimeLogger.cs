using System.Diagnostics;
using Xunit.Abstractions;

namespace RhDev.Common.Web.Core.Impl.Test
{
    public class TestMeasureTimeLogger : IDisposable
    {
        private readonly ITestOutputHelper testOutputHelper;
        private readonly Func<Stopwatch, string> swFormatter;
        readonly Stopwatch sw = new Stopwatch();

        private TestMeasureTimeLogger(ITestOutputHelper testOutputHelper, Func<Stopwatch, string> swFormatter = default)
        {
            this.testOutputHelper = testOutputHelper;
            this.swFormatter = swFormatter;
            sw.Start();
        }

        public static TestMeasureTimeLogger Start(ITestOutputHelper testOutputHelper, Func<Stopwatch, string> swFormatter = default)
        {
            return new TestMeasureTimeLogger(testOutputHelper, swFormatter);
        }

        public void Dispose()
        {
            sw.Stop();

            var msg = Equals(null, swFormatter) ? $"Took {sw.ElapsedMilliseconds} ms." : swFormatter(sw);

            testOutputHelper.WriteLine(msg);
        }
    }
}
