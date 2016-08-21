using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;

namespace Pulse.Common
{
    public interface ISceneryFactory
    {
        ILegendable GetRoleManager(string name, IPopulationManager pm);

        void BuildScenery(PulseScenarioConfig config, GeoCartesUtil gcu, AbstractPulsePluginFactory plgnf,
            ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb);
    }

    public interface ISceneryFactory2
    {
        PulseScenery Scenery { get; set; }
        AbstractExternalPortalBuilder PortalBuilder { get; set; }
        ILegendable RoleManager { get; set; }

        void LoadScenery(PulseScenarioConfig config, GeoCartesUtil gcu, AbstractPulsePluginFactory plgnf);
    }
}
