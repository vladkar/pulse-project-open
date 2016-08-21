using System.Collections.Generic;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Containers;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Map;

namespace Pulse.MultiagentEngine.Stats
{
    public interface IStatistics
    {
        void Finalization();
    }

    /// <summary>
    /// Update is called once after finish
    /// </summary>
    public interface IFinalStatistics : IStatistics
    {
        void Update(SimulationEngine engine);
    }

    public interface IAgentsStatistics : IStatistics
    {
        void Update(IEnumerable<AgentBase> agents, double simTime);
    }

    public interface IMapStatistics : IStatistics
    {
        void Update(IMap map, double simTime);
    }

    public interface IWorldStatistics : IStatistics
    {
        void Update(SimWorld world);
    }

    public interface IEngineStatistics : IStatistics
    {
        void Update(SimulationEngine  engine);
    }

}