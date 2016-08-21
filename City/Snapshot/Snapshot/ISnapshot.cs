using System.Collections.Generic;

namespace City.Snapshot.Snapshot
{
    public interface ISnapshot
    {
        int Number { set; get; }
        IDictionary<byte, ISnapshotExtension> Extensions { set; get; } 
    }
}