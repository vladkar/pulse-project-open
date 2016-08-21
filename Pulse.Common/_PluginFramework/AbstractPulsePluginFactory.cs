using System.Collections.Generic;
using Pulse.Common.ConfigFramework;

namespace Pulse.Common.PluginFramework
{
    public abstract class AbstractPulsePluginFactory
    {
        public IDictionary<string, AbstractPluginFactory> PluginFactories { private set; get; }

        protected AbstractPulsePluginFactory(PulseScenarioConfig config)
        {
            Config = config;
            PluginFactories = new Dictionary<string, AbstractPluginFactory>();
        }

        public PulseScenarioConfig Config { get; set; }

        public abstract PluginBaseAgent[] GetPluginsForAgent();
        public abstract PluginBaseMap[] GetPluginsForMap();
        public abstract PluginBaseMapData[] GetPluginsForMapData();
        public abstract PluginBaseBuilding[] GetPluginsForBuilding();
        public abstract PluginBaseAgentData[] GetPluginsForAgentData();
    }
}
