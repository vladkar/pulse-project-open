using System;
using System.Collections.Generic;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment.Poi;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.AgentScheduling.Planned
{
    public class PlannedDailyAgentSchedule
    {
        public IList<AbstractScheduleActivity> AbstractAgentSchedule { get; set; }
        public SortedSet<PlannedActivity> PlannedDailySchedule { get; set; }
        public DateTime ScheduledDay { get; set; }

        public PlannedDailyAgentSchedule()
        {
            PlannedDailySchedule = new SortedSet<PlannedActivity>(new PlannedAgentActivityComparer());
        }

        public class PlannedAgentActivityComparer : IComparer<PlannedActivity>
        {
            public int Compare(PlannedActivity x, PlannedActivity y)
            {
                if (x.PlannedStartTime > y.PlannedStartTime)
                    return 1;
                if (x.PlannedStartTime < y.PlannedStartTime)
                    return -1;
                return 0;
            }
        }
    }
}
