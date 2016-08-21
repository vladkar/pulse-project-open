using System;
using System.Collections.Generic;
using System.IO;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Plugin.SimpleInfection.Body;

namespace City.Snapshot.Infection
{
    public class InfectionSnapshotExtensionSerializer : IPulseSnapshotControlExtensionSerializer
    {
        public ISnapshotExtension Deserialize(BinaryReader br)
        {
            var ext = new InfectionSnapshotExtension {};

            ext.Count = br.ReadInt32();
            ext.AgentsInfection = new List<byte>();
            
            for (int i = 0; i < ext.Count; i++)
            {
                ext.AgentsInfection.Add(br.ReadByte());
            }

            return ext;
        }

        public void Serialize(ISnapshotExtension extension, BinaryWriter sw)
        {
            var ext = extension as InfectionSnapshotExtension;
            if (ext == null) throw new Exception("Wrong snapshot exntension");

            sw.Write(ext.Count);

            for (int i = 0; i < ext.Count; i++)
            {
                sw.Write(ext.AgentsInfection[i]);
            }
        }

        public ISnapshotExtension GetSnapshotExtension(IPulseMap map)
        {
            var ext = new InfectionSnapshotExtension {AgentsInfection = new List<byte>()};

            foreach (var agent in map.AgentRegistry)
            {
                ext.AgentsInfection.Add((byte)((agent as AbstractPulseAgent).PluginsContainer.Plugins["SimpleInfection"] as SimpleInfectionPluginAgent).InfectionStage.InfectionState);
            }

            ext.Count = ext.AgentsInfection.Count;

            return ext;
        }
    }
}