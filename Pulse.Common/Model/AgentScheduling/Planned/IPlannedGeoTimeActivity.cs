using System;

namespace Pulse.Common.Model.AgentScheduling.Planned
{
    public interface IPlannedGeoTimeActivity : IAgentActivity
    {
        DateTime PlannedStartTime { get; set; }
        DateTime PlannedEndTime { get; set; }
    }
}