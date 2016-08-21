namespace Pulse.MultiagentEngine.ExternalAPI
{
    public interface IAgentsDataProvider
    {
        IAgentsData GetAgentsData();
    }

    public interface IDeadAgentsDataProvider
    {
        IAgentsData GetDeadAgentsData();
    }

    public interface ITimestampedAgentsDataProvider
    {
        TimestampedData<IAgentsData> GetAgentsData();
    }

    public interface ITimestampedDeadAgentsDataProvider
    {
        TimestampedData<IAgentsData> GetDeadAgentsData();
    }

    public class DeadAgent
    {
        public string DeathType { set; get; }
    }
}