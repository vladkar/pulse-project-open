using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Agent.SocialNetworks;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.PluginSystem.Util;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.Common;
using Pulse.Model.AgentNetwork;
using Pulse.Model.Agents;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Containers;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Map;
using Pulse.MultiagentEngine.Settings;
using Pulse.Plugin.SimpleInfection.Body;
using Pulse.Plugin.SimpleInfection.Infection;
using Pulse.Plugin.Traffic;
using Pulse.Scenery.Cinema;
using Pulse.Scenery.Cinema.Data.Readers;
using Pulse.Scenery.Pulkovo;
using Pulse.Scenery.Train;
using Pulse.Scenery.Train.Route;

namespace Pulse.Model.Environment
{
    public class PulseMap : MapBase, IPulseMap
    {
        public SimulationProperties Properties { get; set; }
        public GeoMapConfig MapConfig { set; get; }
        
        public IDictionary<int, PulseLevel> Levels { get { return Scenery.Levels; } }
        public GeoCartesUtil MapUtils { set; get; }
        public AgentRegistry AgentRegistry { get { return Agents; } }
        public AbstractGeoWorld World { get { return _world; } }
        public PluginsContainer PluginsContainer { get; set; }

        public PulseScenery Scenery { get; set; }
        
        private PluginFactory _plgnf;
        private PulseWorld _world;
        private MovementSystemFactory _msf;
        public  SocialGraphQ SocialNetwork { set; get; }
        
        public PulseMap(SimulationProperties properties, GeoMapConfig mapConfig, GeoCartesUtil mapUtils, PulseScenery scnb, AgentNetworkManager anm, PluginFactory plgnf, PulseWorld world, MovementSystemFactory msf)
            : base(world.Agents)
        {
            Properties = properties;
            MapConfig = mapConfig;
            MapUtils = mapUtils;
            Scenery = scnb;
            _plgnf = plgnf;
            _world = world;
            SocialNetwork = anm.Graph;
            _msf = msf;
        }


        public override void Update(double timeStep, double time)
        {
            UpdateMapPlugins(timeStep, time);
            UpdatePortals(timeStep, time, World.GeoTime.GeoTime);
            UpdateMovementSystems(timeStep, time);



            //OBSOLETE should be refactored

            var isParallel = false;

#if (!DEBUG)
            //            isParallel = true;
#endif

            if (isParallel)
            {
                Agents.AsParallel().ForAll(a => UpdatePluginsAgent(a as AbstractPulseAgent, timeStep, time));
            }
            else
            {
                foreach (var a in Agents)
                    UpdatePluginsAgent(a as AbstractPulseAgent, timeStep, time);
            }

        }

        public override void SetCommand(object command)
        {
            var cmd = command as ICommand;
            if (cmd != null)
            {
                ICommand c = new Command {Args = cmd.Args, Cmd = cmd.Cmd};
                _msf.SetCommand(c);
            }
            else
                Log.Error("Wrong argument format, ignored.");
        }

        public MovementSystem GetMovementSystem(MovementSystemTypes type, int subgroupId = 0)
        {
            return _msf.GetMovementSystem(type, subgroupId);
        }

        private void UpdateMovementSystems(double timeStep, double time)
        {
            _msf.Update(timeStep, time);
        }
        

        private void UpdatePortals(double timeStep, double time, DateTime geotime)
        {
            foreach (var level in Levels)
            {
                foreach (var portal in level.Value.ExternalPortals)
                {
                    portal.Update(timeStep, time, geotime);
                }

                foreach (var poi in level.Value.PointsOfInterest.OfType<IComplexUpdatable>())
                {
                    poi.Update(timeStep, time, geotime);
                }

                if (level.Value.LevelPortals != null)
                foreach (var portal in level.Value.LevelPortals.OfType<IComplexUpdatable>())
                {
                    portal.Update(timeStep, time, geotime);
                }
            }
        }

        private void UpdateMapPlugins(double timeStep, double time)
        {
            foreach (var plgn in PluginsContainer.Plugins.Values.OfType<IUpdatablePlugin>())
            {
                plgn.Update(timeStep, time);
            }
        } 

