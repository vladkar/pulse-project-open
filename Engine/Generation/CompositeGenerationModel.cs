using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Agents;

namespace Pulse.MultiagentEngine.Generation
{
    public class CompositeGenerationModel : IAgentGenerationModel
    {
        private readonly IEnumerable<IAgentGenerationModel> _models;

        public CompositeGenerationModel(IEnumerable<IAgentGenerationModel> models)
        {
            _models = models;
        }

        public AgentBase[] GenerateAgentsInTime(double simTime)
        {
            List<AgentBase> ags = new List<AgentBase>();
            foreach (var agentGenerationModel in _models)
            {
                ags.AddRange(agentGenerationModel.GenerateAgentsInTime(simTime));
            }
            return ags.ToArray();
        }

        public bool HaveMoreAgents(double currentSimTime)
        {
            bool ret = _models.Aggregate(false,
                                         (model, generationModel) =>
                                         model || generationModel.HaveMoreAgents(currentSimTime));
            return ret;
        }
    }

}
