using System.Collections.Generic;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.MultiagentEngine.Stats
{
    public abstract class TimedBufferedFileWriterBase<V> : TimedFileWriterBase<V>
    {
        public int BufferInMemorySize { get; }
        protected List<Pair<double, V>> Buffer = new List<Pair<double, V>>();

        public TimedBufferedFileWriterBase(string filename, int bufferInMemorySize = 1024) : base(filename)
        {
            if (bufferInMemorySize < 0) bufferInMemorySize = 1024;

            BufferInMemorySize = bufferInMemorySize;
        }

        public override void Update(SimulationEngine engine)
        {
            Buffer.Add(new Pair<double, V>(engine.World.Time, GetCurrentValue(engine)));

            if (BufferInMemorySize == 0 || Buffer.Count == BufferInMemorySize)
            {
                WriteAndClearBufferHelper();
            }
        }

        protected void WriteAndClearBufferHelper()
        {
            foreach (var b in Buffer)
            {
                string str = FormatValue(b);
                OutFileStream.WriteLine(str);
            }
            Buffer.Clear();
        }

        public override void Finalization()
        {
            WriteAndClearBufferHelper();
            base.Finalization();
        }
    }
}