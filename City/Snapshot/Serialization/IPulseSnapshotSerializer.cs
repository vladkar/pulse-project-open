using System.Collections.Generic;
using City.Snapshot.Snapshot;

namespace City.Snapshot.Serialization
{
    public interface IPulseSnapshotSerializer
    {
        IDictionary<int, ISnapshot> DeserializeSnapshot(byte[] bytesnapshot);
        byte[] SerializeSnapshot(IDictionary<int, ISnapshot> snapshot);

        IDictionary<int, ICommandSnapshot[]> DeserializeCommand(byte[] bytecommand);
        byte[] SerializeCommand(IDictionary<int, ICommandSnapshot[]> commands);
    }
}