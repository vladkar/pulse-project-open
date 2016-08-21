using System;
using System.Collections.Generic;

namespace Pulse.Common.Utils
{
    public static class RandomUtil
    {
        private static Random Rnd = new Random();

//        public static int Seed { set { Rnd = new Random(value);} get {return Rnd.} }

        public static int EnumerableCount<T>(this IEnumerable<T> source)
        {
            var c = source as ICollection<T>;
            if (c != null)
                return c.Count;

            var result = 0;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    result++;
            }
            return result;
        }

        public static T ProportionChoise<T>(this IEnumerable<T> source, Func<T, int> getProportionFunc)
        {
            var totalProportion = 0;
            var proportions = new List<KeyValuePair<int, T>>();
            foreach (var el in source)
            {
                var proportion = getProportionFunc(el);
                totalProportion += proportion;
                proportions.Add(new KeyValuePair<int, T>(totalProportion, el));
            }

            var estimation = NextInt(1, totalProportion + 1);

            if (source.EnumerableCount() == 1 || estimation <= proportions[0].Key)
                return proportions[0].Value;

            for (var i = 1; i < proportions.Count; i++)
            {
                if (estimation > proportions[i - 1].Key & estimation <= proportions[i].Key)
                    return proportions[i].Value;
            }

            throw new Exception("Proportion choiser exception");
        }

        public static T ProportionChoise<T>(this IEnumerable<T> source, Func<T, double> getProportionFunc)
        {
            var totalProportion = 0d;
            var proportions = new List<KeyValuePair<double, T>>();
            foreach (var el in source)
            {
                var proportion = getProportionFunc(el);
                totalProportion += proportion;
                proportions.Add(new KeyValuePair<double, T>(totalProportion, el));
            }

            var estimation = NextInt(0, (int)totalProportion) + NextDouble();

            if (source.EnumerableCount() == 1 || estimation <= proportions[0].Key)
                return proportions[0].Value;

            for (var i = 1; i < proportions.Count; i++)
            {
                if (estimation > proportions[i - 1].Key & estimation <= proportions[i].Key)
                    return proportions[i].Value;
            }

            throw new Exception("Proportion choiser exception");
        }

        public static T RandomChoise<T>(this IEnumerable<T> source)
        {
            var randomEl = NextInt(0, source.EnumerableCount());
            var count = 0;

            foreach (var el in source)
            {
                if (count == randomEl)
                    return el;
                count++;
            }

            throw new RandomUtilException("Random choiser exception");
        }

        //TODO check
        public static bool ToBeOrNot(int val, int maxval)
        {
            var estimation = NextInt(0, maxval);
            return val > estimation;
        }

        public static bool PlayProbabilityPercent(int prob)
        {
            var estimation =  NextInt(1, 101);
            return prob >= estimation;
        }

        public static bool PlayProbability(double prob)
        {
            var estimation = NextDouble();
            return prob > estimation;
        }

        public static bool ResolveProbability(double val)
        {
            var estimation = NextDouble();
            return val > estimation;
        }

        public static int RandomInt(int min, int max)
        {
            var rnd = NextInt(min, max);
            return rnd;
        }

        public static double RandomDouble()
        {
            var rnd = NextDouble();
            return rnd;
        }

        public static double RandomDouble(double min, double max)
        {
            return RandomDouble() * (max - min) + min;
        }

        private static int NextInt(int min, int max)
        {
            lock (Rnd)
            {
                return Rnd.Next(min, max);
            } 
        }

        private static double NextDouble()
        {
            lock (Rnd)
            {
                return Rnd.NextDouble();
            }
        }

        public static bool playProbabilityLess(double prob)
        {
            return (Rnd.NextDouble() <= prob);
        }
    }

    public class RandomUtilException : Exception
    {
        public RandomUtilException(string message) : base(message)
        {
        }
    }
}