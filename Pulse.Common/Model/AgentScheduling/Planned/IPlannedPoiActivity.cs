using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.AgentScheduling.Planned
{
    public interface IPlannedPoiActivity : IAgentActivity
    {
        AbstractScheduleActivity Activity { get; set; }
        IPointOfInterest Poi { get; set; }
    }
}