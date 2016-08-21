using Pulse.Common.Model.Agent;

namespace Pulse.Common.PluginFramework
{
    public interface IUpdatablePluginMap : IUpdatablePlugin
    {
        void UpdateAgent(AbstractPulseAgent agent, double timeStep, double time = -1);
    }
}