using System;
using System.Collections.Concurrent;

namespace Pulse.Common.Utils
{
    public class FixedSizedQueue<T>
    {
        private ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private static readonly Object myLock = new Object();

        public int Limit { get; }

        public FixedSizedQueue(int limit)
        {
            Limit = limit;
        }

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
            lock (myLock)
            {
                T overflow;
                while (_queue.Count > Limit && _queue.TryDequeue(out overflow)) ;
            }
        }

        public bool TryPeek(out T result)
        {
            return _queue.TryPeek(out result);
        }
    }
}