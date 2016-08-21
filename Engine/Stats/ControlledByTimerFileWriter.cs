using System;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.MultiagentEngine.Stats
{
    public abstract class ControlledByTimerFileWriter<V> : TimedFileWriterBase<V>
    {
        private TimeSpan RealTimeInterval { get; }
        private DateTime _lastFetched = DateTime.Now;

        public ControlledByTimerFileWriter(string filename, TimeSpan interval)
            : base(filename)
        {
            RealTimeInterval = interval;
        }

        public override void Update(SimulationEngine engine)
        {
            if(DateTime.Now - _lastFetched >= RealTimeInterval)
            {
                base.Update(engine);
                _lastFetched = DateTime.Now;
            }
        }
    }
}