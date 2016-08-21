using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;
using Pulse.Plugin.Traffic.Body;

namespace Pulse.Plugin.Traffic
{
    public enum InVehicleState { PEDESTRIAN = 0, CAR = 1, BUS = 2 }
    
    public class SocialDriverRole : SimpleScheduleRole
    {
        public InVehicleState VehicleState { set; get; }

        public SocialDriverRole(AbstractPulseAgent agent)
            : base(agent, "Social Driver Role", 0)
        {
            if (agent.SocialEconomyClass != null)
            {
                _name = agent.SocialEconomyClass.Name;
                _id = agent.SocialEconomyClass.Id;
            }
        }

        #region schedule

        public override void CheckSchedule()
        {
            if (PlannedDailyAgentSchedule == null ||
                _agent.WorldKnowledge.Clock - PlannedDailyAgentSchedule.ScheduledDay > new TimeSpan(24, 2, 0))
            {
                PlannedDailyAgentSchedule = GetPlannedSchedule();
            }
        }

        public override IList<AbstractScheduleActivity> GetAbstractSchedule()
        {
            return AbstractClassSchedule.Weekdays;
        }

        public override PlannedDailyAgentSchedule GetPlannedSchedule()
        {
            var sch = new PlannedDailyAgentSchedule();

            sch.ScheduledDay = _agent.WorldKnowledge.Clock.Date;
            sch.AbstractAgentSchedule = GetAbstractSchedule();
            for (int i = 0; i < sch.AbstractAgentSchedule.Count; i++)
            {
                var a = sch.AbstractAgentSchedule[i];
                if (RandomUtil.ToBeOrNot(a.Rate, 100))
                {
                    var plannedActivity = new PlannedActivity();

                    plannedActivity.Poi = _agent.WorldKnowledge.GetTypedBuildingInRadius(a.PoiActivity.PossiblePoiTypes,
                        _agent.Home.TravelPoint, 30000 / _agent.WorldKnowledge.MetersPerMapUnit);

                    plannedActivity.Activity = a;

                    #region time
                    if (a.TimeStart != null & a.TimeEnd != null)
                    {
                        plannedActivity.PlannedStartTime = _agent.WorldKnowledge.Clock.Date + a.TimeStart.GetRandom();
                        plannedActivity.PlannedEndTime = _agent.WorldKnowledge.Clock.Date + a.TimeEnd.GetRandom();
                    }
                    else
                    {
                        if (a.TimeDuration != null)
                        {
                            //TODO calculate travel time from current or from home
                            var estimatedTravelTime = new TimeSpan(0, 20, 0);
                            if (sch.PlannedDailySchedule.Count > 0)
                            {
                                plannedActivity.PlannedStartTime = sch.PlannedDailySchedule.First().PlannedEndTime +
                                                            estimatedTravelTime;
                            }
                            else
                            {
                                var wakeUpTime = new TimeSpan(7, 0, 0);
                                var today = _agent.WorldKnowledge.Clock.Date;
                                plannedActivity.PlannedStartTime = today + wakeUpTime + estimatedTravelTime;
                            }
                            var duration = new TimeSpan(0, a.TimeDuration.GetRandom(), 0);
                            plannedActivity.PlannedEndTime = plannedActivity.PlannedStartTime + duration;
                        }
                        else
                        {
                            throw new Exception(String.Format("Virtual Society Agent ({0}) can't build daily schedule, Socio-economical class: {1}: {2}", _agent.Id, _agent.SocialEconomyClass.Name, AbstractClassSchedule.Name));
                        }
                    }
                    #endregion

                    sch.PlannedDailySchedule.Add(plannedActivity);
                }

                //TODO disabled: sleeping etc
            }

            return sch;
        }

        #endregion

        #region activity
        
