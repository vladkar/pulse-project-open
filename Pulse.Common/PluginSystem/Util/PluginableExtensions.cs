using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.PluginSystem.Spawn;

namespace Pulse.Common.PluginSystem.Util
{
    public static class PluginableExtensions
    {
        public static void RegisterPugin(this IPluginable pluginable, PluginBase pluginBase)
        {
            if (pluginable.PluginsContainer == null)
                pluginable.PluginsContainer = new PluginsContainer();

            pluginBase.PluggedInObject = pluginable;
            pluginable.PluginsContainer.Plugins[pluginBase.Name] = pluginBase;
        }


        public static void RegisterPugins(this IPluginable pluginable, PluginBase[] pluginBase)
        {
            if (pluginable.PluginsContainer == null)
                pluginable.PluginsContainer = new PluginsContainer();

            foreach (var plugin in pluginBase)
            {
                RegisterPugin(pluginable, plugin);
            }
        }
    }
}