using System.Collections.Generic;
using Pulse.Plugin.SimpleInfection.Body;

namespace Pulse.Plugin.SimpleInfection
{
    public class ArrayNode
    {
        public ArrayNode()
        {
            //Agents = new HashSet<SimpleInfectionPluginAgent>();
        }

        public HashSet<SimpleInfectionPluginAgent> Agents { set; get; }
    }
}