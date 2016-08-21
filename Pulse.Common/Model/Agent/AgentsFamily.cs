using System.Collections.Generic;

namespace Pulse.Common.Model.Agent
{
    public class AgentsFamily
    {
        public AgentsFamily()
        {
            Members = new List<AbstractPulseAgent>();
        }

        public ICollection<AbstractPulseAgent> Members { set; get; } 
    }
}
