namespace Pulse.Common.ConfigSystem
{
    public class PulsePluginScenarioConfig : BaseScenarioConfig
    {
        public PulseBaseScenarioConfig BaseConfig { set; get; }

        public PulsePluginScenarioConfig(string name, PulseBaseScenarioConfig baseConfig)
        {
            Name = new BaseConfigField<string>(name);
            BaseConfig = baseConfig;
        }
    }
}
