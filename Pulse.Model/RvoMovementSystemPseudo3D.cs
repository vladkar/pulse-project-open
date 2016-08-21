using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MultiagentEngine.Pulse.Map;
using Pulse.Common;
using Pulse.Common.ConfigFramework;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Scheduling.Traveling;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.Model.Agents;
using RVO2D;

namespace Pulse.Model
{
    public class RvoMovementSystemPseudo3D : MovementSystem
    {
//        public List<AbstractPulseAgent> Agents { get; private set; }
        public IDictionary<int, string> RvoPulseRegister { get; private set; }
        public IDictionary<string, int> PulseRvoRegister { get; private set; }

        public Coords Offset { private set; get; }

        private RVOSimulator _sim;

        public RvoMovementSystemPseudo3D(PulseScenarioConfig config, SceneryBlock sb)
            : base(config, sb)
        {
            _sim = new RVOSimulator();
           
            RvoPulseRegister = new ConcurrentDictionary<int, string>();
            PulseRvoRegister = new ConcurrentDictionary<string, int>();
        }

        private RvoVector2 CoordsToRvoVector2(Coords p)
        {
            return new RvoVector2((float)p.X, (float)p.Y);
        }

        private Coords RvoVector2ToCoords(RvoVector2 p)
        {
            return new Coords(p.X, p.Y);
        }


        /*
         * It is important to know that the order of the vertices matter, 
         * clockwise polygons will shut agents in but let them move freely into it, 
         * as opposed to counter-clockwise polygons will keep agents out from it, 
         * but if they are inside it for some reason, they can move out from it easily.
         */
        protected override void LoadData()
        {
            Offset = new Coords(400, 0);

            var timeStep = (float)_config.TimeStep.Value;
            _sim.setTimeStep(timeStep);
            var prop = new AgentProperty(15.0f, 10, 5.0f, 5.0f, 2.0f, 2.0f, 0.5f, 0.6f, 100, 0.075f, 0.25f, 0.035f, new RvoVector2());
            _sim.setAgentDefaults(prop);

            foreach (var level in _sb.Levels)
            {
                // CINEMA harcode
//                for (var i = 0; i < level.Value.Obstacles.Count; i++)
//                {
//                    var obstacle = level.Value.Obstacles[i];
//                    var rvoObstacle = obstacle.Take(obstacle.Length - 1).Select(p => CoordsToRvoVector2(p + Offset * (level.Key - 1))).ToArray();
//                    if (i < 2)
//                        rvoObstacle = rvoObstacle.Reverse().ToArray();
//
//                    _sim.addObstacle(rvoObstacle);
//                }

                // SEA and SUBWAY STATIONS harcode

//                var mm = level.Value.Obstacles.Max(o => o.Max(p => p.X));
//                var maxI = level.Value.Obstacles.ToList().FindIndex(o => o.Any(p => p.X == mm));
//
//
//                for (var i = 0; i < level.Value.Obstacles.Count; i++)
//                {
//                    var obstacle = level.Value.Obstacles[i];
//                    
//                    var rvoObstacle = obstacle.Take(obstacle.Length - 1).Select(p => CoordsToRvoVector2(p + Offset * (level.Key - 1))).ToArray();
//                    if (i == maxI)
//                    {
//                        rvoObstacle = rvoObstacle.ToArray();
//                        _sim.addObstacle(rvoObstacle);
//                    }
//                }


                //universal

//                var mm = level.Value.Obstacles.Max(o => o.Max(p => p.X));
//                var maxI = level.Value.Obstacles.ToList().FindIndex(o => o.Any(p => p.X == mm));

                var maxContourCount = level.Value.Obstacles.Max(o => o.Length);


                foreach (var obstacle in level.Value.Obstacles)
                {
                    var preparedObstacle = obstacle.Take(obstacle.Length - 1).Select(p => p + Offset * (level.Key - 1)).ToArray();
                    
                    preparedObstacle = (preparedObstacle.Length + 1) == maxContourCount ? ToClockwise(preparedObstacle) : ToCounterclockWise(preparedObstacle);

                    var rvoObstacle = preparedObstacle.Select(CoordsToRvoVector2).ToArray();
                    _sim.addObstacle(rvoObstacle);
                }

                // CINEMA 2
//                for (var i = 0; i < level.Value.Obstacles.Count; i++)
//                {
//                    var obstacle = level.Value.Obstacles[i];
//                    var rvoObstacle = obstacle.Take(obstacle.Length - 1).Select(p => CoordsToRvoVector2(p + Offset * (level.Key - 1))).ToArray();
//
//                    if ()
//                }

//                foreach (var obstacle in level.Value.Obstacles)
//                {
//                    var rvoObstacle = obstacle.Take(obstacle.Length - 1).Select(p => CoordsToRvoVector2(p + Offset * (level.Key - 1))).ToArray();
//                    if (rvoObstacle.Length > 150)
//                        rvoObstacle = rvoObstacle.Reverse().ToArray();
//                    _sim.addObstacle(rvoObstacle);
//                }
            }

            _sim.processObstacles();
        }

        private Coords[] ToClockwise(Coords[] polygon)
        {
            return   ClipperUtil.IsClockWise(polygon) ? polygon : polygon.Reverse().ToArray();
        }

        private Coords[] ToCounterclockWise(Coords[] polygon)
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
                
                var agentCoords = RvoVector2ToCoords(pos) - Offset * (agent.Floor - 1);
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
                    var rvoAgentCoords = CoordsToRvoVector2(agent.Point + Offset*(agent.Floor - 1));
                    var rvoId = _sim.addAgent(rvoAgentCoords);
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

        public override bool QueryVisibility(Coords point, Coords current, float i)
        {
            var p1 = new RvoVector2((float) point.X, (float) point.Y);
            var p2 = new RvoVector2((float)current.X, (float)current.Y);
            return _sim.queryVisibility(p1, p2, i);
        }
    }
}
