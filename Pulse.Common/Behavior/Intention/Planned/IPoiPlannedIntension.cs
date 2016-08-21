namespace Pulse.Common.Behavior.Intention.Planned
{
    public interface IPoiPlannedIntension : IPlannedIntention
    {
        string[] PoiTypes { set; get; }
    }
}