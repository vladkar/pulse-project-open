using System.Collections.Generic;
using NLog;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Map;

namespace Pulse.MultiagentEngine.Containers
{
    /// <summary>
    /// Passive container for simulation objects
    /// </summary>
    public class SimWorld
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public SimulationEngine Engine { get; set; }

        /// <summary>
        /// World name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Simulation time. Now.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Spatial map of the world
        /// </summary>
        public IMap Map { get; set; }


        //TODO: world should know about all agents and update their state in time
        private AgentRegistry _agents = new AgentRegistry();
        private AgentRegistry _agentsCemetry = new AgentRegistry();

        /// <summary>
        /// Agent collection
        /// </summary>
        public AgentRegistry Agents
        {
            get { return _agents; }
        }

        public AgentRegistry DeadAgents
        {
            get { return _agentsCemetry; }
        }
        
        public void AddNewAgent(AgentBase agent)
        {
            agent.World = this;
            agent.InitializeSimulation(Time);
            Agents.Add(agent);
        }

        public void AddNewAgents(IEnumerable<AgentBase> agents)
        {
            foreach (var agent in agents)
            {
                AddNewAgent(agent);
            }
        }

        public SimWorld(string name)
        {
            Name = name;

            Log.Info("World '{0}' has been created", Name);
        }
    }
}