        public override CurrentActivity GetActivity()
        {
            //config
            var earlyArrivalGandigap = new TimeSpan(0, 15, 0);
            var speed = _agent.PhysicalCapabilityClass.Speed * 1000d / 3600; // meters per second
            var minTouchDistance = 10;

            // 1. Queue is not empty?
            // 1 + : queue is not empty
            if (PlannedDailyAgentSchedule.PlannedDailySchedule.Count > 0)
            {
                var nextAction = PlannedDailyAgentSchedule.PlannedDailySchedule.First();

                // 2. Already at goal building?
                // 2 + : at goal building
                if (_agent.Point.DistanceTo(nextAction.Poi.TravelPoint) < minTouchDistance & _agent.Level == nextAction.Poi.Level)
                {
                    var first = PlannedDailyAgentSchedule.PlannedDailySchedule.First();
                    PlannedDailyAgentSchedule.PlannedDailySchedule.Remove(first);

                    //2.1 Interactable building?
                    //2.1 - : not interactable
                    var interactable = nextAction.Poi as IActivePoi;
                    if (interactable == null)
                        return _currentActiityFactory.GetSimpleActionActivity(first);
                    //2.1 + : interactable
                    else
                        return _currentActiityFactory.GetPassiveActionActivity(first);
                }
                // 2 - : not at goal building
                else
                {
                    var distanceFromCurrentPosToGoal = _agent.Point.DistanceTo(nextAction.Poi.TravelPoint) *
                                                _agent.WorldKnowledge.MetersPerMapUnit; // meters
                    var estimatedTravlTimeFromCurrentPosToGoal = new TimeSpan(0, 0, (int)(distanceFromCurrentPosToGoal / speed));

                    var elapsedTime = nextAction.PlannedStartTime - _agent.WorldKnowledge.Clock;

                    // 3. Should waiting?
                    // 3 + : waiting
                    if (elapsedTime > estimatedTravlTimeFromCurrentPosToGoal + earlyArrivalGandigap)
                    {
                        var distanceFromCurrentToHome = _agent.Point.DistanceTo(_agent.Home.TravelPoint) *
                                                 _agent.WorldKnowledge.MetersPerMapUnit; // meters
                        var estimatedTravlTimeFromCurrentToHome = new TimeSpan(0, 0, (int)(distanceFromCurrentToHome / speed));

                        var distanceFromHomeToGoal = _agent.Home.TravelPoint.DistanceTo(nextAction.Poi.TravelPoint) *
                                                 _agent.WorldKnowledge.MetersPerMapUnit; // meters
                        var estimatedTravlTimeFromHomeToGoal = new TimeSpan(0, 0, (int)(distanceFromHomeToGoal / speed));

                        // 3.1. Should visit home before next action?
                        // 3.1 + : waiting at home (and not at home)
                        if (elapsedTime >
                            estimatedTravlTimeFromCurrentToHome + estimatedTravlTimeFromHomeToGoal +
                            earlyArrivalGandigap + earlyArrivalGandigap & _agent.Point != _agent.Home.TravelPoint)
                        {
                            return _currentActiityFactory.GetComplexTravelingActivity(_agent.Home.TravelPoint, _agent.Home.Level);
                        }
                        // 3.1 - : waiting here
                        else
                        {
                            return _currentActiityFactory.GetWaitingActivity(elapsedTime + earlyArrivalGandigap);
                        }
                    }
                    // 3 - : go to next action goal
                    else
                    {
                        if (distanceFromCurrentPosToGoal > 1000)
                            return ProcessComplexTreavelingVehicleActivity(nextAction.Poi);
                        else
                            return _currentActiityFactory.GetComplexTravelingActivity(nextAction.Poi.TravelPoint, nextAction.Poi.Level);
                    }
                }
            }
            // 1 - : queue is empty 
            else
            {
                if (_agent.Point.DistanceTo(_agent.Home.TravelPoint) < minTouchDistance &
                    _agent.Level == _agent.Home.Level)
                {
                        return _currentActiityFactory.GetHomeActivity(new TimeSpan(0, 15, 0));
                }
                else
                    return _currentActiityFactory.GetComplexTravelingActivity(_agent.Home.TravelPoint, _agent.Home.Level);
            }
        }

        private CurrentActivity ProcessComplexTreavelingVehicleActivity(IPointOfInterest dest)
        {
            var plgn = _agent.PluginsContainer.Plugins["Traffic"] as TrafficPluginAgent;

            var cta = _currentActiityFactory.GetComplexTravelingActivity(dest.TravelPoint, dest.Level) as ComplexTravelingActivity;

            var va = new VehicleActivity
            {
                TotalDestination = cta.Point,
                VehicleOrigin = plgn.GetCarLocation(),
                Parent = cta
            };

            var fromOriginToVehicle = _currentActiityFactory.GetComplexTravelingActivity(va.VehicleOrigin, 1) as ComplexTravelingActivity;
            fromOriginToVehicle.Parent = cta;

            var fromVehicletoGoal = _currentActiityFactory.GetComplexTravelingActivity(dest.TravelPoint, dest.Level) as ComplexTravelingActivity;
            fromVehicletoGoal.Parent = cta;

            cta.Path = new List<ITravelPath> { fromOriginToVehicle, va, fromVehicletoGoal };

            return cta;
        }

        public override void DoActivity()
        {
            //config
            var minTouchDistance = 10;

         
            if (_agent.CurrentActivity is HomeActivity)
            {
                var ca = _agent.CurrentActivity as HomeActivity;
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(_agent.Home);
                    ca.State = ActivityState.Started;
                }

                if (ca.PlannedEndTime <= _agent.WorldKnowledge.Clock)
                    _agent.DoneActivity();
            }

