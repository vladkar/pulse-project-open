using Pulse.Common.Model.Agent;

namespace Pulse.Common.PluginSystem.Base
{
    public abstract class PluginBaseAgentData : PluginBase
    {
        public IPulseAgentData2 AgentData { set; get; }

        public virtual void Initialize(IPulseAgentData2 agentData)
        {
            AgentData = agentData;
        }
    }
}