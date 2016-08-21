using Pulse.Common.Model.Agent;

namespace Pulse.Common.PluginSystem.Interface
{
    public interface IUpdatablePluginMap : IUpdatablePlugin
    {
        void UpdateAgent(AbstractPulseAgent agent, double timeStep, double time = -1);
    }
}