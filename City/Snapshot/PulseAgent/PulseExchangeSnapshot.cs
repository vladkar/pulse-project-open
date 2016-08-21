using City.Snapshot.Snapshot;

namespace City.Snapshot.PulseAgent
{
    public class PulseExchangeSnapshot : PulseSnapshot, IExchangeSnapshot
    {
        public byte OriginServer { get; set; }
        public byte DestinationServer { get; set; }
    }
}