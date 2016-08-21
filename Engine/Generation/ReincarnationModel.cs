using System.Collections.Generic;
using System.Linq;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Containers;

namespace Pulse.MultiagentEngine.Generation
{
    public class ReincarnationModel : IAgentGenerationModel
    {
        private AgentRegistry Ar { get; }
        private List<AgentBase> DeadAgents = new List<AgentBase>();

        public ReincarnationModel(AgentRegistry ar)
        {
            Ar = ar;
        }

        public void AddDeadAgents(IEnumerable<AgentBase> ags)
        {
            DeadAgents.AddRange(ags);
        }

        public AgentBase[] GenerateAgentsInTime(double simTime)
        {
            AgentBase[] ret;
            if (DeadAgents.Count > 0)
            {
                var ls = DeadAgents.ToList();
                // reincarnation
                foreach (var deadAgent in DeadAgents)
                {
                    deadAgent.IsAlive = true;
                    deadAgent.TerminationReason = TerminationReason.None;

                    if(Ar.Contains(deadAgent.Id))
                    {
                        // distributed case when turned agent already fetched from another process
                        // ignore this agent
                        ls.Remove(deadAgent);
                    }
                }
                ret = ls.ToArray();
                DeadAgents.Clear();
            }
            else
            {
                ret = new AgentBase[0];
            }
            return ret;
        }

        public bool HaveMoreAgents(double currentSimTime)
        {
            return true;
        }
    }
}