        [Obsolete]
        private void UpdatePluginsAgent(AbstractPulseAgent agent, double timeStep, double time)
        {
            if (PluginsContainer.Plugins != null)
                foreach (var plugin in PluginsContainer.Plugins)
                    switch (plugin.Value.Name)
                    {
                        case "SimpleInfection":
                            ((IUpdatablePluginMap) plugin.Value).UpdateAgent(agent, timeStep, time);
                            break;
                    }
        }

        [Obsolete]
        private void MoveAgent(AgentBase agentBase, double timeStep, double time)
        {
            var ag = (PulseAgent) agentBase;
            UpdatePluginsAgent(ag, timeStep, time);
        }
        
        public IInfectionData GetInfectionData()
        {
            var infectionData = new InfectionData();
            infectionData.Contacts = new List<InfectionContactData>(AgentRegistry.Count);
            foreach (var agent in Agents.OfType<PulseAgent>())
            {
                if (!agent.PluginsContainer.Plugins.ContainsKey("SimpleInfection")) continue;
                
                var infectionPlgn = (SimpleInfectionPluginAgent) agent.PluginsContainer.Plugins["SimpleInfection"];
                foreach (var contact in infectionPlgn.Contacts)
                {
                    var contactData = new InfectionContactData
                    {
                        Infected = contact.Infected.Agent.Id,
                        Infector = contact.Infector == null ? -1 : contact.Infector.Agent.Id,
                        Point = new PulseVector2(contact.MapPoint.X - 300, contact.MapPoint.Y - 300),
                        GeoPoint = MapUtils.GetGeoCoords(contact.MapPoint),
                        Level = contact.Level,
                        InfectionTime = contact.InfectionTime,
                        ContactDurationSeconds = contact.ContactDuration.TotalSeconds,
                        ContactType = contact.ContactType
                    };

                    infectionData.Contacts.Add(contactData);
                }
            }

            return infectionData;
        }

        public AirportFlightSchedule GetAirportFlightSchedule()
        {
            var sb = Scenery as PulkovoSceneryModule2;
            return sb != null ? sb.GetAirportSchedule() : null;
        }

        public IEnumerable<Station> GetTrainSchedule()
        {
            var sb = Scenery as TrainScenery;
            return sb != null ? sb.GetTrainSchedule() : null;
        }

        public IEnumerable<MovieSession> GetCinemaSchedule()
        {
            var sb = Scenery as CinemaScenery;
            return sb != null ? sb.GetCinemaSchedule() : null;
        }

        public Dictionary<int, List<IPointOfInterest>>[] GetSensorEvolution()
        {
            var sb = Scenery as PulkovoSceneryModule2;
            return sb != null ? sb.GetSensorEvolution() : null;
        }

        public override IMapInfo GetMapInfo()
        {
            var mapInfo = new PulseMapInfo(MapConfig);
            return mapInfo;
        }

        public override IMapData GetMapData()
        {
            var gmd = new PulseMapData(Levels, Scenery.Graph, GetMapInfo() as PulseMapInfo);
            gmd.RegisterPugins(_plgnf.GetPluginsForMapData());
            return gmd;
        }
        
