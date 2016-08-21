using System.Collections.Generic;
using System.IO;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Environment.Map;

namespace City.Snapshot.Serialization
{
    public interface IPulseSnapshotControlExtensionSerializer
    {
        ISnapshotExtension Deserialize(BinaryReader br);
        void Serialize(ISnapshotExtension extension, BinaryWriter sw);
        ISnapshotExtension GetSnapshotExtension(IPulseMap map);
    }
}