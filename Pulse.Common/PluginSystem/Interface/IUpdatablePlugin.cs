namespace Pulse.Common.PluginSystem.Interface
{
    public interface IUpdatablePlugin
    {
        void Update(double timeStep, double time = -1);
    }
}