        public override IAgentsData GetAgentsData()
        {
            var agentsData2 = new PulseAgentsData2();
            var agentsDataList2 = new LinkedList<IPulseAgentData2>();

            var scaledAgentRangeMin = byte.MinValue;
            var scaledAgentRangeMax = byte.MaxValue;

            var rangeAgentMin = 0d;
            var rangeAgentMax = 0.01d;

            var scaledObstacleRangeMin = byte.MinValue;
            var scaledObstacleRangeMax = byte.MaxValue;

            var rangeObstacleMin = 0d;
            var rangeObstacleMax = 1d;

            var minPress = double.MaxValue;
            var maxPress = double.MinValue;

            foreach (var agent in Agents.OfType<PulseAgent>().Where(a => a.Point != null))
            {
                bool toadd = true;
                var agentData = new PulseAgentData4
                {
                    Id = agent.Id,
                    Point = agent.Point,
                    Floor = agent.Level,
                    GeoPoint = MapUtils.GetGeoCoords(agent.Point),
                    IsAlive = agent.IsAlive,
                    IsInBuilding = agent.IsInsideBuilding,
                    PhysicalCapabilityClass = agent.PhysicalCapabilityClass.Id,
                    SocialEconomyClass = agent.Role.Id,
                };
                
                if (agent.PluginsContainer.Plugins.ContainsKey("Traffic"))
                {
                    agentData.InfectionState = 1;

//                    int magnIdPref = 11111000;

                    if (agent.Role is SocialDriverRole)
                    {
                        var role = agent.Role as SocialDriverRole;
                        if (role.VehicleState == InVehicleState.CAR)
                        {
//                            agentData.Id = ((int)agent.DesiredSpeedMagnitude).ToString();
                            agentData.SocialEconomyClass = 211;
                        }                       
                    }

                    if (agentData.PhysicalCapabilityClass == 19 ||
                        agentData.PhysicalCapabilityClass == 20 ||
                        agentData.PhysicalCapabilityClass == 21 ||
                        agentData.PhysicalCapabilityClass == 22)
                    {
//                        agentData.Id = ((int) agent.DesiredSpeedMagnitude).ToString();
                        agentData.SocialEconomyClass = agentData.PhysicalCapabilityClass;
                    }
                     else
                        if (agentData.PhysicalCapabilityClass != 7891)
                            if (agent.Point.X > 6700 && agent.Point.Y > 2800 && agent.Point.X < 7100 && agent.Point.Y < 3100)
                            {
                                //TODO micro submodel!11
//                                toadd = false;
                                                            //var p1 = MapUtils.GetCoords(new GeoCoords(59.941743, 30.276741));
                                                            //var p2 = MapUtils.GetCoords(new GeoCoords(59.943805, 30.280975));
                            }
                        }



                if (agent.PluginsContainer.Plugins.ContainsKey("SimpleInfection"))
                {
                    var plgn = (SimpleInfectionPluginAgent) agent.PluginsContainer.Plugins["SimpleInfection"];
                    agentData.InfectionState = (byte) plgn.InfectionStage.InfectionState;
                }
                else
                {
                    agentData.InfectionState = (byte) BaseInfectionStage.InfectionStates.S;
                }

                var actInfo = GetActivityInfo(agent);
                agentData.NavInfo = actInfo;


                var pressure = true;
                if (pressure)
                {
                    var ms = agent.MovementSystem as SfMovementSystem;
                    if (ms != null)
                    {
                        var pressA = ms.GetAgentPressure(agent);
                        var pressO = ms.GetObstaclePressure(agent);
                                                if (pressO < minPress) minPress = pressO;
                                                if (pressO > maxPress) maxPress = pressO;

                        //                        var scaled = scaledRangeMin + ((elementToScale - rangeMin) * (scaledRangeMax - scaledRangeMin) / (rangeMax - rangeMin));

                        var scaledAgentPress = scaledAgentRangeMin + ((pressA - rangeAgentMin) * (scaledAgentRangeMax - scaledAgentRangeMin) / (rangeAgentMax - rangeAgentMin));
                        var scaledObstaclePress = scaledObstacleRangeMin + ((pressO - rangeObstacleMin) * (scaledObstacleRangeMax - scaledObstacleRangeMin) / (rangeObstacleMax - rangeObstacleMin));
                        var scaledTotalPress = (scaledAgentPress + scaledObstaclePress) /2 ;

                        agentData.PhysicalCapabilityClass = (byte)scaledObstaclePress;
                    }
                    else
                        agentData.PhysicalCapabilityClass = 0;
                }

                if (toadd) agentsDataList2.AddLast(agentData);
            }

            agentsData2.Agents = agentsDataList2.ToArray();

            

            return agentsData2;
        }

