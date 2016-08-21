namespace Pulse.Common.PluginFramework
{
    public abstract class PluginBase
    {
        public string Name { set; get; }
        public IPluginable PluggedInObject { set; get; }
    }
}
