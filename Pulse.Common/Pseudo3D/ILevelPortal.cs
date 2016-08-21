using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Pseudo3D
{
    public interface ILevelPortal : IPortal
    {
        LevelPortalTransporter PortalTransporter { set; get; }
    }
}