        private Tuple<byte, PulseVector2, PulseVector2, PulseVector2> GetActivityInfo(PulseAgent agent)
        {
            if (agent.CurrentActivity is SimplePathTravelingActivity)
            {
                var ta = agent.CurrentActivity as SimplePathTravelingActivity;
                return new Tuple<byte, PulseVector2, PulseVector2, PulseVector2>(1, agent.DesiredPosition, ta.SimplePath.First(), ta.SimplePath.Last());
            }

            else if (agent.CurrentActivity is QueueActivity)
            {
                var ta = agent.CurrentActivity as QueueActivity;
                return new Tuple<byte, PulseVector2, PulseVector2, PulseVector2>(1, agent.DesiredPosition, default(PulseVector2), ta.Queueable.TravelPoint);
            }

            else if (agent.CurrentActivity is SimpleActionActivity)
            {
                var ta = agent.CurrentActivity as SimpleActionActivity;
                return new Tuple<byte, PulseVector2, PulseVector2, PulseVector2>(1, agent.DesiredPosition, default(PulseVector2), ta.Building.TravelPoint);
            }

            else if (agent.CurrentActivity is PassiveActionActivity)
            {
                var ta = agent.CurrentActivity as PassiveActionActivity;
                return new Tuple<byte, PulseVector2, PulseVector2, PulseVector2>(1, agent.DesiredPosition, default(PulseVector2), ta.Building.TravelPoint);
            }

            return null;
        }
        
        private void LoadPluginsMap()
        {
//            foreach (var plgn in PluginsContainer.Plugins.Values.OfType<PluginBaseMap>())
//            {
//                plgn.Initialize(this);
//            }
            PluginsContainer.Plugins.Values.OfType<PluginBaseMap>().ToList().ForEach(plgn => plgn.Initialize(this));
        }

        public override void Load()
        {
            Log.Info("Loading map");

#if !DEBUG
            try
            {
#endif
                Scenery.Initialize();
                LoadPluginsMap();
#if !DEBUG
            }
            catch (Exception e)
            {
                Log.Error("Loading map error. Exception: {0}, inner {1}", e.Message, e.InnerException == null ? "null": e.InnerException.Message);
                throw;
            }
#endif
                Log.Info("Map successfuly loaded!");
        }

        public override IAgentsData GetDeadAgentsData()
        {
            var agentsData2 = new PulseDeadAgentsData();
            var agentsDataList2 = new LinkedList<IPulseDeadAgentData>();
            foreach (var agent in World.DeadAgents.OfType<PulseAgent>().Where(a => a.Point != null))
            {
                var agentData = new PulseDeadAgentData
                {
                    Id = agent.Id,
                    Point = agent.Point,
                    Floor = agent.Level,
                    GeoPoint = MapUtils.GetGeoCoords(agent.Point),
                    IsAlive = agent.IsAlive,
                    IsInBuilding = agent.IsInsideBuilding,
                    PhysicalCapabilityClass = agent.PhysicalCapabilityClass.Id,
                    SocialEconomyClass = agent.SocialEconomyClass.Id,
                    TerminationReason = GetTerminationReason(agent)
                };

                if (agent.PluginsContainer.Plugins.ContainsKey("SimpleInfection"))
                {
                    var plgn = (SimpleInfectionPluginAgent)agent.PluginsContainer.Plugins["SimpleInfection"];
                    agentData.InfectionState = (byte)plgn.InfectionStage.InfectionState;
                }

                agentsDataList2.AddLast(agentData);
            }
            agentsData2.Agents = agentsDataList2.ToArray();

            return agentsData2;
        }

        public IEnumerable<IPointOfInterest> GetInfectionSources()
        {
            var plgn = (SimpleInfectionPluginMap)PluginsContainer.Plugins["SimpleInfection"];
            return plgn.NullPatients.Select(a => a.Agent.Home);
        }

        private int GetTerminationReason(PulseAgent agent)
        {
            if (agent.TerminationReasonInfo == null)
                return 0;
            if (agent.TerminationReasonInfo == "migration_gate")
                    return 2;
            if (agent.TerminationReasonInfo == "migration_exit")
                    return 3;
            return 0;
        }
    }
}