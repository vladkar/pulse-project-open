namespace Pulse.MultiagentEngine.Stats
{
    public interface IStatisticsRegistry
    {
        void Update();
        T GetStatistics<T>() where T : IStatistics;
        void Add(IStatistics agentsStatistics);
        void Remove(IStatistics agentsStatistics);
        void Finalization();
    }
}