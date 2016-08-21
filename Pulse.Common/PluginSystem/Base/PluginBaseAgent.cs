using Pulse.Common.Model.Agent;

namespace Pulse.Common.PluginSystem.Base
{
    public abstract class PluginBaseAgent : PluginBase
    {
        public AbstractPulseAgent Agent { set; get; }

        public virtual void Initialize(AbstractPulseAgent agent)
        {
            Agent = agent;
        }
    }
}