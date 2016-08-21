using System.Security.Cryptography.X509Certificates;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Environment.World;

namespace Pulse.Plugin.SubModel
{
    public class SubModelScenarioConfig : PulsePluginScenarioConfig
    {
        public BaseConfigField<GeoCoords> MappingPointBL { set; get; }
        public BaseConfigField<string> SubModel { set; get; }
        public BaseConfigField<string> SubScenario { set; get; }

        public SubModelScenarioConfig(PulseBaseScenarioConfig baseConfig)
            : base(GlobalStrings.PluginName, baseConfig)
        {
        }
    }
}
