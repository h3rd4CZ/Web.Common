using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhDev.Common.Web.Core.Utils
{
    public class MeasureTime : IDisposable
    {
        private readonly Action<Stopwatch> swFormatter;
        readonly Stopwatch sw = new Stopwatch();

        private MeasureTime(Action<Stopwatch> swFormatter = default!)
        {
            this.swFormatter = swFormatter;

            sw.Start();
        }

        public static MeasureTime Start(Action<Stopwatch> swFormatter)
        {
            return new MeasureTime(swFormatter);
        }

        public void Dispose()
        {
            sw.Stop();

            swFormatter(sw);
        }
    }
}
