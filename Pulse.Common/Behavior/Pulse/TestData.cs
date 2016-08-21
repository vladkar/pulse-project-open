using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Behavior.Intention.Planned;

namespace Pulse.Common.Behavior.Pulse
{
    public class TestData
    {
        public IPlannedIntention PlannedIntention { set; get; }
        public ICurrentIntention CurrentIntention { set; get; }
    }
}