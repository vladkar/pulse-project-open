//extern alias sf;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;
using SF2D;
using SF3D;

namespace Pulse.Model.MovementSystems
{
    public class SfMultilevelMovementSystem : Common.Model.Agent.MovementSystem, ISixFreedomDegreeMovementSystem, ISFMovementSystem
    {
        public IDictionary<int, IDictionary<int, long>> LevelSfPulseRegister { get; }
        public IDictionary<int, IDictionary<long, int>> LevelPulseSfRegister { get; }

        private IDictionary<int, SFSimulator> _lSims;

        public SfMultilevelMovementSystem(PulseScenarioConfig config, PulseScenery sb)
            : base(config, sb)
        {
            _lSims = new Dictionary<int, SFSimulator>();

            LevelPulseSfRegister = new ConcurrentDictionary<int, IDictionary<long, int>>();
            LevelSfPulseRegister = new ConcurrentDictionary<int, IDictionary<int, long>>();
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
            var timeStep = (float)_config.TimeStep.Value;

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
                    PlatformFactor = 0.0000005f
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
                    PlatformFactor = 0f
                };
            }
            
            foreach (var l in _sb.Levels.Keys)
            {
                _lSims[l] = new SFSimulator();
                _lSims[l].setTimeStep(timeStep);
                _lSims[l].setAgentDefaults(ap);

                LevelSfPulseRegister[l] = new ConcurrentDictionary<int, long>();
                LevelPulseSfRegister[l] = new ConcurrentDictionary<long, int>();

                foreach (var obstacle in _sb.Levels[l].Obstacles)
                {
                    var preparedObstacle = obstacle.Take(obstacle.Length - 1).ToArray();
                    var sfObstacle = preparedObstacle.Select(CoordsToSFVector2).ToArray();
                    _lSims[l].addObstacle(sfObstacle);
                    _lSims[l].processObstacles();
                }
            }
        }

        private bool ValidateConfig(IDictionary<string, string> sfConfig)
        {
            return sfConfig != null && sfConfig.Any();
        }

        public override void Update(double timeStep, double time)
        {
            foreach (var l in _sb.Levels.Keys)
            {
                _lSims[l].doStep();


                foreach (var agentKvp in LevelPulseSfRegister[l])
                {
                    var agent = AgentsRegister[agentKvp.Key];

                    var agentPrefVel = new SFVector2((float)agent.PrefVelocity.X, (float)agent.PrefVelocity.Y);
                    var rvoId = LevelPulseSfRegister[l][agent.Id];
                    _lSims[l].setAgentPrefVelocity(rvoId, agentPrefVel);
                }



                foreach (var rvolink in LevelSfPulseRegister[l])
                {
                    var pos = _lSims[l].getAgentPosition(rvolink.Key);
                    var agent = AgentsRegister[rvolink.Value];
                    var force =_lSims[l].getAgentRepulsiveForce(rvolink.Key);
                    agent.Force = new PulseVector2(force.X, force.Y);
                    agent.StepDistance = agent.Point.DistanceTo(agent.Point);
                    agent.Pressure = _lSims[l].getAgentPressure(rvolink.Key);

                    var agentCoords = SFVector2ToCoords(pos);

                    if (!Double.IsNaN(agentCoords.X) && !Double.IsNaN(agentCoords.Y))
                        agent.Move(agentCoords);
                    else
                    {
                        Debug.Assert(true);
                        Console.Out.WriteLine("NaN from SF movement system");
                    }
                }
            }
        }


        //TODO TEMP!
        public static float ExpCoef = 1;

        private object _sem = new object();
        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                lock (_sem)
                {
                    var rvoAgentCoords = CoordsToSFVector2(agent.Point);
                    var rvoId = _lSims[agent.Level].addAgent(rvoAgentCoords);
                    var walkingSpeed = (float)agent.PhysicalCapabilityClass.Speed * 1000f / 3600;
                    _lSims[agent.Level].setAgentMaxSpeed(rvoId, walkingSpeed);
                    LevelSfPulseRegister[agent.Level][rvoId] = agent.Id;
                    LevelPulseSfRegister[agent.Level][agent.Id] = rvoId;
                    AgentsRegister[agent.Id] = agent;

                    var force = agent.Role.Id == 140? ExpCoef : 1;
                    _lSims[agent.Level].setAgentForce(rvoId, force);
                }
            }
        }

        private static int removedAgentsOffset = 0;
        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                var rvoId = LevelPulseSfRegister[agent.Level][agent.Id];
                LevelSfPulseRegister[agent.Level].Remove(rvoId);
                LevelPulseSfRegister[agent.Level].Remove(agent.Id);
                AgentsRegister.Remove(agent.Id);

                _lSims[agent.Level].setAgentVelocity(rvoId, new SFVector2(0, 0));
                _lSims[agent.Level].setAgentPosition(rvoId, new SFVector2(-1000, -1000-removedAgentsOffset--));
                _lSims[agent.Level].setAgentPrefVelocity(rvoId, new SFVector2(-1000, -1000));
            }
        }

        public override bool QueryVisibility(PulseVector2 point, PulseVector2 current, int level, float i)
        {
            var p1 = new SFVector2((float) point.X, (float) point.Y);
            var p2 = new SFVector2((float)current.X, (float)current.Y);
            return _lSims[level].queryVisibility(p1, p2, i);
        }

        public override void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            _lSims[agent.Level].setAgentMaxSpeed(LevelPulseSfRegister[agent.Level][agent.Id], (float)maxSpeed);
        }

        public void SetAgentForce(AbstractPulseAgent agent, double force)
        {
            _lSims[agent.Level].setAgentForce(LevelPulseSfRegister[agent.Level][agent.Id], (float)force);
        }

        public override double GetAgentMaxSpeed(AbstractPulseAgent agent)
        {
            return _lSims[agent.Level].getAgentMaxSpeed(LevelPulseSfRegister[agent.Level][agent.Id]);
        }

        public override void SetCommand(ICommand command)
        {
            var args = command.Args;
            foreach (var sim in _lSims.Values)
            {
                sim.updateSFParameters(Convert.ToSingle(args[0]), Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]));
            }
        }

        public void MovePlatform(double dx, double dy, double dz, double pitch, double roll, double yaw)
        {
            foreach (var sim in _lSims.Values)
            {
                sim.setAdditionalForce(new SFVector3((float) dx, (float) dy, (float) dz),
                    new SFRotationDegreeSet((float) pitch, (float) roll, (float) yaw, new SFVector3(0, 0, 0)));
            }
            
        }

        public double GetAgentPressure(AbstractPulseAgent agent)
        {
            return _lSims[agent.Level].getAgentPressure(LevelPulseSfRegister[agent.Level][agent.Id]);
        }

        public double GetObstaclePressure(AbstractPulseAgent agent)
        {
            return _lSims[agent.Level].getObstaclePressure(LevelPulseSfRegister[agent.Level][agent.Id]);
        }

        public void AddObstacle(AbstractPulseAgent agent, IList<PulseVector2> obstacle)
        {
            _lSims[agent.Level].addObstacle(obstacle.Select(p => new SFVector2((float)p.X, (float)p.Y)).ToArray());
            _lSims[agent.Level].processObstacles();
        }
    }
}