using System;

namespace Pulse.Common.Utils.Interval
{
    public class MinuteInterval : Interval<int>
    {
        public MinuteInterval(int first)
            : base(first)
        {
        }

        public MinuteInterval(int first, int second, bool includeFirst = true, bool includeSecond = false)
            : base(first, second, includeFirst, includeSecond)
        {
        }

        public override int GetRandom()
        {
            var min = IncludeFirst ? First : First + 1;
            var max = IncludeSecond ? Second : Second - 1;
            return new Random().Next(min, max + 1);
        }
    }
}