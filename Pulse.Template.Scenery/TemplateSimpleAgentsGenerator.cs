using System;
using MultiagentEngine.Pulse.Map;
using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace $pulsepre$$safeprojectname$
{
	public class $safeprojectname$SimpleAgentsGenerator : AbstractExternalPortalAgentGenerator
    {
        private IExternalPortal _portal;

        public $safeprojectname$SimpleAgentsGenerator(IExternalPortal externalPortal)
        {
            _portal = externalPortal;
        }

        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (RandomUtil.PlayProbability(0.1))
            {
                CreateAgent();
            }
        }

        private AbstractPulseAgent CreateAgent()
        {
            var agent = _pm.CreateAgent();

            agent.Home = _portal;
            agent.Floor = _portal.Floor;
            agent.Move(ClipperUtil.GetRandomPointOnPolygon(_portal.Polygon));

            var arrivedShipPasRole = new $safeprojectname$SimpleRole(agent);
            agent.Role = arrivedShipPasRole;

            _map.World.AddNewAgent(agent);

            return agent;
        }

        private Coords GetRandomPointOnCircle(Coords center, double radius)
        {
            var d = radius*RandomUtil.RandomDouble();
            var a = RandomUtil.RandomInt(0, 360);

            return new Coords(center.X + d*Math.Cos(a), center.Y + d*Math.Sin(a));
        }
    }
}