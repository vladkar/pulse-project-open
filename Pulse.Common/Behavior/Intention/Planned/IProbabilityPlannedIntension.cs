namespace Pulse.Common.Behavior.Intention.Planned
{
    public interface IProbabilityPlannedIntension : IPlannedIntention
    {
        double Probability { set; get; }
    }
}