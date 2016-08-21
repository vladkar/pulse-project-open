using NLog;
using Pulse.MultiagentEngine.Containers;
using Pulse.MultiagentEngine.ExternalAPI;

namespace Pulse.MultiagentEngine.Map
{
    public abstract class MapBase : IMap
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        //TODO: map should know only about spatial agents and move them
        protected AgentRegistry Agents { get; private set; }

        protected MapBase(AgentRegistry agents)
        {
            Agents = agents;
        }

        public abstract void Update(double timeStep, double time);
        public abstract void SetCommand(object command);
        public abstract IMapData GetMapData();
        public abstract IMapInfo GetMapInfo();
        public abstract IAgentsData GetAgentsData();
        public abstract void Load();

        public abstract IAgentsData GetDeadAgentsData();
    }
}
