using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.Snapshot.Infection;
using City.Snapshot.PulseAgent;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Environment.Map;
using Pulse.Model.Environment;
using Pulse.Scenery.SubwayStation.Abstract.Plugin;

namespace City.Snapshot
{
    public class SnapshotUtil
    {
        public IDictionary<byte, IPulseSnapshotControlExtensionSerializer> ExtensionRegister { get; private set; } 

        public IDictionary<byte, Func<BinaryReader, ISnapshotExtension>> ReadSnapshotExtension { get; private set; }
        public Dictionary<byte, Action<ISnapshotExtension, BinaryWriter>> WriteSnapshotExtension { get; private set; }
        
        public ControlInfo ControlConfig { get; private set; }

        public SnapshotUtil(ControlInfo ci)
        {
            ControlConfig = ci;
            ConfigExtensions();
        }

        private void ConfigExtensions()
        {
            ReadSnapshotExtension = new Dictionary<byte, Func<BinaryReader, ISnapshotExtension>>();
            WriteSnapshotExtension = new Dictionary<byte, Action<ISnapshotExtension, BinaryWriter>>();

            ExtensionRegister = new Dictionary<byte, IPulseSnapshotControlExtensionSerializer>();

            if (ControlConfig.Scenario.ToLower() == "novokrest")
            {
                const byte id = 0;

                var stationExtensionSerializer = new SubwayStationSnapshotExtensionSerializer();
                ReadSnapshotExtension[id] = (br) => stationExtensionSerializer.Deserialize(br);
                WriteSnapshotExtension[id] = (extension, bw) => stationExtensionSerializer.Serialize(extension, bw);
                ExtensionRegister[id] = stationExtensionSerializer;
            }

            if (ControlConfig.Scenario.ToLower().Contains("train"))
            {
                const byte ieid = 1;
                var ies = new InfectionSnapshotExtensionSerializer();

                ReadSnapshotExtension[ieid] = (br) => ies.Deserialize(br);
                WriteSnapshotExtension[ieid] = (extension, bw) => ies.Serialize(extension, bw);
                ExtensionRegister[ieid] = ies;
            }

//            if(ControlConfig.Scenario.ToLower() == "harsiddhitemple"
//                | ControlConfig.Scenario.ToLower() == "mahakaltemple")
//            {
//                const byte id = 1;
//
//                var stationExtensionSerializer = new NavfieldSnapshotExtensionSerializer();
//                ReadSnapshotExtension[id] = (br) => stationExtensionSerializer.Deserialize(br);
//                WriteSnapshotExtension[id] = (extension, bw) => stationExtensionSerializer.Serialize(extension, bw);
//                ExtensionRegister[id] = stationExtensionSerializer;
//            }
        }

        public IDictionary<byte, ISnapshotExtension> GetExtensions(IPulseMap map)
        {
            return ExtensionRegister.ToDictionary(k => k.Key, v => v.Value.GetSnapshotExtension(map));
        }
    }
}
