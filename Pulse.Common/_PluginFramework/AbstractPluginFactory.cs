﻿using Pulse.Common.ConfigFramework;

namespace Pulse.Common.PluginFramework
{
    public abstract class AbstractPluginFactory
    {
        private bool _isInitialized = false;
        public bool IsInitialized { get { return _isInitialized; } }

        protected AbstractPluginFactory(PluginScenarioConfig pluginConfig)
        {
            PluginConfig = pluginConfig;
        }

        public PluginScenarioConfig PluginConfig { get; set; }

        public abstract void Initialize();

        private void InitCheckDecorator()
        {
            if (!_isInitialized)
            {
                Initialize();
                _isInitialized = true;
            }
        }

        public PluginBaseAgent GetAgentPlugin()
        {
            InitCheckDecorator();
            return GetAgentPluginImpl();
        }

        public PluginBaseMap GetMapPlugin()
        {
            InitCheckDecorator();
            return GetMapPluginImpl();
        }

        public PluginBaseMap GetMapPlugin(LegendAggregator la)
        {
            var mapPlgn = GetMapPlugin();

            var iLegendable = mapPlgn as ILegendable;
            if (iLegendable != null)
                la.Legendables.Add(iLegendable);

            return mapPlgn;
        }

        public PluginBaseMapData GetMapDataPlugin()
        {
            InitCheckDecorator();
            return GetMapDatatPluginImpl();
        }

        public PluginBaseBuilding GetBuildingPlugin()
        {
            InitCheckDecorator();
            return GetBuildingPluginImpl();
        }

        public PluginBaseAgentData GetAgentDataPlugin()
        {
            InitCheckDecorator();
            return GetAgentDataPluginImpl();
        }

        protected abstract PluginBaseAgent GetAgentPluginImpl();
        protected abstract PluginBaseMap GetMapPluginImpl();
        protected abstract PluginBaseMapData GetMapDatatPluginImpl();
        protected abstract PluginBaseBuilding GetBuildingPluginImpl();
        protected abstract PluginBaseAgentData GetAgentDataPluginImpl();

        
    }
}
