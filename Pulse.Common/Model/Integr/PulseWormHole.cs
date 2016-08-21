using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Model.Integr
{
    public class PulseWormHole : ExternalPortal
    {
        public PulseWormHole(ILevelPortal p) : base(p)
        {
        }

        public override void Enter(AbstractPulseAgent agent)
        {
            OnAgentEnterInvoke(agent);
            (AgentGenerator as WormHoleAgentsGenerator).TeleportAgent(agent);
            agent.Kill("migration_" + ((Types != null && Types.Any()) ? Types.First().Name : "null"));
        }
    }
}
