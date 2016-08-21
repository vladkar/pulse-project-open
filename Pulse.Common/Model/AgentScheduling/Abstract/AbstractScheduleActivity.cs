using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Utils.Interval;

namespace Pulse.Common.Model.AgentScheduling.Abstract
{
    public class AbstractScheduleActivity
    {
        public AbstractPoiActivity PoiActivity { set; get; }
        public string Name { set; get; }
        public int Rate { set; get; }

        public TimeSpanInterval TimeStart { set; get; }
        public TimeSpanInterval TimeEnd { set; get; }
        public MinuteInterval TimeDuration { set; get; }
        public string Zone { set; get; }
        public int Level { set; get; }
        public IPointOfInterest ExactPoi { get; set; }
    }
}