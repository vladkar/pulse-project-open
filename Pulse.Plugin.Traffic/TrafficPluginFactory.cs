using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Plugin.Traffic.Body;

namespace Pulse.Plugin.Traffic
{
    public class TrafficPluginFactory : AbstractPluginFactory
    {
        private TrafficPluginMap _mapPlugin;
        public TrafficScenarioConfig PluginConfigNative { get { return PluginConfig as TrafficScenarioConfig; } }

        public TrafficPluginFactory(TrafficScenarioConfig pluginConfig)
            : base(pluginConfig)
        {
        }

        public override void Initialize()
        {
            _mapPlugin = new TrafficPluginMap(PluginConfigNative);
        }
        
        protected override PluginBaseAgent GetAgentPluginImpl()
        {
            return new TrafficPluginAgent(_mapPlugin);
        }
        
        protected override PluginBaseMap GetMapPluginImpl()
        {
            return _mapPlugin;
        }

        protected override PluginBaseMapData GetMapDatatPluginImpl()
        {
            return new TrafficPluginMapData();
        }

        protected override PluginBaseBuilding GetBuildingPluginImpl()
        {
            return new TrafficPluginBuilding();
        }

        protected override PluginBaseAgentData GetAgentDataPluginImpl()
        {
            return new TrafficPluginAgentData();
        }
    }
}
