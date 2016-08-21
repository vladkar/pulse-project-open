namespace Pulse.Common.Model.Environment.Poi
{
    public interface IPortal : IPointOfInterest, IInteractable
    {
        bool Enterable { set; get; }
        bool Exitable { set; get; }
    }
}