using Pulse.Common.PluginSystem.Base;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Agents
{
    public interface IPulseAgentFactory
    {
        PulseAgent CreateAgent(long id, PluginBaseAgent[] plugins, PulseVector2 coord = default(PulseVector2));
        PulseAgent CreateAgent(long id, MovementSystemFactory msf, PluginBaseAgent[] plugins, PulseVector2 coord = default(PulseVector2));
    }
}