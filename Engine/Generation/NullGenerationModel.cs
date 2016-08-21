using Pulse.MultiagentEngine.Agents;

namespace Pulse.MultiagentEngine.Generation
{
    public class NullGenerationModel : IAgentGenerationModel
    {
        public AgentBase[] GenerateAgentsInTime(double simTime)
        {
            return new AgentBase[0];
        }

        public bool HaveMoreAgents(double currentSimTime)
        {
            return false;
        }
    }
}
