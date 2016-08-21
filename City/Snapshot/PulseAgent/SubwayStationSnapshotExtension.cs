using City.Snapshot.Snapshot;

namespace City.Snapshot.PulseAgent
{
    public class SubwayStationSnapshotExtension : ISnapshotExtension
    {
        public byte Id { get; set; }
        public byte[] Platforms { set; get; }
    }
}