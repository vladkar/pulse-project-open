using Pulse.MultiagentEngine.Agents;

namespace Pulse.MultiagentEngine.Generation
{
    public interface IAgentGenerationModel
    {
        AgentBase[] GenerateAgentsInTime(double simTime);

        /// <summary>
        /// To determine stop condition
        /// </summary>
        /// <returns></returns>
        bool HaveMoreAgents(double currentSimTime);
    }
}
