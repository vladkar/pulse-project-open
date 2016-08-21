using System;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.AgentScheduling.Current
{
    public class SimpleActionActivity : CurrentActivity
    {
        public DateTime PlannedEndTime { get; set; }
        public PlannedActivity PlannedActivity { get; set; }
        public IPointOfInterest Building { get; set; }
        public PulseVector2 ActionPoint { get; set; }

        public SimpleActionActivity()
        {
            State = ActivityState.NotStarted;
        }
    }
}