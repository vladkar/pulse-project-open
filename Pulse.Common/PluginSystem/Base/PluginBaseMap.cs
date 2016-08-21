using Pulse.Common.Model.Environment.Map;

namespace Pulse.Common.PluginSystem.Base
{
    public abstract class PluginBaseMap : PluginBase
    {
        public IPulseMap Map { set; get; }

        public virtual void Initialize(IPulseMap map)
        {
            Map = map;
        }
    }
}