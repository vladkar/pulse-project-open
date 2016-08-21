using System.Collections.Generic;

namespace Pulse.Common.PluginFramework
{
    public class PluginsContainer
    {
        public IDictionary<string, PluginBase> Plugins { set; get; }

        public PluginsContainer()
        {
            Plugins = new Dictionary<string, PluginBase>();
        }
    }
}