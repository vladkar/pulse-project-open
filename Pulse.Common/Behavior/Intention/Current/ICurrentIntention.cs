using Pulse.Common.Behavior.Intention.Planned;

namespace Pulse.Common.Behavior.Intention.Current
{
    public interface ICurrentIntention
    {
        IPlannedIntention PlannedIntention { set; get; }
    }
}