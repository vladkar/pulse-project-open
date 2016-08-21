using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;

namespace Pulse.Plugin.SimpleInfection.Roles
{
    public class HomePatient : SimpleScheduleRole
    {
        public HomePatient(AbstractPulseAgent agent)
            : base(agent, "Домашний режим", 62)
        {
        }

        public override PlannedDailyAgentSchedule GetPlannedSchedule()
        {
            var sch = new PlannedDailyAgentSchedule();
            sch.ScheduledDay = _agent.WorldKnowledge.Clock.Date;

            var plannedActivity = new PlannedActivity();
            plannedActivity.PlannedStartTime = _agent.WorldKnowledge.Clock.Date;
            plannedActivity.Poi = _agent.Home;

            return sch;
        }

        public override IList<AbstractScheduleActivity> GetAbstractSchedule()
        {
            throw new NotImplementedException();
        }

        public override CurrentActivity GetActivity()
        {
            var a = new SimpleActionActivity();
            a.Building = _agent.Home;
            a.RealStartTime = _agent.WorldKnowledge.Clock.Date;
            a.PlannedEndTime = a.RealStartTime + new TimeSpan(8, 0, 0);
            return a;
        }
    }
}
