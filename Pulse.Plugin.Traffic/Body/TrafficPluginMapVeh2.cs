using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EmbeddableTraffic;
using MultiagentEngine.Map;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;
using Pulse.Social.Population;
using SimpleTraffic;
using SimpleTraffic.Buses;

namespace Pulse.Plugin.Traffic.Body
{
    public class TrafficPluginMap : PluginBaseMap, IUpdatablePluginMap, ILegendable
    {
        private EmbeddableTrafficEngine _engine;
        private ICollection<VehicleAgent> _newAgents;
        private TrafficMovementSystem2D _vms;

        private IDictionary<ITrafficAgentInfo, AbstractPulseAgent> _vehicleRegistry;

        private TrafficScenarioConfig _config;

        private Task _currentTask;

        public TrafficPluginMap(TrafficScenarioConfig config)
        {
            Name = GlobalStrings.PluginName;
            _newAgents = new LinkedList<VehicleAgent>();
            _engine = new EmbeddableTrafficEngine();
            _config = config;

            var equalityComparer =
                new FuncEqualityComparer<ITrafficAgentInfo>(
                    new Func<ITrafficAgentInfo, ITrafficAgentInfo, bool>((a, b) => a == b),
                    new Func<ITrafficAgentInfo, int>((a) => a.GetHashCode()));

            _vehicleRegistry = new Dictionary<ITrafficAgentInfo, AbstractPulseAgent>(equalityComparer);
        }

        public Tuple<GeoCoords, GeoCoords, uint>[] GetRoadGraph()
        {
            var g = _engine.GetRoadGraph();
            return
                g.Select(
                    e =>
                        new Tuple<GeoCoords, GeoCoords, uint>(
                            new GeoCoords(e.StartPosition.Latitude, e.StartPosition.Longitude),
                            new GeoCoords(e.EndPosition.Latitude, e.EndPosition.Longitude), e.Id)).ToArray();
        }

        public Dictionary<uint, float> GetGraphLoad()
        {
            return _engine.GetRoadGraphLoad();
        }

        public void Update(double timeStep, double time)
        {
//            Task.WaitAll(new[] {_currentTask});
//            _currentTask.Wait();

            _engine.AddNewAgents(_newAgents);
            _newAgents = new LinkedList<VehicleAgent>();

            //_engine.Step();

            var allVvehicles = _engine.GetVehiclesShort();

            var _deadVehicles = _engine.AgentsTerminatedInLastIteration;
            var _newVehicles = _engine.AgentsAddedInLastIteration;
            
            foreach (var guestAgent in _newVehicles)
            {
                var newAgent = (Map.World.Engine.GenerationModel as IPulseAgentGenerationModel).CreateNewAgent();

                newAgent.Point = Map.MapUtils.GetCoordsTuple(new GeoCoords(guestAgent.Position.Latitude, guestAgent.Position.Longitude));
                newAgent.Level = 1;
                newAgent.IsInsideBuilding = false;
                newAgent.CurrentActivity = null;

                switch (guestAgent.VehicleType)
                {
                    case VehicleType.bus:
                        newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass { Id = 19 };
                        break;
                    case VehicleType.tram:
                        newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass { Id = 20 };
                        break;
                    case VehicleType.trolleybus:
                        newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass { Id = 21 };
                        break;
                    case VehicleType.shutlebus:
                        newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass { Id = 22 };
                        break;
                    case VehicleType.car:
                        newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass { Id = 23 };
                        break;
                    default:
                        throw new Exception("Unknown transport type");
                }

//                newAgent.DesiredSpeedMagnitude = guestAgent.DirectionAngle;

                newAgent.Home =
                    newAgent.WorldKnowledge.GetClosestPointOfInterest(
                        newAgent.WorldKnowledge.PossiBuildingTypes.RandomChoise(), newAgent.Point);
//                newAgent.Home = null;
                newAgent.Role.PlannedDailyAgentSchedule = new PlannedDailyAgentSchedule{AbstractAgentSchedule = new AbstractScheduleActivity[0], PlannedDailySchedule = new SortedSet<PlannedActivity>()};

                newAgent.CurrentActivity = new TrafficAgentActivity
                {
//                    OriginWorldName = guestAgent.World.Name,
//                    OriginAgentId = guestAgent.Id
                };

                (Map.World as PulseWorld).AddNewAgent(newAgent);
                _vehicleRegistry[guestAgent] = newAgent;
            }

            foreach (var guestAgent in _deadVehicles)
            {
                var agent = _vehicleRegistry[guestAgent];
                _vehicleRegistry.Remove(guestAgent);
                agent.DoneActivity();
                agent.Kill("Traffic_finish");
            }

            foreach (var guestAgent in allVvehicles)
            {
                var agent = _vehicleRegistry[guestAgent];

                agent.Move(Map.MapUtils.GetCoordsTuple(new GeoCoords(guestAgent.PositionOnLane.Latitude, guestAgent.PositionOnLane.Longitude)));
//                agent.DesiredSpeedMagnitude = guestAgent.DirectionAngle;
            }

            foreach (var ag in _engine.AgentsFinishedAfterStep)
            {
                var agentsId = ag.PassengersExternalIds;
                foreach (var agentId in agentsId)
                {
                    var agent = Map.AgentRegistry.GetById(Int64.Parse(agentId)) as AbstractPulseAgent;
                    agent.DoneActivity(); 
                }
            }

//            _currentTask = Task.Factory.StartNew(_engine.Step);

            _engine.Step();;
        }

