using System;

namespace Pulse.Common.Utils.Interval
{
    public abstract class Interval<T>
    {
        public T First { set; get; }
        public T Second { set; get; }
        public bool IncludeFirst { set; get; }
        public bool IncludeSecond { set; get; }

        //TODO
        public T Discreteness { set; get; }

        /// <summary>
        /// Initialize new point interval
        /// </summary>
        /// <param name="first"></param>
        public Interval(T first)
        {
            First = first;
            Second = first;
            IncludeFirst = true;
            IncludeSecond = true;
        }


        /// <summary>
        /// Initialize new interval
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="includeFirst"></param>
        /// <param name="includeSecond"></param>
        protected Interval(T first, T second, bool includeFirst = true, bool includeSecond = false)
        {
            First = first;
            Second = second;
            IncludeFirst = includeFirst;
            IncludeSecond = includeSecond;
        }

        public virtual T GetRandom()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            var fIncl = IncludeFirst ? "[" : "(";
            var sIncl = IncludeSecond ? "]" : ")";

            return string.Format("{0} {1} - {2} {3}", fIncl, First.ToString(), Second.ToString(), sIncl);
        }
    }
}
