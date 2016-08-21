namespace Pulse.Common.PluginFramework
{
    public interface IUpdatablePlugin
    {
        void Update(double timeStep, double time = -1);
    }
}