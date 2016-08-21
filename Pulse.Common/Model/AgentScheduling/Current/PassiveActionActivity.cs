using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.AgentScheduling.Current
{
    public class PassiveActionActivity : CurrentActivity
    {
        public PlannedActivity PlannedActivity { get; set; }
        public IPointOfInterest Building { get; set; }

        public PassiveActionActivity()
        {
            State = ActivityState.NotStarted;
        }
    }
}