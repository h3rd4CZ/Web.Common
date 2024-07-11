namespace RhDev.Common.Web.Core.Impl
{
    public class CentralClockProvider : ICentralClockProvider
    {
        public CentralClockProvider()
        {

        }

        public CentralClock Now() => CreateClock(DateTime.Now);
        public CentralClock UtcNow() => CreateClock(DateTime.UtcNow);

        public long UtcUnixSecondsNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private CentralClock CreateClock(DateTime dtm)
        {
            var clock = new CentralClock();

            clock.FillFrom(dtm);

            return clock;
        }

        public static ICentralClockProvider Get => new CentralClockProvider();
    }
}
