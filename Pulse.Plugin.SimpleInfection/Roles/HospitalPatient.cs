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
    public class HospitalPatient : SimpleScheduleRole
    {
        public HospitalPatient(AbstractPulseAgent agent)
            : base(agent, "Госпитализированный", 61)
        {
        }

        public override PlannedDailyAgentSchedule GetPlannedSchedule()
        {
            var sch = new PlannedDailyAgentSchedule();
            sch.ScheduledDay = _agent.WorldKnowledge.Clock.Date;

            var plannedActivity = new PlannedActivity();
            plannedActivity.PlannedStartTime = _agent.WorldKnowledge.Clock.Date;
            var type = _agent.WorldKnowledge.PossiBuildingTypes.First(t => t.Name == "больница, госпиталь, стационар");
            plannedActivity.Poi = _agent.WorldKnowledge.GetClosestPointOfInterest(type, _agent.Point);

            return sch;
        }

        public override IList<AbstractScheduleActivity> GetAbstractSchedule()
        {
            throw new NotImplementedException();
        }

        public override CurrentActivity GetActivity()
        {
            var a = new SimpleActionActivity();
            var type = _agent.WorldKnowledge.PossiBuildingTypes.First(t => t.Name == "больница, госпиталь, стационар");
            a.Building = _agent.WorldKnowledge.GetClosestPointOfInterest(type, _agent.Point);
            a.RealStartTime = _agent.WorldKnowledge.Clock.Date;
            a.PlannedEndTime = a.RealStartTime + new TimeSpan(8, 0, 0);
            return a;
        }
    }
}
