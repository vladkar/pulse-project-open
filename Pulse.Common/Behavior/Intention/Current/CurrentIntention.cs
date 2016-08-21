using Pulse.Common.Behavior.Intention.Planned;

namespace Pulse.Common.Behavior.Intention.Current
{
    public abstract class CurrentIntention : ICurrentIntention
    {
        public IPlannedIntention PlannedIntention { set; get; }

        protected CurrentIntention(IPlannedIntention plannedIntention)
        {
            PlannedIntention = plannedIntention;
        }
    }
}