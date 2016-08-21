using Pulse.Common.Model.Environment;

namespace Pulse.Common.PluginFramework
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