using System;
using System.Collections.Generic;
using System.IO;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Agent;

namespace City.Snapshot.PulseAgent
{
    public class PulseAgentBinarySerializer : IPulseSnapshotControlSerializer
    {
        private ControlInfo _controlConfig;
        private SnapshotUtil _snapshotExtensionUtil;

        public PulseAgentBinarySerializer(ControlInfo controlConfig)
        {
            _controlConfig = controlConfig;
            _snapshotExtensionUtil = new SnapshotUtil(_controlConfig);
        }

        public void WriteAgent(BinaryWriter sw, IPulseAgentData agent)
        {
            sw.Write(agent.Id);
            sw.Write(agent.X);
            sw.Write(agent.Y);
            sw.Write(agent.Level);

            //todo to extentions
            var a = agent as ISfAgent;
            sw.Write(a.Pressure);
            sw.Write(a.StepDist);
            sw.Write(a.Angle);
            sw.Write(a.ForceX);
            sw.Write(a.ForceY);
            sw.Write(a.Role);
        }

        public ISnapshot DeserializeSnapshot(BinaryReader sr)
        {
            var snapshot = new PulseSnapshot();
            if (sr.BaseStream.Length == 0)
            {
                snapshot.Agents = new IPulseAgentData[0];
                return snapshot;
            };

            snapshot.Number = sr.ReadInt32();
            snapshot.Time = new DateTime(sr.ReadInt64());
            var count = sr.ReadInt32();

            snapshot.Agents = new List<IPulseAgentData>();
            for (var i = 0; i < count; i++)
            {
                snapshot.Agents.Add(ReadAgent(sr));
            }

            var extensionCount = sr.ReadInt32();
            snapshot.Extensions = new Dictionary<byte, ISnapshotExtension>();
            for (int i = 0; i < extensionCount; i++)
            {
                var extensionId = sr.ReadByte();
                snapshot.Extensions[extensionId] = _snapshotExtensionUtil.ReadSnapshotExtension[extensionId](sr);
            }

            return snapshot;
        }

        public byte[] SerializeSnapshot(ISnapshot snapshot)
        {
            var sht = snapshot as PulseSnapshot;
            if (sht?.Agents == null) return new byte[0];

            var ms = new MemoryStream();
            var sw = new BinaryWriter(ms);
            
            sw.Write(sht.Number);
            sw.Write(sht.Time.Ticks);
            sw.Write(sht.Agents.Count);

            foreach (var agent in sht.Agents)
            {
                WriteAgent(sw, agent);
            }
            
            sw.Write(snapshot.Extensions.Count);
            foreach (var extension in snapshot.Extensions)
            {
                sw.Write(extension.Key);
                _snapshotExtensionUtil.WriteSnapshotExtension[extension.Key](extension.Value, sw);
            }

            return ms.ToArray();
        }

        public ICommandSnapshot DeserializeCommand(BinaryReader sr)
        {
            if (sr.BaseStream.Length == 0) return null;

            var command = new CommandSnapshot();

            command.Command = sr.ReadString();

            List<string> list = new List<string>();

            //TODO string table
            var argCount = sr.ReadByte();

            for (int i = 0; i < argCount; i++)
            {
                list.Add(sr.ReadString());
            }

//            while (sr.PeekChar() != -1)

            command.Args = list.ToArray();
            
            return command;
        }

        public byte[] SerializeCommand(ICommandSnapshot snapshot)
        {
            var sht = snapshot as CommandSnapshot;
            if (sht == null) return new byte[0];

            var ms = new MemoryStream();
            var sw = new BinaryWriter(ms);

            sw.Write(sht.Command);
            sw.Write((byte)sht.Args.Length);

            foreach (var cmd in sht.Args)
                sw.Write(cmd);



            //            sw.Write(snapshot.Extensions.Count);
            //            foreach (var extension in snapshot.Extensions)
            //            {
            //                sw.Write(extension.Key);
            //                _snapshotExtensionUtil.WriteSnapshotExtension[extension.Key](extension.Value, sw);
            //            }

            return ms.ToArray();
        }

        public ISnapshot DeserializeSnapshot(byte[] rawsnapshot)
        {
            using (var sr = new BinaryReader(new MemoryStream(rawsnapshot)))
                return DeserializeSnapshot(sr);
        }

        public IPulseAgentData ReadAgent(BinaryReader sr)
        {
            //todo to extentions
            return new SfAgent
            {
                Id = sr.ReadInt32(),
                X = sr.ReadDouble(),
                Y = sr.ReadDouble(),
                Level = sr.ReadByte(),
                Pressure = sr.ReadDouble(),
                StepDist = sr.ReadDouble(),
                Angle = sr.ReadInt16(),
                ForceX = sr.ReadDouble(),
                ForceY = sr.ReadDouble(),
                Role = sr.ReadByte()
            };

//            return new PulseAgentData
//            {
//                Id = sr.ReadInt32(),
//                X = sr.ReadDouble(),
//                Y = sr.ReadDouble()
//            };
        }

        private Tuple<int, int> NormalizeGeoCoord(double x, double y, double x_min, double y_min, double x_max, double y_max)
        {
            var n_x = (x - x_min) / (x_max - x_min);
            var n_y = (y - y_min) / (y_max - y_min);

            return new Tuple<int, int>(GetFractionAsCodedInt(n_x), GetFractionAsCodedInt(n_y));
        }


        private int GetFractionAsCodedInt(double num)
        {
            var codedFraction = num * 0xffffff;
            return (int)codedFraction;
        }
    }
}