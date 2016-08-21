using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Interval;

namespace $pulsepre$$safeprojectname$
{
    public class $safeprojectname$SimpleRole : SimpleScheduleRole 
    {
        public $safeprojectname$SimpleRole(AbstractPulseAgent agent) : 
            base(agent, new SimpleScheduleRoleConfig {CornerCutOffRadius = 2, PoiInteractionRadius = 0.5, DeadZoneRadius = 0.01},
                "$safeprojectname$ Simple Agent Role", IdUtil.NextRandomId()) { }

        public override IList<AbstractScheduleActivity> GetAbstractSchedule ()
        {
            #region possible activities

            var abstract_poi_activity = new AbstractPoiActivity
            {
                ActivityType = "use_poi",
                Name = "Use poi",
                PossiblePoiTypes = _agent.WorldKnowledge.PossiBuildingTypes.Where(t => t.Name == "exmaple_poi").ToList()
            };

            var abstract_exit_activity = new AbstractPoiActivity
            {
                ActivityType = "exit",
                Name = "Exit",
                PossiblePoiTypes =
                    _agent.WorldKnowledge.PossiBuildingTypes.Where(t => t.Name == "example_killer").ToList()
            };

            #endregion

            #region schedule activities

            var schedule_poi_activity = new AbstractScheduleActivity
            {
                PoiActivity = abstract_poi_activity,
                Rate = 5,
                Name = abstract_poi_activity.Name,
                TimeDuration = new MinuteInterval(1, 5)
            };

            var schedule_exit_activity = new AbstractScheduleActivity
            {
                PoiActivity = abstract_exit_activity,
                Rate = 100,
                Name = abstract_exit_activity.Name
            };

            #endregion

            var schedule = new List<AbstractScheduleActivity> {schedule_poi_activity, schedule_exit_activity};

            /* multiple schedules */
            /*
            var schdls = new Dictionary<int, List<AbstractScheduleActivity>>
            {
                { 100, walkSch }
            };
            var sch = schdls.ProportionChoise(s => s.Key).Value;
            */
            

            return schedule;
        }

        public override PlannedDailyAgentSchedule GetPlannedSchedule ()
        {
            var sch = new PlannedDailyAgentSchedule();

            sch.ScheduledDay = _agent.WorldKnowledge.Clock.Date;
            sch.AbstractAgentSchedule = GetAbstractSchedule();

            for (int i = 0; i < sch.AbstractAgentSchedule.Count; i++)
            {
                var a = sch.AbstractAgentSchedule[i];
                if (RandomUtil.ToBeOrNot(a.Rate, 100))
                {
                    var plannedActivity = new PlannedActivity
                    {
                        Activity = a,
                        PlannedStartTime = _agent.WorldKnowledge.Clock + new TimeSpan(0, i, 0),
                        Poi =
                            _agent.WorldKnowledge.GetTypedBuildingsInRadius(
                                a.PoiActivity.PossiblePoiTypes, _agent.Point, 5000)
                                .RandomChoise()
                    };

                    sch.PlannedDailySchedule.Add(plannedActivity);
                }
            }

            return sch;
        }
    }
}