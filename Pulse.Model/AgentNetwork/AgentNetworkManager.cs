using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Agent.SocialNetworks;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using QuickGraph;

namespace Pulse.Model.AgentNetwork
{
    public class AgentNetworkManager : AbstractDataBroker
    {
        private IEnumerable<IPointOfInterest> _pois;

        public SocialGraphQ Graph { set; get; }

        public AgentNetworkManager(IEnumerable<IPointOfInterest> pois)
        {
            _pois = pois;
        }

        protected override void LoadData()
        {
            //„исло человек на домохоз€йство
            var val = 2.8;

            var graph = new AdjacencyGraph<AbstractPulseAgent, Edge<AbstractPulseAgent>>(allowParallelEdges: false);
            

            foreach (var poi in _pois.OfType<IBuilding>().Where(p => p.PoepleAmount > 0))
            {
                // prepare families
                var familiesCount = poi.Housemates.Count == 0 ? 0 : poi.Housemates.Count < val ? 1 : Convert.ToInt32(poi.Housemates.Count/val);

                var families = new List<AbstractPulseAgent> [familiesCount];

                for (int i = 0; i < familiesCount; i++)
                {
                    families[i] = new List<AbstractPulseAgent>();
                }

                foreach (var agent in poi.Housemates)
                {
                    var family = families.RandomChoise();
                    family.Add(agent);
                    agent.Family = new AgentsFamily
                    {
                        Members = family
                    };
                }

                // load families to graph
                foreach (var family in families)
                {
                    graph.AddVertexRange(family);

                    foreach (var member1 in family)
                    {
                        foreach (var member2 in family)
                        {
                            if (member1 == member2) continue;

                            graph.AddEdge(new Edge<AbstractPulseAgent>(member1, member2));
                            graph.AddEdge(new Edge<AbstractPulseAgent>(member2, member1));
                        }
                    }
                }
            }

            Graph = new SocialGraphQ(graph);
        }
    }
}