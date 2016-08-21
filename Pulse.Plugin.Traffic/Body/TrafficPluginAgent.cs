
using Pulse.Common.Model.Agent;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;

namespace Pulse.Plugin.Traffic.Body
{
    public class TrafficPluginAgent : PluginBaseAgent, IUpdatablePlugin
    {
        public bool IsInTransport { private set; get; }

        private TrafficPluginMap _mapPlugin;

        public TrafficPluginAgent(TrafficPluginMap mapPlugin)
        {
            Name = GlobalStrings.PluginName;
            _mapPlugin = mapPlugin;
        }

        public Pulse.MultiagentEngine.Map.PulseVector2 GetCarLocation()
        {
            return _mapPlugin.GetCarLocation(Agent.Point);
        }

        public override void Initialize(AbstractPulseAgent agent)
        {
            base.Initialize(agent);
        }
        
        public void Update(double timeStep, double time = -1)
        {
        }
    }
}