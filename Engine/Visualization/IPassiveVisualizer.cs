using Pulse.MultiagentEngine.Stats;

namespace Pulse.MultiagentEngine.Visualization
{
    public interface IPassiveVisualizer : IEngineStatistics
    {
        void Initialize();
        void Dispose();
    }
}