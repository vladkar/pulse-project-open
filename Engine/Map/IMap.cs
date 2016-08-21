using Pulse.MultiagentEngine.ExternalAPI;

namespace Pulse.MultiagentEngine.Map
{
    //TODO: Think about IAgentsDataProvider, may be not the right place for it
    public interface IMap : IMapDataProvider, IAgentsDataProvider, IMapInfoProvider, IDeadAgentsDataProvider
    {
        #region Simulation
        void Update(double timeStep, double time);
        void SetCommand(object command);
        #endregion
    }
}