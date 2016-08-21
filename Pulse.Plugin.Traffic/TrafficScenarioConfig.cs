using System;
using Pulse.Common.ConfigSystem;

namespace Pulse.Plugin.Traffic
{
    public class TrafficScenarioConfig : PulsePluginScenarioConfig
    {
        public TrafficScenarioConfig(PulseBaseScenarioConfig baseConfig)
            : base(GlobalStrings.PluginName, baseConfig)
        {
        }
    }
}
