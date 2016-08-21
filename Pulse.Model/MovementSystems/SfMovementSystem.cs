//extern alias sf;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;
using Pulse.Model.Agents;
using Pulse.MultiagentEngine.Map;
using SF2D;
using SF3D;

namespace Pulse.Model.MovementSystems
{
    public class SfMovementSystem : Common.Model.Agent.MovementSystem, ISixFreedomDegreeMovementSystem
    {
        public IDictionary<int, long> SfPulseRegister { get; }
        public IDictionary<long, int> PulseSfRegister { get; }

        public PulseVector2 Offset { private set; get; }

        private SFSimulator _sim;

        public SfMovementSystem(PulseScenarioConfig config, PulseScenery sb)
            : base(config, sb)
        {
            _sim = new SFSimulator();
           
            SfPulseRegister = new ConcurrentDictionary<int, long>();
            PulseSfRegister = new ConcurrentDictionary<long, int>();
        }

        private SFVector2 CoordsToSFVector2(PulseVector2 p)
        {
            return new SFVector2((float)p.X, (float)p.Y);
        }

        private PulseVector2 SFVector2ToCoords(SFVector2 p)
        {
            return new PulseVector2(p.X, p.Y);
        }
        
        protected override void LoadData()
        {
            Offset = new PulseVector2(400, 0);

            var timeStep = (float)_config.TimeStep.Value;
            _sim.setTimeStep(timeStep);

            AgentProperty ap;
            if (ValidateConfig(_config.SfConfig))
            {
                ap = new AgentProperty
                {
                    NeighborDist = float.Parse(_config.SfConfig["NeighborDist"], CultureInfo.InvariantCulture),
                    MaxNeighbors = int.Parse(_config.SfConfig["MaxNeighbors"], CultureInfo.InvariantCulture),
                    TimeHorizon = float.Parse(_config.SfConfig["TimeHorizon"], CultureInfo.InvariantCulture),
                    Radius = float.Parse(_config.SfConfig["Radius"], CultureInfo.InvariantCulture),
                    MaxSpeed = float.Parse(_config.SfConfig["MaxSpeed"], CultureInfo.InvariantCulture),
                    AccelerationCoefficient = float.Parse(_config.SfConfig["AccelerationCoefficient"], CultureInfo.InvariantCulture),
                    RepulsiveAgent = float.Parse(_config.SfConfig["RepulsiveAgent"], CultureInfo.InvariantCulture),
                    RepulsiveAgentFactor = float.Parse(_config.SfConfig["RepulsiveAgentFactor"], CultureInfo.InvariantCulture),
                    RepulsiveObstacle = float.Parse(_config.SfConfig["RepulsiveObstacle"], CultureInfo.InvariantCulture),
                    RepulsiveObstacleFactor = float.Parse(_config.SfConfig["RepulsiveObstacleFactor"], CultureInfo.InvariantCulture),
                    Perception = float.Parse(_config.SfConfig["Perception"], CultureInfo.InvariantCulture),
                    RelaxationTime = float.Parse(_config.SfConfig["RelaxationTime"], CultureInfo.InvariantCulture),
                    Friction = float.Parse(_config.SfConfig["Friction"], CultureInfo.InvariantCulture),
                    Velocity = new SFVector2(),
                    PlatformFactor = float.Parse(_config.SfConfig["PlatformFactor"], CultureInfo.InvariantCulture)
                };
            }
            else
            {
                ap = new AgentProperty
                {
                    NeighborDist = 15f,
                    MaxNeighbors = 10,
                    TimeHorizon = 5f,
                    Radius = 0.2f,
                    MaxSpeed = 2.0f,
                    AccelerationCoefficient = 0.5f,
                    RepulsiveAgent = 0.19f,
                    RepulsiveAgentFactor = 46,
                    RepulsiveObstacle = 4f,
                    RepulsiveObstacleFactor = 0.32f,
                    Perception = 0.25f,
                    Friction = 1,
                    RelaxationTime = 1,
                    Velocity = new SFVector2(),
                    PlatformFactor = 0.0000005f
                };
            }

            _sim.setAgentDefaults(ap);
            
            foreach (var level in _sb.Levels)
            {
                foreach (var obstacle in level.Value.Obstacles)
                {
                    var preparedObstacle = obstacle.Take(obstacle.Length - 1).Select(p => p + Offset * (level.Key - 1)).ToArray();
                    var sfObstacle = preparedObstacle.Select(CoordsToSFVector2).ToArray();
                    _sim.addObstacle(sfObstacle);
                }
            }

            _sim.processObstacles();
        }

        private bool ValidateConfig(IDictionary<string, string> sfConfig)
        {
            return sfConfig != null && sfConfig.Any();
        }

        private PulseVector2[] ToClockwise(PulseVector2[] polygon)
        {
            return ClipperUtil.IsClockWise(polygon) ? polygon : polygon.Reverse().ToArray();
        }

        private PulseVector2[] ToCounterclockWise(PulseVector2[] polygon)
        {
            return !ClipperUtil.IsClockWise(polygon) ? polygon : polygon.Reverse().ToArray();
        }
        
