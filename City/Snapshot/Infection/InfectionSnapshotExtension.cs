using System.Collections.Generic;
using City.Snapshot.Snapshot;

namespace City.Snapshot.Infection
{
    public class InfectionSnapshotExtension : ISnapshotExtension
    {
        public byte Id { get; set; }
        public int Count { get; set; }
        public IList<byte> AgentsInfection { set; get; }
    }
}