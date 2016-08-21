using Pulse.MultiagentEngine.Generation;

namespace Pulse.Common.Model.Agent
{
    public interface IPulseAgentGenerationModel : IAgentGenerationModel
    {
        AbstractPulseAgent CreateNewAgent();
    }
}