using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Plugin.SimpleInfection.Body;
using Pulse.Plugin.SimpleInfection.Infection;

namespace Pulse.Plugin.SimpleInfection
{
    public class SimpleInfectionPluginFactory : AbstractPluginFactory
    {
        private SimpleInfectionPluginMap _mapPlugin;
        private InfectionStateManager _ifsf;
        public SimpleInfectionScenarioConfig PluginConfigNative { get { return PluginConfig as SimpleInfectionScenarioConfig; } }

        public SimpleInfectionPluginFactory(SimpleInfectionScenarioConfig pluginConfig)
            : base(pluginConfig)
        {
        }

        public override void Initialize()
        {
            var ifr = new InfectionDataReader(PluginConfigNative.BaseConfig.DataDir.Value, PluginConfigNative.InfectionName.Value);
            _ifsf = new InfectionStateManager(ifr);
            _ifsf.Initialize();


            _mapPlugin = new SimpleInfectionPluginMap(PluginConfigNative, _ifsf);
        }


        protected override PluginBaseAgent GetAgentPluginImpl()
        {
            return new SimpleInfectionPluginAgent(_mapPlugin, _ifsf);
        }
        
        protected override PluginBaseMap GetMapPluginImpl()
        {
            return _mapPlugin;
        }

        protected override PluginBaseMapData GetMapDatatPluginImpl()
        {
            return new SimpleInfectionPluginMapData();
        }

        protected override PluginBaseBuilding GetBuildingPluginImpl()
        {
            return new SimpleInfectionPluginBuilding();
        }

        protected override PluginBaseAgentData GetAgentDataPluginImpl()
        {
            return new SimpleInfectionPluginAgentData();
        }
    }
}
