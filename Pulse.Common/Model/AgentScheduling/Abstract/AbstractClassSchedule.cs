using System.Collections.Generic;

namespace Pulse.Common.Model.AgentScheduling.Abstract
{
    public class AbstractClassSchedule
    {
        public string Name { set; get; }
        public int Raiting { set; get; }
        public IList<AbstractScheduleActivity> Weekdays { set; get; }
        public IList<AbstractScheduleActivity> Weekends { set; get; }
    }
}
