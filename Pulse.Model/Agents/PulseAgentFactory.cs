using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Util;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Agents
{
    public class PulseAgentFactory : IPulseAgentFactory
    {
        public PulseAgent CreateAgent(long id, PluginBaseAgent[] plugins, PulseVector2 coord = default(PulseVector2))
        {
            var agent = new PulseAgent(id, coord);
            PluginableExtensions.RegisterPugins(agent, plugins);
            return agent;
        }

        public PulseAgent CreateAgent(long id, MovementSystemFactory msf, PluginBaseAgent[] plugins, PulseVector2 coord = default(PulseVector2))
        {
            var agent = new PulseAgent(id, msf, coord);
            PluginableExtensions.RegisterPugins(agent, plugins);
            return agent;
        }
    }
}