using System;
using System.IO;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.MultiagentEngine.Stats
{
    public abstract class TimedFileWriterBase<V> : IEngineStatistics
    {
        public string FileName { get; }
        protected StreamWriter OutFileStream = null;

        public TimedFileWriterBase(string filename)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            FileName = filename;

            if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            OutFileStream = File.CreateText(FileName);

            OutFileStream.Write(GetFileHeader());
        }

        protected virtual string GetFileHeader()
        {
            return "";
        }

        protected abstract V GetCurrentValue(SimulationEngine engine);
        protected abstract string FormatValue(Pair<double, V> val);

        public virtual void Update(SimulationEngine engine)
        {
            string str = FormatValue(new Pair<double, V>(engine.World.Time, GetCurrentValue(engine)));
            OutFileStream.WriteLine(str);
        }

        public virtual void Finalization()
        {
            OutFileStream.Flush();
            OutFileStream.Close();
        }
    }
}