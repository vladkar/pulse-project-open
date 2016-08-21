using System.Collections.Generic;
using City.Snapshot.Snapshot;
using DistributedTraffic.Emergency;

namespace City.Snapshot.TrafficAgent
{
    public class TrafficSnapshot: ISnapshot
    {
        public IList<TrafficAgentData> Agents { set; get; }
        public IList<TrafficEdgeData> Edges { set; get; }
        public TrafficEmcPathData EmcPaths { set; get; }
        public IList<TrafficCallData> Calls { set; get; }
        public IList<TrafficSelectData> SelectLines { set; get; }
        public EmcStatictics emc_Stats { set; get; }
        public int Number { get; set; }
        public IDictionary<byte, ISnapshotExtension> Extensions { get; set; }
        public int IterationCount { get; set; }

    }
}
