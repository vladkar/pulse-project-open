namespace Pulse.MultiagentEngine.ExternalAPI
{
    /// <summary>
    /// Domain-specific packed data about current state of the map.
    /// Shoul be optimized and ready to transfer through the network.
    /// </summary>
    public interface IMapData : IPackableData
    {

    }

    public interface IMapInfo : IPackableData
    {

    }
}