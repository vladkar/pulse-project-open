using System;
using System.Linq;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Plugin.SimpleInfection;
using Pulse.Plugin.SubModel;
using Pulse.Plugin.Traffic;
using Pulse.Scenery.MovingPlatform.ComputationalPlugin;
using Pulse.Scenery.SubwayStation.Abstract.Plugin;
using Pulse.Scenery.SubwayStation.Vasileostrovskaya.ValidationExperiment.ComputationalPlugin;

namespace Pulse.Model
{
    public class PluginFactory : AbstractPulsePluginFactory
    {
        private LegendAggregator _la;

        public PluginFactory(PulseScenarioConfig config, LegendAggregator la) : base(config)
        {
            _la = la;
            CreatePluginFactories();
            InitPluginFactories();
        }

        private void CreatePluginFactories()
        {
            foreach (var plugin in Config.Plugins.Value)
            {
                PluginFactories.Add(plugin, GetPluginFactory(plugin));
            }
        }

        private void InitPluginFactories()
        {
            foreach (var pluginFactory in PluginFactories.Values)
            {
                pluginFactory.Initialize();
            }
        }

        private AbstractPluginFactory GetPluginFactory(string plugin)
        {
            switch (plugin)
            {
//                case "Flood":
//                    return new FloodPluginFactory(null);
//                case "QuadTree":
//                    return new QuadTreePluginFactory(Config.GetPluginConfig<QuadTreeScenarioConfig>("QuadTree"));
//                case "RoadGraph":
//                    return new RoadGraphPluginFactory(Config.GetPluginConfig<RoadGraphScenarioConfig>("RoadGraph"));
                case "SimpleInfection":
                    return new SimpleInfectionPluginFactory(Config.GetPluginConfig<SimpleInfectionScenarioConfig>("SimpleInfection"));
//                case "ExternalFlows":
//                    return new ExternalFlowsPluginFactory(Config.GetPluginConfig<ExternalFlowsScenarioConfig>("ExternalFlows"));
//                case "RVO":
//                    return new RvoPluginFactory(Config.GetPluginConfig<RvoScenarioConfig>("RVO"));
//                case "Evacuation":
//                    return new EvacuationPluginFactory(Config.GetPluginConfig<EvacuationScenarioConfig>("Evacuation"));
                case "Traffic":
                    return new TrafficPluginFactory(Config.GetPluginConfig<TrafficScenarioConfig>("Traffic"));
                case "SubModel":
                    return new SubModelPluginFactory(Config.GetPluginConfig<SubModelScenarioConfig>("SubModel"));
                case "VasStatPlugin":
                    return new VasStatPluginFactory(Config.GetPluginConfig<VasStatScenarioConfig>("VasStatPlugin"));
                case "MovingPlatformPlugin":
                    return new MovingPlatformPluginFactory(Config.GetPluginConfig<MovingPlatformScenarioConfig>("MovingPlatformPlugin"));
                case "SubwayStation":
                    return new SubwayStationPluginFactory(Config.GetPluginConfig<SubwayStationPluginConfig>("SubwayStation"));
                default:
                    throw new Exception("Unknown plugin: " + plugin);
            }
        }

        public override PluginBaseAgent[] GetPluginsForAgent()
        {
            return PluginFactories.Values.Select(pluginFactory => pluginFactory.GetAgentPlugin()).Where(el => el != null).ToArray();
        }

        public override PluginBaseMap[] GetPluginsForMap()
        {
            return PluginFactories.Values.Select(pluginFactory => pluginFactory.GetMapPlugin(_la)).Where(el => el != null).ToArray();
        }

        public override PluginBaseMapData[] GetPluginsForMapData()
        {
            return PluginFactories.Values.Select(pluginFactory => pluginFactory.GetMapDataPlugin()).Where(el => el != null).ToArray();
        }

        public override PluginBaseBuilding[] GetPluginsForBuilding()
        {
            return PluginFactories.Values.Select(pluginFactory => pluginFactory.GetBuildingPlugin()).Where(el => el != null).ToArray();
        }

        public override PluginBaseAgentData[] GetPluginsForAgentData()
        {
            return PluginFactories.Values.Select(pluginFactory => pluginFactory.GetAgentDataPlugin()).Where(el => el != null).ToArray();
        }
    }
}
