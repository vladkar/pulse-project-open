using System.Collections.Generic;

namespace City.Snapshot.Snapshot
{
    public class CommandSnapshot : ICommandSnapshot
    {
        public IDictionary<byte, ISnapshotExtension> Extensions { get; set; }
        public int Number { get; set; }

        public string Command { get; set; }
        public string[] Args { get; set; }
    }
}