        public IDictionary<long, PulseVector2> GetAgetnsPositions()
        {
            return _engine.GetAllExternalAgentsPositions().ToDictionary(a => Int64.Parse(a.Key), a => GeoCoordsToCoords(a.Value));
        } 

        //TODO remove method
        public void UpdateAgent(AbstractPulseAgent agent, double timeStep, double time = -1)
        {
            
        }

        public void AddAgent(AbstractPulseAgent agent, PulseVector2 start, PulseVector2 dest)
        {
            var veh = new VehicleAgent
            {
                Origin = _engine.GetCoordinateProjection(CoordsToGeoCoords(start)),
                Destination = _engine.GetCoordinateProjection(CoordsToGeoCoords(dest)),
                PassengersExternalIds = new[] { agent.Id.ToString() }
            };

            _newAgents.Add(veh);
        }

        public PulseVector2 GetCarLocation(PulseVector2 point)
        {
            var car = _engine.GetClosestNodeTo(CoordsToGeoCoords(point));
            return GeoCoordsToCoords(car.Position);

//            return null;
        }

        private PulseVector2 GeoCoordsToCoords(GeoCoordinate p)
        {
            return Map.MapUtils.GetCoordsTuple(GeoCoordsToGeoCoords(p));
        }

        private GeoCoordinate CoordsToGeoCoords(PulseVector2 p)
        {
            return GeoCoordsToGeoCoords(Map.MapUtils.GetGeoCoords(p));
        }

        private GeoCoords GeoCoordsToGeoCoords(GeoCoordinate p)
        {
            return new GeoCoords(p.Latitude, p.Longitude);
        }

        private GeoCoordinate GeoCoordsToGeoCoords(GeoCoords p)
        {
            return new GeoCoordinate(p.Lat, p.Lon);
        }

        public override void Initialize(IPulseMap map)
        {
            base.Initialize(map);

            _vms = Map.GetMovementSystem(MovementSystemTypes.VEHICLE) as TrafficMovementSystem2D;
            _vms.MapPlugin = this;

            var config = new EmbeddableTrafficEngineConfig  
            {
                RoadGraphFilePath = "zsd.XML",
                SimulationStartsAt = 21600,
                TimeStep = _config.BaseConfig.TimeStep.Value,
                UseAdditionalTraffic = false,
                AdditionalTrafficFilePath = "Data\\zsd.100k.bson",
                PublicTransportIntensity = 0.1
            };

            _engine.InitializeEngine(config);

            var type = new PointOfInterestType {Id = 77777, Name = "busstop"};
            var stops = _engine.PublicTransportDepot.AllStops;

            var gcu = Map.MapUtils;
            var pulseStops = stops.Select( s => new PointOfInterest
            {
                Types = new[] {type}, 
                ObjectId = IdUtil.NextRandomId().ToString(),
                Point = gcu.GetCoordsTuple(new GeoCoords(s.Position.Latitude, s.Position.Longitude)),
                Polygon = new[] { gcu.GetCoordsTuple(new GeoCoords(s.Position.Latitude, s.Position.Longitude)) },
                NavgationBlock = new PoiNavigationBlockPoint(new PulseVector2(500, 500)),
                Level = 1
            }).ToList();

            pulseStops.ForEach(map.Scenery.Levels[1].PointsOfInterest.Add);
            map.Scenery.Graph.AddPoisToGraph(pulseStops);
            
            map.Scenery.Graph.BuildKdTree();

//            _currentTask = new Task(_engine.Step);
//            _currentTask.Start();
            _engine.Step();
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>>();
        }
    }

    public class TrafficAgentActivity : CurrentActivity
    {
    }
}
