
namespace SwarmingFleet.Common.Helpers
{
    using System;

    public static class RandomHelper
    {
        private static readonly Random s_random = new Random(Guid.NewGuid().GetHashCode());
        public static DateTime AddRandomMinutes(this DateTime dateTime, Range range)
        {
            var minute = s_random.Next(range.Start.Value, range.End.Value);
            return dateTime.AddMinutes(minute);
        }
    }
}
