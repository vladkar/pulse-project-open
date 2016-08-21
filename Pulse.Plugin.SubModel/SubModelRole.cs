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
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Plugin.SubModel
{
    public class SubModelRole : AbstractAgentRole
    {
        public SubModelRole(AbstractPulseAgent agent) : base(agent, "SubModel role", 771)
        {
        }

        public override void CheckSchedule()
        {
        }

        public override PlannedDailyAgentSchedule GetPlannedSchedule()
        {
            return null;
        }

        public override IList<AbstractScheduleActivity> GetAbstractSchedule()
        {
            return null;
        }

        public override CurrentActivity GetActivity()
        {
            return null;
        }

        public override void CheckActivity()
        {
        }

        public override void DoActivity()
        {
        }

        public override void GetDesiredPosition()
        {
            _agent.DesiredPosition = _agent.Point;
        }

        public override void StartInteract(IPointOfInterest poi)
        {
        }

        public override void StopInteract(IPointOfInterest poi)
        {
        }
    }
}
