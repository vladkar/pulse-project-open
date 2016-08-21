using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public class SimpleMovementSystem : Common.Model.Agent.MovementSystem
    {
        public IDictionary<long, SimpleMovementSystemUnitWrapper> AgentDataRegister { get; }

        public SimpleMovementSystem(PulseScenarioConfig config, PulseScenery sb, GeoCartesUtil gcu) : base(config, sb)
        {
            MapUtils = gcu;
            AgentDataRegister = new ConcurrentDictionary<long, SimpleMovementSystemUnitWrapper>();
        }

        public GeoCartesUtil MapUtils { get; set; }

        protected override void LoadData()
        {
        }

        public override void Update(double timeStep, double time)
        {
            foreach (var agentKvp in AgentsRegister)
            {
                var agWrapper = AgentDataRegister[agentKvp.Key];
                var agent = agentKvp.Value;
                
                PulseVector2 currentPoint = default(PulseVector2);

                if (agWrapper.Path.Count == 1)
                    currentPoint = agWrapper.Path.First();

                else { 

//                if (!agent.Point.Equals(agWrapper.Path.Last()))
//                {
                    var speed = agent.PhysicalCapabilityClass.Speed*1000d/3600; // meters per second
                    var distStep = speed*timeStep/MapUtils.MetersPerMapUnit;
                    var distanceAccumulator = 0d;
                    agWrapper.TraveledDistance += distStep;

                    for (var i = 0; i < agWrapper.Path.Count - 1; i++)
                    {
                        var A = agWrapper.Path[i];
                        var B = agWrapper.Path[i + 1];

                        var distOfSeg = A.DistanceTo(B);


                        //current segment
                        if (agWrapper.TraveledDistance < distanceAccumulator + distOfSeg)
                        {
                            var traveledDistanceOnSegment = agWrapper.TraveledDistance - distanceAccumulator;

//                            var product = Math.Sqrt(Math.Pow(B.X - A.X, 2) + Math.Pow(B.Y - A.Y, 2));
//                            var x = A.X + traveledDistanceOnSegment*(B.X - A.X)/product;
//                            var y = A.Y + traveledDistanceOnSegment*(B.Y - A.Y)/product;

                            //big timestep distance overflow control
                            if (agWrapper.TraveledDistance >= agWrapper.RouteDistance)
                                currentPoint = agWrapper.Path.Last();
                            else
                                currentPoint = ClipperUtil.GetDistancePointOnGgment(A, B, traveledDistanceOnSegment);
                            break;
                        }
                            //pass segment
                        else
                        {
                            distanceAccumulator += distOfSeg;
                        }
                    }

                    if (agWrapper.TraveledDistance >= agWrapper.RouteDistance)
                        currentPoint = agWrapper.Path.Last();
                }


                agent.Move(currentPoint);
            }
        }

        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister[agent.Id] = agent;
            }

            AgentDataRegister[agent.Id] = new SimpleMovementSystemUnitWrapper(agent, unit);
        }

        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister.Remove(agent.Id);
                AgentDataRegister.Remove(agent.Id);
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

    public class SimpleMovementSystemUnitWrapper
    {
        private AbstractPulseAgent _agent;

        public double TraveledDistance { get; set; }
        public double RouteDistance { set; get; }
        public IList<PulseVector2> Path { get; set; }

        public SimpleMovementSystemUnitWrapper(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            _agent = agent;
            Path = (unit as SimplePathTravelingActivity).SimplePath;
            RouteDistance = CalculateRouteDistance(Path);
        }

        private double CalculateRouteDistance(IList<PulseVector2> path)
        {
            var dist = 0d;

            for (var i = 0; i < path.Count - 1; i++)
            {
                dist += path[i].DistanceTo(path[i + 1]);
            }

            return dist;
        }
    }
}
