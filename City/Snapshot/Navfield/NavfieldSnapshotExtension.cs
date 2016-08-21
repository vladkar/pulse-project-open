using City.Snapshot.Snapshot;
using Pulse.MultiagentEngine.Map;

namespace City.Snapshot.Navfield
{
    public class NavfieldSnapshotExtension : ISnapshotExtension
    {
        public byte Id { get; set; }
        public PulseVector2 BottomLeft { set; get; }
        public byte Level { set; get; }
        public double Size { set; get; }
        public double[][] Grid { set; get; }
    }
}