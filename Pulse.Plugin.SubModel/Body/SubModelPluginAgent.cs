using Pulse.Common.Model.Agent;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;

namespace Pulse.Plugin.SubModel.Body
{
    public class SubModelPluginAgent : PluginBaseAgent, IUpdatablePlugin
    {
        public bool IsInTransport { private set; get; }

        private SubModelPluginMap _mapPlugin;

        public SubModelPluginAgent(SubModelPluginMap mapPlugin)
        {
            Name = GlobalStrings.PluginName;
            _mapPlugin = mapPlugin;
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