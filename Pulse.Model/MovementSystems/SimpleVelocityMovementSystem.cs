using System;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.MovementSystems
{
    public class SimpleVelocityMovementSystem : Common.Model.Agent.MovementSystem
    {
        public SimpleVelocityMovementSystem(PulseScenarioConfig config, PulseScenery sb, GeoCartesUtil gcu) : base(config, sb)
        {
            MapUtils = gcu;
        }

        public GeoCartesUtil MapUtils { get; set; }

        protected override void LoadData()
        {
        }

        public override void Update(double timeStep, double time)
        {
//            AgentsRegister.AsParallel().ForAll(agentKvp =>
            foreach (var agentKvp in AgentsRegister)
            {
                var agent = agentKvp.Value;
                PulseVector2 currentPoint = default(PulseVector2);

                var speed = agent.PhysicalCapabilityClass.Speed * 1000d / 3600; // meters per second
                var distStep = speed * timeStep / MapUtils.MetersPerMapUnit;

                if (agent.DesiredPosition != null)
                {
                    if (agent.DesiredPosition.DistanceTo(agent.Point) < distStep)
                        currentPoint = agent.DesiredPosition;
                    else
                        currentPoint = ClipperUtil.GetDistancePointOnGgment(agent.Point, agent.DesiredPosition, distStep);


//                    var newPos = ClipperUtil.GetDistancePointOnGgment(agent.Point, agent.DesiredPosition, distStep);
//                    currentPoint = double.IsNaN(newPos.X) || double.IsNaN(newPos.Y) || agent.Point.DistanceTo(agent.DesiredPosition) < distStep ? agent.DesiredPosition : newPos;
                }
                else
                {
                    currentPoint = agent.Point;
                }

                agent.Move(currentPoint);
                }
//            });
        }

        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister[agent.Id] = agent;
            }
        }

        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister.Remove(agent.Id);
            }
        }

        public override bool QueryVisibility(PulseVector2 point, PulseVector2 current, int level, float i)
        {
            return true;
        }

        public override void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            throw new NotImplementedException();
        }

        public override void SetCommand(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
