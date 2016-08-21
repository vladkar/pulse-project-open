using System.IO;
using City.Snapshot.Snapshot;

namespace City.Snapshot.Serialization
{
    public interface IPulseSnapshotControlSerializer
    {
        // ISnapshot DeserializeSnapShot(byte[] rawsnapshot);
        ISnapshot DeserializeSnapshot(BinaryReader sr);
        byte[] SerializeSnapshot(ISnapshot snapshot);

        ICommandSnapshot DeserializeCommand(BinaryReader sr);
        byte[] SerializeCommand(ICommandSnapshot snapshot);
    }
}