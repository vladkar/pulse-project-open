using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Utils;

namespace Pulse.Population
{
    public class SocialRole : SimpleScheduleRole
    {
        public SocialRole(AbstractPulseAgent agent)
            : base(agent, "Social Role", 0)
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
            //            if (WorldKnowledge.Clock.DayOfWeek == DayOfWeek.Saturday || WorldKnowledge.Clock.DayOfWeek == DayOfWeek.Sunday)
            //                return this.AbstractClassSchedule.Weekends;
            //            else
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

            // 1. Queue is not empty?
            // 1 + : queue is not empty
            if (PlannedDailyAgentSchedule.PlannedDailySchedule.Count > 0)
            {
                var nextAction = PlannedDailyAgentSchedule.PlannedDailySchedule.First();

                // 2. Already at goal building?
                // 2 + : at goal building
                if (_agent.Point == nextAction.Poi.TravelPoint)
                {
                    //                    CurrentBuilding = nextAction.Building;
                    var first = PlannedDailyAgentSchedule.PlannedDailySchedule.First();
                    PlannedDailyAgentSchedule.PlannedDailySchedule.Remove(first);

                    //2.1 Interactable building?
                    //2.1 - : not interactable
                    var interactable = nextAction.Poi as IInteractable;
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
                        return _currentActiityFactory.GetComplexTravelingActivity(nextAction.Poi.TravelPoint, nextAction.Poi.Level);
                    }
                }
            }
            // 1 - : queue is empty 
            else
            {
                if (_agent.Point == _agent.Home.TravelPoint)
                    return _currentActiityFactory.GetWaitingActivity(new TimeSpan(0, 0, 15));
                else
                    return _currentActiityFactory.GetComplexTravelingActivity(_agent.Home.TravelPoint, _agent.Home.Level);
            }
        }

        #endregion

    }
}
