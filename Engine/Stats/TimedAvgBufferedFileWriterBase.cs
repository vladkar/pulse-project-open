using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.MultiagentEngine.Stats
{
    public abstract class TimedAvgBufferedFileWriterBase<V> : TimedBufferedFileWriterBase<V>
    {
        public int MeanWindowWidth { get; }

        List<Pair<double, V>> Window = new List<Pair<double, V>>();

        public TimedAvgBufferedFileWriterBase(string filename, int meanWindowWidth = 1, int bufferInMemorySize = 1024)
            : base(filename, bufferInMemorySize)
        {
            if (meanWindowWidth <= 1) throw new ArgumentException("meanWindowWidth <= 1");
            if (meanWindowWidth > bufferInMemorySize) throw new ArgumentException("meanWindowWidth > bufferInMemorySize");

            MeanWindowWidth = meanWindowWidth;
        }

        protected abstract V CalcAverage(IEnumerable<V> vals);

        public override void Update(SimulationEngine engine)
        {
            Window.Add(new Pair<double, V>(engine.World.Time, GetCurrentValue(engine)));

            if (Window.Count == MeanWindowWidth)
            {
                Buffer.Add(new Pair<double, V>(Window[0].First, CalcAverage(Window.Select(pair => pair.Second))));
                Window.Clear();
            }

            if (BufferInMemorySize == 0 || Buffer.Count == BufferInMemorySize)
            {
                WriteAndClearBufferHelper();
            }

        }

        public virtual void Finalization()
        {
            // write data to file
            if (Window.Count > 0)
            {
                Buffer.Add(new Pair<double, V>(Window[0].First, CalcAverage(Window.Select(pair => pair.Second))));
                Window.Clear();
            }

            base.Finalization();
        }
    }
}