using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using City.Snapshot.Serialization;
using City.Snapshot.Snapshot;
using Pulse.Common.Model.Environment.Map;
using Pulse.Model.Environment;
using Pulse.Scenery.SubwayStation.Abstract.Plugin;

namespace City.Snapshot.PulseAgent
{
    public class SubwayStationSnapshotExtensionSerializer : IPulseSnapshotControlExtensionSerializer
    {
        public ISnapshotExtension Deserialize(BinaryReader br)
        {
            var subwayExt = new SubwayStationSnapshotExtension();

            var platformCount = br.ReadInt32();
            subwayExt.Platforms = new byte[platformCount];

            for (int i = 0; i < platformCount; i++)
            {
                subwayExt.Platforms[i] = br.ReadByte();
            }

            return subwayExt;
        }

        public void Serialize(ISnapshotExtension extension, BinaryWriter sw)
        {
            var subwayExt = extension as SubwayStationSnapshotExtension;
            if (subwayExt == null)
                throw new Exception("Wrong snapshot exntension");

            sw.Write(subwayExt.Platforms.Length);
            foreach (var platform in subwayExt.Platforms)
            {
                sw.Write(platform);
            }
        }

        public ISnapshotExtension GetSnapshotExtension(IPulseMap map)
        {
            var platforms = ((map as PulseMap).PluginsContainer.Plugins["SubwayStation"] as SubwayStationPluginMap).Platforms;
            var subwayExtension = new SubwayStationSnapshotExtension
            {
                Platforms = platforms.Select(p => (byte) p.State).ToArray()
            };

            return subwayExtension;
        }
    }
}