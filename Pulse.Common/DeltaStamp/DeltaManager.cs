using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.DeltaStamp
{
    public class AgentPersistData : IPulseAgentDeltaData
    {
        public long Id { get; set; }
        public PulseVector2 Point { get; set; }
        public int Level { get; set; }
    }

    //TODO try deltamanager for one delta[] with typecasting
    public class DeltaManager
    {
        public IDictionary<long, AgentPersistData> Agents;
        public LinkedList<IPulseDelta> History { set; get; }

        public DeltaManager()
        {
            Agents = new ConcurrentDictionary<long, AgentPersistData>();
            History = new LinkedList<IPulseDelta>();
        }

        public void Update(IPulseDelta delta)
        {
            var agents = delta.Agents.ToArray();
            for (int i = 0; i < agents.Length; i++)
            {
                var ag = agents[i];
                if (!Agents.ContainsKey(ag.Id))
                    Agents[ag.Id] = new AgentPersistData {Id = ag.Id, Point = ag.Point, Level = ag.Level};
                else
                    agents[i].ApplyDelta(ag);
            }
        }
    }
}
