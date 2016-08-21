using Pulse.Common.PluginSystem.Interface;

namespace Pulse.Common.PluginSystem.Base
{
    public abstract class PluginBase
    {
        public string Name { set; get; }
        public IPluginable PluggedInObject { set; get; }
    }
}
