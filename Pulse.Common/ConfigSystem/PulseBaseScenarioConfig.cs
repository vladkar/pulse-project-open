using System;
using System.Collections.Generic;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.ConfigSystem
{
    public class PulseBaseScenarioConfig : BaseScenarioConfig
    {
        public BaseConfigField<string> DataDir { set; get; }
        public BaseConfigField<string[]> Plugins { get; set; }
        public BaseConfigField<double> TimeStep { set; get; }
        public BaseConfigField<double> ToSecondsMultiplier { set; get; }
        public BaseConfigField<DateTime> TimeStart { set; get; }
        public BaseConfigField<PulseVector2> MapPointMin { set; get; }
        public BaseConfigField<PulseVector2> MapPointMax { set; get; }
        public BaseConfigField<GeoCoords> GeoPointMin { set; get; }
        public BaseConfigField<GeoCoords> GeoPointMax { get; set; }
        public BaseConfigField<double> MetersPerMapUnit { set; get; }
        public BaseConfigField<int> InitialPopulation { set; get; }
        //public BaseConfigField<string> SceneryBlock { set; get; }
        public BaseConfigField<string> PreferredCoordinates { set; get; }
        public BaseConfigField<string> VisNiceName { get; set; }

        public BaseConfigField<IDictionary<string, string>> MovementSystems { get; set; }


        public IDictionary<string, PulsePluginScenarioConfig> PluginsConfig { set; get; }

        public T GetPluginConfig<T>(string name) where T : PulsePluginScenarioConfig
        {
            var pluginConfig = PluginsConfig[name] as T;
            return pluginConfig;
        }
    }
}
