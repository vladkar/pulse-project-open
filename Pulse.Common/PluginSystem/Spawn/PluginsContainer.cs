using System.Collections.Generic;
using Pulse.Common.PluginSystem.Base;

namespace Pulse.Common.PluginSystem.Spawn
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