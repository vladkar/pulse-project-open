using QuickGraph;

namespace Pulse.Common.Model.Agent.SocialNetworks
{
    public class SocialGraphQ
    {
        private AdjacencyGraph<AbstractPulseAgent, Edge<AbstractPulseAgent>> _graph;

        public SocialGraphQ() { }

        public SocialGraphQ(AdjacencyGraph<AbstractPulseAgent, Edge<AbstractPulseAgent>> graph) : this()
        {
            _graph = graph;
        }
    }
}