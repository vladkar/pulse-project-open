using Pulse.Common.Model.Agent;

namespace Pulse.Common.PluginFramework
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