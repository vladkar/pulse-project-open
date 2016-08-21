using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageAgentGenerator : SimpleIterationProbabilityAgentsGenerator
    {
        public VkfestStageAgentGenerator(IExternalPortal externalPortal, double p, double count) : base(externalPortal, p, count)
        {
        }

        protected override AbstractPulseAgent CreateAgent()
        {
            var agent = _pm.CreateAgent();

            agent.Home = _portal;
            agent.Level = _portal.Level;
            agent.Move(ClipperUtil.GetRandomPointOnPolygon(_portal.Polygon));
//            agent.Move(ClipperUtil.GetCentroid(_portal.Polygon));

            var arrivedShipPasRole = new VkfestStageSimpleRole(agent);
            agent.Role = arrivedShipPasRole;

            _map.World.AddNewAgent(agent);
            agent.Role.Initialize();

            return agent;
        }
    }
}