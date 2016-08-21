using System;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.AgentScheduling.Planned
{
    public class PlannedActivity : IPlannedPoiActivity, IPlannedGeoTimeActivity
    {
        public AbstractScheduleActivity Activity { get; set; }
        public IPointOfInterest Poi { get; set; }
        
        public DateTime PlannedStartTime { get; set; }
        public DateTime PlannedEndTime { get; set; }
    }
}
