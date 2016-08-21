using System;
using System.Collections.Concurrent;

namespace Pulse.Common.Utils
{
    public class IntervalSizedQueue<T>
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public int Min { get; }
        public int Max { get; }
        public int Count { get { return _queue.Count; } }

        private static readonly Object myLock = new Object();

        public IntervalSizedQueue(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);

            lock (myLock)
            {
                T overflow;
                while (_queue.Count > Max && _queue.TryDequeue(out overflow)) ;
            }
        }

        public bool TryDequeue(out T result)
        {
            lock (myLock)
            {
                if (_queue.Count > Min)
                    return _queue.TryDequeue(out result);
            }

            result = default(T);
            return false;
        }

        public bool TryPeek(out T result)
        {
            return _queue.TryPeek(out result);
        }

        /// <summary>
        /// Dequeue if pass count constraints, else peek
        /// </summary>
        public bool TryGetElement(out T result)
        {
            return _queue.Count > Min ? TryDequeue(out result) : TryPeek(out result);
        }
    }
}