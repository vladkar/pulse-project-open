using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Pseudo3D
{
    public interface IExternalPortal : IPortal, IComplexUpdatable
    {
        AbstractExternalPortalAgentGenerator AgentGenerator { set; get; }
    }
}