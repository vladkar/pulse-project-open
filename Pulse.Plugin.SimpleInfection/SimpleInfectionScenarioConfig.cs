using Pulse.Common.ConfigSystem;

namespace Pulse.Plugin.SimpleInfection
{
    public class SimpleInfectionScenarioConfig : PulsePluginScenarioConfig
    {
        public SimpleInfectionScenarioConfig(PulseBaseScenarioConfig baseConfig)
            : base(GlobalStrings.PluginName, baseConfig)
        {
        }

        public BaseConfigField<string> InfectionName { set; get; }
        public BaseConfigField<string> InfectionInitialization { set; get; }
    }
}
