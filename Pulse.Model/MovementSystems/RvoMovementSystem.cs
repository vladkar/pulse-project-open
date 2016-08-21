using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;
using RVO2D;

namespace Pulse.Model.MovementSystems
{
    public class RvoMovementSystem : Common.Model.Agent.MovementSystem
    {
        public IDictionary<int, long> RvoPulseRegister { get; }
        public IDictionary<long, int> PulseRvoRegister { get; }

        public PulseVector2 Offset { private set; get; }

        private RVOSimulator _sim;

        public RvoMovementSystem(PulseScenarioConfig config, PulseScenery sb)
            : base(config, sb)
        {
            _sim = new RVOSimulator();
           
            RvoPulseRegister = new ConcurrentDictionary<int, long>();
            PulseRvoRegister = new ConcurrentDictionary<long, int>();
        }

        private RvoVector2 CoordsToRvoVector2(PulseVector2 p)
        {
            return new RvoVector2((float)p.X, (float)p.Y);
        }

        private PulseVector2 RvoVector2ToCoords(RvoVector2 p)
        {
            return new PulseVector2(p.X, p.Y);
        }


        /*
         * It is important to know that the order of the vertices matter, 
         * clockwise polygons will shut agents in but let them move freely into it, 
         * as opposed to counter-clockwise polygons will keep agents out from it, 
         * but if they are inside it for some reason, they can move out from it easily.
         */
        protected override void LoadData()
        {
            Offset = new PulseVector2(400, 0);

            var timeStep = (float)_config.TimeStep.Value;
            _sim.setTimeStep(timeStep);
//            _sim.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, new RvoVector2());
            var defaultBodyRadius = 0.2f;
            _sim.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, defaultBodyRadius, 2.0f, new RvoVector2());

            foreach (var level in _sb.Levels)
            {
                var maxContourCount = level.Value.Obstacles.Max(o => o.Length);

                foreach (var obstacle in level.Value.Obstacles)
                {
                    var preparedObstacle = obstacle.Take(obstacle.Length - 1).Select(p => p + Offset * (level.Key - 1)).ToArray();
                    
                    preparedObstacle = (preparedObstacle.Length + 1) == maxContourCount ? ToClockwise(preparedObstacle) : ToCounterclockWise(preparedObstacle);

                    var rvoObstacle = preparedObstacle.Select(CoordsToRvoVector2).ToArray();
                    _sim.addObstacle(rvoObstacle);
                }
            }

            _sim.processObstacles();
        }

        private PulseVector2[] ToClockwise(PulseVector2[] polygon)
        {
            return ClipperUtil.IsClockWise(polygon) ? polygon : polygon.Reverse().ToArray();
        }

        private PulseVector2[] ToCounterclockWise(PulseVector2[] polygon)
        {
            return !ClipperUtil.IsClockWise(polygon) ? polygon : polygon.Reverse().ToArray();
        }

        public override void Update(double timeStep, double time)
        {
            _sim.doStep();
            SetPreferredVelocities2();

            foreach (var rvolink in RvoPulseRegister)
            {
                var pos = _sim.getAgentPosition(rvolink.Key);
                var agent = AgentsRegister[rvolink.Value];
                
                var agentCoords = RvoVector2ToCoords(pos) - Offset * (agent.Level - 1);
                if (!Double.IsNaN(agentCoords.X) && !Double.IsNaN(agentCoords.Y))
                    agent.Move(agentCoords);
                else
                    Debug.Assert(true);
            }
        }
        
        private void SetPreferredVelocities2()
        {
            foreach (var agentKvp in AgentsRegister)
            {
                var agent = agentKvp.Value;
                var agentPrefVel = new RvoVector2((float)agent.PrefVelocity.X, (float)agent.PrefVelocity.Y);
                var rvoId = PulseRvoRegister[agent.Id];
                _sim.setAgentPrefVelocity(rvoId, agentPrefVel);

                /*
                 * Perturb a little to avoid deadlocks due to perfect symmetry.
                 */

                float angle = (float)RandomUtil.RandomDouble() * 2.0f * (float)Math.PI;
                float dist = (float)RandomUtil.RandomDouble() * 0.0001f;

                var velRaw = _sim.getAgentPrefVelocity(rvoId);
                var x = velRaw.X + Math.Cos(angle)*dist;
                var y = velRaw.Y + Math.Sin(angle)*dist;
                var vel = new RvoVector2((float) x, (float) y);

                _sim.setAgentPrefVelocity(rvoId, vel);
            }
        }

        private object _sem = new object();
        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                lock (_sem)
                {
                    var rvoAgentCoords = CoordsToRvoVector2(agent.Point + Offset*(agent.Level - 1));
                    var rvoId = _sim.addAgent(rvoAgentCoords);
                    var walkingSpeed = (float)agent.PhysicalCapabilityClass.Speed*1000f/3600;
                    _sim.setAgentMaxSpeed(rvoId, walkingSpeed);
                    RvoPulseRegister[rvoId] = agent.Id;
                    PulseRvoRegister[agent.Id] = rvoId;
                    AgentsRegister[agent.Id] = agent;
                }
            }
        }

        private static int removedAgentsOffset = 0;
        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                var rvoId = PulseRvoRegister[agent.Id];
                RvoPulseRegister.Remove(rvoId);
                PulseRvoRegister.Remove(agent.Id);
                AgentsRegister.Remove(agent.Id);

                _sim.setAgentVelocity(rvoId, new RvoVector2(0, 0));
                _sim.setAgentPosition(rvoId, new RvoVector2(-1000, -1000-removedAgentsOffset--));
            }
        }

        public override bool QueryVisibility(PulseVector2 point, PulseVector2 current, int level, float i)
        {
            var p1 = new RvoVector2((float) point.X, (float) point.Y);
            var p2 = new RvoVector2((float)current.X, (float)current.Y);
            return _sim.queryVisibility(p1, p2, i);
        }

        public override void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            _sim.setAgentMaxSpeed(PulseRvoRegister[agent.Id], (float)maxSpeed);
        }

        public override double GetAgentMaxSpeed(AbstractPulseAgent agent)
        {
            return _sim.getAgentMaxSpeed(PulseRvoRegister[agent.Id]);
        }

        public override void SetCommand(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
