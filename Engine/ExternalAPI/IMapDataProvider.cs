namespace Pulse.MultiagentEngine.ExternalAPI
{
    public interface IMapDataProvider
    {
        IMapData GetMapData();
    }

    public interface ITimestampedMapDataProvider
    {
        TimestampedData<IMapData> GetMapData();
    }

    public interface IMapInfoProvider
    {
        IMapInfo GetMapInfo();
    }

    public interface ITimestampedMapInfoProvider
    {
        TimestampedData<IMapInfo> GetMapInfo();
    }
}