using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public static class PathCalc
    {
        public class CalcTask
        {
            public Action Action { set; get; }
            public BehaviorState State { set; get; }
            public long Id { set; get; }
        }

        private static long _count;
        private static ConcurrentQueue<CalcTask> _queue = new ConcurrentQueue<CalcTask>();
        private static Task _worker;

        static PathCalc()
        {
            _worker = new Task(Update);
            _worker.Start();
        }

        private static void Update()
        {
            while (true)
            {
                if (_queue.IsEmpty)
                    Thread.Sleep(50);

//                var d = new ConcurrentDictionary<int, int>();
//                d.Ta
                
                Console.Out.WriteLine($"----------- start: {_queue.Count} --------------");
                var sw = new Stopwatch();
                sw.Start();

                Parallel.For(0, _queue.Count,
                    new ParallelOptions() { MaxDegreeOfParallelism = 20 },
                    (i) =>
                    {
                        CalcTask t;
                        if (_queue.TryDequeue(out t))
                        {
                            t.Action.Invoke();
                            t.State = BehaviorState.Success;
                        }
                    });

                sw.Stop();
                Console.Out.WriteLine($"----------- done: {sw.Elapsed} --------------");

                //                _queue.AsParallel().ForAll(e =>
                //                {
                //                    e.Action.Invoke();
                //                    e.State = BehaviorState.Success;
                //                    closed.AddLast(e);
                //                });

                //                foreach (var task in closed)
                //                {
                //                    
                //                }
            }
        }

        public static CalcTask PushTask(Action calcPath)
        {
            var t = new CalcTask
            {
                Id = ++_count,
                State = BehaviorState.Running,
                Action = calcPath
            };

            _queue.Enqueue(t);

            return t;
        }
    }
}