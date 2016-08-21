using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Plugin.SubModel.Body;

namespace Pulse.Plugin.SubModel
{
    public class SubModelPluginFactory : AbstractPluginFactory
    {
        private SubModelPluginMap _mapPlugin;
        public SubModelScenarioConfig PluginConfigNative { get { return PluginConfig as SubModelScenarioConfig; } }

        public SubModelPluginFactory(SubModelScenarioConfig pluginConfig)
            : base(pluginConfig)
        {
        }

        public override void Initialize()
        {
            _mapPlugin = new SubModelPluginMap(PluginConfigNative);
        }
        
        protected override PluginBaseAgent GetAgentPluginImpl()
        {
            return new SubModelPluginAgent(_mapPlugin);
        }
        
        protected override PluginBaseMap GetMapPluginImpl()
        {
            return _mapPlugin;
        }

        protected override PluginBaseMapData GetMapDatatPluginImpl()
        {
            return new SubModelPluginMapData();
        }

        protected override PluginBaseBuilding GetBuildingPluginImpl()
        {
            return new SubModelPluginBuilding();
        }

        protected override PluginBaseAgentData GetAgentDataPluginImpl()
        {
            return new SubModelPluginAgentData();
        }
    }
}
