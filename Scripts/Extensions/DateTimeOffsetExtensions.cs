using System;


namespace UniJSON
{
    public static class DateTimeOffsetExtensions
    {
        public const long TicksPerSecond = 10000000;
        public readonly static DateTimeOffset EpocTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
#if !NET_4_6 && !NET_STANDARD_2_0
        public static long ToUnixTimeSeconds(this DateTimeOffset now)
        {
            if (now < EpocTime)
            {
                throw new ArgumentOutOfRangeException();
            }
            return (now - EpocTime).Ticks / TicksPerSecond;
        }
#endif
    }
}
