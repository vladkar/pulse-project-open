using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using City.Snapshot.Snapshot;

namespace City.Snapshot.Serialization
{
    public class PulseSnapshotBinarySerializer : IPulseSnapshotSerializer
    {
        private IDictionary<int, ControlInfo> _controls;

        public PulseSnapshotBinarySerializer(IDictionary<int, ControlInfo> controls)
        {
            _controls = controls;
        }

        public IDictionary<int, ISnapshot> DeserializeSnapshot(byte[] bytesnapshot)
        {
            using (var sr = new BinaryReader(new MemoryStream(bytesnapshot)))
            {
                // Header
                var count = sr.ReadByte();

                var snapshot = new Dictionary<int, ISnapshot>();

                for (var i = 0; i < count; i++)
                {
                    // key
                    var id = sr.ReadByte();
                    // len
                    var len = sr.ReadInt32();
                    // body
                    var reader = ControlFactory.GetReader(_controls[id]);
                    snapshot[id] = reader.DeserializeSnapshot(sr);
                }

                return snapshot;
            }
        }

        public byte[] SerializeSnapshot(IDictionary<int, ISnapshot> snapshot)
        {
            using (var commandStream = new MemoryStream())
            {
                using (var sw = new BinaryWriter(commandStream))
                {
                    // Header
                    sw.Write((byte)snapshot.Count);

                    foreach (var control in snapshot)
                    {
                        var writer = ControlFactory.GetReader(_controls[control.Key]);
                        var byteControlSnapshot = writer.SerializeSnapshot(control.Value);

                        // Key
                        sw.Write((byte) control.Key);
                        // Len
                        sw.Write(byteControlSnapshot.Length);
                        // Body
                        sw.Write(byteControlSnapshot);
                    }
                }

                return commandStream.ToArray();
            }
        }

        public IDictionary<int, ICommandSnapshot[]> DeserializeCommand(byte[] bytecommand)
        {
            using (var sr = new BinaryReader(new MemoryStream(bytecommand)))
            {
                // Header
                var count = sr.ReadByte();

                

                var command = new Dictionary<int, ICommandSnapshot[]>();

                for (var i = 0; i < count; i++)
                {
                    // cmds per control
                    var controlCmdsCount = sr.ReadByte();
                    // key
                    var id = sr.ReadByte();
                    command[id] = new ICommandSnapshot[controlCmdsCount];
                    for (var j = 0; j < controlCmdsCount; j++)
                    {

                        // len
                        var len = sr.ReadByte();

                        if (len == 0)
                            continue;

                        // body
                        var reader = ControlFactory.GetReader(_controls[id]);
                        

                        command[id][j] = reader.DeserializeCommand(sr);
                    }
                }

                return command;
            }
        }

        public byte[] SerializeCommand(IDictionary<int, ICommandSnapshot[]> commands)
        {
            using (var commandStream = new MemoryStream())
            {
                using (var sw = new BinaryWriter(commandStream))
                {
                    // Header
                    sw.Write((byte) commands.Count);

                    foreach (var controlCmds in commands)
                    {
                        if (controlCmds.Value == null || controlCmds.Value.All(cmd => cmd == null))
                        {
                            sw.Write(0);
                            continue;
                        };

                        var writer = ControlFactory.GetReader(_controls[controlCmds.Key]);
                        
                        // cmds per control
                        sw.Write((byte) controlCmds.Value.Count(cmd => cmd != null));
                        // Key
                        sw.Write((byte)controlCmds.Key);
                        foreach (var cmd in controlCmds.Value.Where(cmd => cmd != null))
                        {
                            var byteControlCommand = writer.SerializeCommand(cmd);

                            // Len
                            sw.Write((byte)byteControlCommand.Length);
                            // Body
                            sw.Write(byteControlCommand);
                        }
                    }
                }

                return commandStream.ToArray();
            }
        }
    }
}