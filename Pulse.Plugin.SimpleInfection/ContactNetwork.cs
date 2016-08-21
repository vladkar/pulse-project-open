using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils.Graph;
using Pulse.Plugin.SimpleInfection.Body;
using Pulse.Plugin.SimpleInfection.Infection;

namespace Pulse.Plugin.SimpleInfection
{
    public class ContactNetwork
    {
        public Graph<AgentVertexData, AgentEdgeData> Graph { set; get; }

        public ContactNetwork()
        {
            Graph = new Graph<AgentVertexData, AgentEdgeData>();
            Graph.AddVertex(new Vertex<AgentVertexData, AgentEdgeData>
            {
                Id = 0,
                NodeData = new AgentVertexData()
            });
        }

        public Vertex<AgentVertexData, AgentEdgeData> CheckAndGetAgentVertex(AbstractPulseAgent agent)
        {
            if (agent == null) return Graph.Vertices[0];
            if (Graph.Vertices.ContainsKey(agent.Id)) return Graph.Vertices[agent.Id];

            var vertex = new Vertex<AgentVertexData, AgentEdgeData>
            {
                Id = Convert.ToInt64(agent.Id),
                NodeData = new AgentVertexData {Agent = agent}
            };
            Graph.AddVertex(vertex);

            return vertex;
        }

        public void AddContact(InfectionContact contact)
        {
            Graph.AddEdge(new Edge<AgentVertexData, AgentEdgeData>
            {
                NodeFrom = CheckAndGetAgentVertex(contact.Infector == null ? null : contact.Infected.Agent),
                NodeTo = CheckAndGetAgentVertex(contact.Infected.Agent),
                EdgeData = new AgentEdgeData
                {
                    ConnectionType = AgentConnectionTypes.Contact
                }
            });
        }
    }


    public class AgentVertexData
    {
        public AbstractPulseAgent Agent { set; get; }
    }

    public class AgentEdgeData
    {
        public AgentConnectionTypes ConnectionType { set; get; }
    }

    public enum AgentConnectionTypes { Family, Friend, Contact }
}
