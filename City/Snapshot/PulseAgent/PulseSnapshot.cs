using System;
using System.Collections.Generic;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Agent;

namespace City.Snapshot.PulseAgent
{
    public class PulseSnapshot : ISnapshot
    {
        public IList<IPulseAgentData> Agents { set; get; }
        public int Number { get; set; }
        public DateTime Time { set; get; }
        public IDictionary<byte, ISnapshotExtension> Extensions { get; set; }
    }
}
