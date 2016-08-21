using System.Collections.Generic;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Environment
{
    public class PulseMapData : IMapData, IPluginable
    {
        public IDictionary<int, PulseLevel> Levels { set; get; }
        public RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> Graph { set; get; }
        public PulseMapInfo MapInfo { set; get; }

        public PluginsContainer PluginsContainer { get; set; }

        public PulseMapData(IDictionary<int, PulseLevel> levels, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> graph, PulseMapInfo mapInfo)
            : this()
        {
            Levels = levels;
            Graph = graph;
            MapInfo = mapInfo;
        }

        public PulseMapData()
        {
            Levels = new Dictionary<int, PulseLevel>();
        }
    }

    public class PulseMapInfo : IMapInfo
    {
        public GeoCoords MinGeo { get { return _mapConfig.MinGeo; } }
        public GeoCoords MaxGeo { get { return _mapConfig.MaxGeo; } }
        public PulseVector2 MinMap { get { return _mapConfig.Min; } }
        public PulseVector2 MaxMap { get { return _mapConfig.Max; } }
        public double MetersPerMapUnit { get { return _mapConfig.MetersPerMapUnit; } }
        private readonly GeoMapConfig _mapConfig;

        public PulseMapInfo(GeoMapConfig mapConfig)
        {
            _mapConfig = mapConfig;
        }

        public GeoMapConfig GetMapConfig()
        {
            return _mapConfig;
        }
    }
}
