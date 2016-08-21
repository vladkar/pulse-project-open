using System;
using System.Diagnostics;
using System.Globalization;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.MultiagentEngine.Stats
{
    public class DiagnosticsInfo
    {
        public long TotalMemoryByGarbageCollector { get; set; }
        public long PrivateMemorySize64 { get; set; }
        public float CpuUsgeInPercents { get; set; }
        public float AvailableRamInMbytes { get; set; }
    }

    public class DiagnosticsInfoControlledByTimer : ControlledByTimerFileWriter<DiagnosticsInfo>
    {
        readonly PerformanceCounter _cpuCounter;
        readonly PerformanceCounter _ramCounter;

        protected override string GetFileHeader()
        {
            return "DateTime.Now; CpuUsgeInPercents; TotalMemoryByGarbageCollector; PrivateMemorySize64; AvailableRamInMbytes\n";
        }

        public DiagnosticsInfoControlledByTimer(string filename, TimeSpan interval)
            : base(filename, interval)
        {
            _cpuCounter = new PerformanceCounter();
            _cpuCounter.CategoryName = "Processor";
            _cpuCounter.CounterName = "% Processor Time";
            _cpuCounter.InstanceName = "_Total";
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        protected override DiagnosticsInfo GetCurrentValue(SimulationEngine engine)
        {
            var ret = new DiagnosticsInfo();
            ret.TotalMemoryByGarbageCollector = GC.GetTotalMemory(false);
            Process proc = Process.GetCurrentProcess();
            ret.PrivateMemorySize64 = proc.PrivateMemorySize64;
            ret.CpuUsgeInPercents = _cpuCounter.NextValue();
            ret.AvailableRamInMbytes = _ramCounter.NextValue();
            return ret;
        }

        protected override string FormatValue(Pair<double, DiagnosticsInfo> val)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}; {1}; {2}; {3}; {4}", DateTime.Now.ToFileTime(), val.Second.CpuUsgeInPercents,
                                 val.Second.TotalMemoryByGarbageCollector, val.Second.PrivateMemorySize64,
                                 val.Second.AvailableRamInMbytes);
        }
    }
}
