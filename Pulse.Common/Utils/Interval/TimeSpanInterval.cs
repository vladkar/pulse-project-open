using System;

namespace Pulse.Common.Utils.Interval
{
    public class TimeSpanInterval : Interval<TimeSpan>
    {
        public TimeSpanInterval(TimeSpan first) : base(first) { }

        public TimeSpanInterval(string first) : this(TimeSpan.Parse(first)) { }

        public TimeSpanInterval(TimeSpan first, TimeSpan second, bool includeFirst = true, bool includeSecond = false)
            : base(first, second, includeFirst, includeSecond)
        {
        }

        public TimeSpanInterval(string first, string second, bool includeFirst = true, bool includeSecond = false)
            : this(TimeSpan.Parse(first), TimeSpan.Parse(second), includeFirst, includeSecond)
        {
        }

        public override TimeSpan GetRandom()
        {
            var min = IncludeFirst ? First.TotalMinutes : First.TotalMinutes + 1;
            var max = IncludeSecond ? Second.TotalMinutes : Second.TotalMinutes - 1;
            var randomMinute = new Random().Next((int)min, (int)(max + 1));
            return new TimeSpan(hours: 0, minutes: randomMinute, seconds: 0);
        }
    }
}