        private static Dictionary<int, float> FromLine(string line)
        {
            var words = line.Replace(".", ",").Split(new[] {'\t', ' '}, StringSplitOptions.RemoveEmptyEntries);
                        
            float roll = float.Parse(words[0]);
            float pitch = float.Parse(words[1]);
            float heave = float.Parse(words[2]);

            Dictionary<int, float> motions = new Dictionary<int, float>(3);

            motions.Add(1, roll);
            motions.Add(2, pitch);
            motions.Add(3, heave);

            return motions;
        }

        private static Dictionary<int, float>[] ReadMatrix(string filePath)
        {
            var m = File.ReadAllLines(filePath)
                .Select(line => FromLine(line))
                .ToArray();
            return m;
        }


        public override void Update(double timeStep, double time)
        {
            
            SetPreferredVelocities2();
            _sim.doStep();

            foreach (var rvolink in SfPulseRegister)
            {
                var pos = _sim.getAgentPosition(rvolink.Key);
                var agent = AgentsRegister[rvolink.Value];

                var agentCoords = SFVector2ToCoords(pos) - Offset*(agent.Level - 1);
                var agetPressure = _sim.getAgentPressure(rvolink.Key);

                if (!Double.IsNaN(agentCoords.X) && !Double.IsNaN(agentCoords.Y))
                {
                    agent.StepDistance = agent.Point.DistanceTo(agentCoords);
                    agent.Move(agentCoords);
                    agent.Pressure = agetPressure;
                }
                else
                {
//                    Debug.Assert(true);
                    Console.Out.WriteLine("NaN from SF movement system");
                }
            }
        }

        private void SetPreferredVelocities2()
        {
            foreach (var agentKvp in AgentsRegister)
            {
                _sim.setAgentPrefVelocity(PulseSfRegister[agentKvp.Value.Id], new SFVector2((float)agentKvp.Value.PrefVelocity.X, (float)agentKvp.Value.PrefVelocity.Y));

                /*
                 * Perturb a little to avoid deadlocks due to perfect symmetry.
                 */

//                float angle = (float)RandomUtil.RandomDouble() * 2.0f * (float)Math.PI;
//                float dist = (float)RandomUtil.RandomDouble() * 0.0001f;
//
//                var velRaw = _sim.getAgentPrefVelocity(rvoId);
//                var x = velRaw.X + Math.Cos(angle)*dist;
//                var y = velRaw.Y + Math.Sin(angle)*dist;
//                var vel = new SF2D.SFVector2((float) x, (float) y);
//
//                _sim.setAgentPrefVelocity(rvoId, vel);
            }
        }

        private object _sem = new object();
        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                lock (_sem)
                {
                    var rvoAgentCoords = CoordsToSFVector2(agent.Point + Offset*(agent.Level - 1));
                    var rvoId = _sim.addAgent(rvoAgentCoords);
                    var walkingSpeed = (float)agent.PhysicalCapabilityClass.Speed * 1000f / 3600;
                    
                    _sim.setAgentMaxSpeed(rvoId, walkingSpeed);

                    SfPulseRegister[rvoId] = agent.Id;
                    PulseSfRegister[agent.Id] = rvoId;
                    AgentsRegister[agent.Id] = agent;
                }
            }
        }

        private static int removedAgentsOffset = 0;
        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                var rvoId = PulseSfRegister[agent.Id];
                SfPulseRegister.Remove(rvoId);
                PulseSfRegister.Remove(agent.Id);
                AgentsRegister.Remove(agent.Id);
                
                _sim.setAgentPosition(rvoId, new SFVector2(-1000, -1000-removedAgentsOffset--));
                _sim.setAgentVelocity(rvoId, new SFVector2(0, 0));
            }
        }

        public override bool QueryVisibility(PulseVector2 point, PulseVector2 current, int level, float i)
        {
            var p1 = CoordsToSFVector2(point + Offset * (level - 1));
            var p2 = CoordsToSFVector2(current + Offset * (level - 1));
            return _sim.queryVisibility(p1, p2, i);
        }

        public override void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            _sim.setAgentMaxSpeed(PulseSfRegister[agent.Id], (float)maxSpeed);
        }

        public override double GetAgentMaxSpeed(AbstractPulseAgent agent)
        {
            return _sim.getAgentMaxSpeed(PulseSfRegister[agent.Id]);
        }

        public override void SetCommand(ICommand command)
        {
            var args = command.Args;
            _sim.updateSFParameters(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]));
        }

        public void MovePlatform(double dx, double dy, double dz, double pitch, double roll, double yaw)
        {
            _sim.setAdditionalForce(new SFVector3((float)dx, (float)dy, (float)dz), new SFRotationDegreeSet((float)pitch, (float)roll, (float)yaw, new SFVector3(0, 0, 0)));
        }

        public double GetAgentPressure(AbstractPulseAgent agent)
        {
            return _sim.getAgentPressure(PulseSfRegister[agent.Id]);
        }

        public double GetObstaclePressure(AbstractPulseAgent agent)
        {
            return _sim.getObstaclePressure(PulseSfRegister[agent.Id]);
        }
    }
}