            else if (_agent.CurrentActivity is WaitingActivity)
            {
                var ca = _agent.CurrentActivity as WaitingActivity;
                if (ca.PlannedEndTime <= _agent.WorldKnowledge.Clock)
                    _agent.DoneActivity();
            }

            else if (_agent.CurrentActivity is SimpleActionActivity)
            {
                var ca = _agent.CurrentActivity as SimpleActionActivity;

                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(ca.Building);
                    ca.State = ActivityState.Started;
                }
                else if (ca.State == ActivityState.Started)
                {
                    if (ca.PlannedEndTime <= _agent.WorldKnowledge.Clock)
                    {
                        _agent.DoneActivity();
                    }
                }
            }

            else if (_agent.CurrentActivity is PassiveActionActivity)
            {
                var ca = _agent.CurrentActivity as PassiveActionActivity;

                // 1. Started?
                // 1.1 - : not started
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(ca.Building);
                    ca.State = ActivityState.Started;
                }
                else if (ca.State == ActivityState.Started)
                {
                    //processing by poi
                }
            }

            else if (_agent.CurrentActivity is ComplexTravelingActivity)
            {
                var ca = _agent.CurrentActivity as ComplexTravelingActivity;
                
                // 1. Already at goal?
                // 1 + : at goal
                if (_agent.Point.DistanceTo(ca.Point) < minTouchDistance & _agent.Level == ca.Level)
                {
                    _agent.CurrentActivity.State = ActivityState.Finished;
                }

                // 1 - : not at goal
                else
                {
                    // 1.2. Is path calculated?
                    // 1.2.-. : not calculated
                    if (ca.Path == null)
                    {
                        // 1.2.1. Is path possible?
                        // 1.2.1.+. : possible, ok
                        try
                        {
                            ca.Path = _agent.CalculatePath(_agent.Point, _agent.Level,
                                ca.Point, ca.Level);
                        }
                        // 1.2.1.-. : impossible, kill agent
                        catch (Exception e)
                        {
                            _agent.Kill("bad_path");
                            ca.Path = new List<ITravelPath>();
                            Log.Error(
                                "Pathfinding trouble. Agent: {0}, Start pos: (point: {1}, level: {2}), Dest pos: (point: {3}, level: {4})",
                                _agent.Id, _agent.Point, _agent.Level, ca.Point,
                                ca.Level);
                            Log.Error("Pathfinding trouble. Exception: {0}, inner exception {1}", e.Message,
                                e.InnerException == null ? "null" : e.InnerException.Message);
                        }
                    }

                    // 1.2.+. : calculated and existed path: processing complex path
                    else if (ca.Path.Count > 0)
                    {
                        var firstSub = _currentActiityFactory.GetTravelingActivityFromTravelingUnit(ca.Path[0]);
                        var prev = firstSub;

                        for (var i = 1; i < ca.Path.Count; i++)
                        {
                            var next = _currentActiityFactory.GetTravelingActivityFromTravelingUnit(ca.Path[i]);

                            prev.Next = next;
                            next.Prev = prev;

                            prev = next;
                        }

                        ca.Child = firstSub;
                        firstSub.Parent = ca;
                    }
                }
            }

            else if (_agent.CurrentActivity is SimplePathTravelingActivity)
            {
                var ca = _agent.CurrentActivity as SimplePathTravelingActivity;

                if (_agent.Point.DistanceTo(ca.SimplePath.Last()) < minTouchDistance)
                {
                    _agent.CurrentActivity.State = ActivityState.Finished;
                }
                else if (ca.State == ActivityState.NotStarted)
                {
                    _agent.SetMovementSystem(_agent.CurrentActivity as UnitTravelingActivity);
                    ca.State = ActivityState.Started;
                }
            }

            else if (_agent.CurrentActivity is SimplePortalActivity)
            {
                var ca = _agent.CurrentActivity as SimplePortalActivity;

                // 1. Started?
                // 1.1 - : not started
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.StartInteractWithPoi(ca.Enter);
                    ca.State = ActivityState.Started;
                }
                else if (ca.State == ActivityState.Started)
                {
                    //processing by poi
                }
            }

            else if (_agent.CurrentActivity is VehicleActivity)
            {
                var ca = _agent.CurrentActivity as VehicleActivity;

                // 1. Started?
                // 1.1 - : not started
                if (ca.State == ActivityState.NotStarted)
                {
                    _agent.SetMovementSystem(_agent.CurrentActivity as UnitTravelingActivity); 
                    ca.State = ActivityState.Started;
                    VehicleState = InVehicleState.CAR;
                    ca.OnFinish += () => VehicleState = InVehicleState.PEDESTRIAN;
                }
                else if (ca.State == ActivityState.Started)
                {
                    
                    //processing by movement system
                }
            }
        }

        #endregion

    }
}
