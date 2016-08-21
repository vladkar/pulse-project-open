using System.Collections.Generic;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Containers;

namespace Pulse.Common.Model.Environment.Map
{
    public interface IPulseMap : IPluginable
    {
        IDictionary<int, PulseLevel> Levels { get; }
        GeoCartesUtil MapUtils { set; get; }
        AgentRegistry AgentRegistry { get; }
        AbstractGeoWorld World { get; }
        PulseScenery Scenery { get; set; }
        IInfectionData GetInfectionData();
        MovementSystem GetMovementSystem(MovementSystemTypes type, int subgroupId = 0);
    }
}
