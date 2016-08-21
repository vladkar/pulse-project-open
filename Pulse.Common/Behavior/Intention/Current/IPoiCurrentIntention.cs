using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Behavior.Intention.Current
{
    public interface IPoiCurrentIntention : ICurrentIntention
    {
        IPointOfInterest Poi { set; get; }
    }
}