using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Scenery.Loaders;
using Pulse.MultiagentEngine.Containers;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{
    public enum MovementSystemTypes { PEDESTRIAN, VEHICLE, SUB_MODEL }

    public abstract class MovementSystem : AbstractDataBroker
    {
        public IDictionary<long, AbstractPulseAgent> AgentsRegister { get; }

        protected PulseScenery _sb;
        protected PulseScenarioConfig _config;

        protected MovementSystem(PulseScenarioConfig config, PulseScenery sb)
        {
            _config = config;
            _sb = sb;
            AgentsRegister = new ConcurrentDictionary<long, AbstractPulseAgent>();
        }

        public abstract void Update(double timeStep, double time);

        public void OnAgentsAdded(object sender, AgentRegistry.AgentAdditionEventArgs e)
        {
            foreach (var agent in e.NewAgents.OfType<AbstractPulseAgent>())
            {
                AddAgent(agent, null);
            }
        }

        public void OnAgentRemoved(object sender, AgentRegistry.AgentRemoveEventArgs e)
        {
            if (e.RemovedAgent is AbstractPulseAgent)
                RemoveAgent(e.RemovedAgent as AbstractPulseAgent);
        }

        //TODO refactor all inherit objects?
        public virtual void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister[agent.Id] = agent;
            }
        }

        public virtual void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister.Remove(agent.Id);
            }
        }

//        public abstract void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit);
//        public abstract void RemoveAgent(AbstractPulseAgent agent);

        public abstract bool QueryVisibility(PulseVector2 point, PulseVector2 current, int level, float i);

        public virtual void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            throw new NotImplementedException();
        }

        public virtual double GetAgentMaxSpeed(AbstractPulseAgent agent)
        {
            throw new NotImplementedException();
        }

        public abstract void SetCommand(ICommand command);
    }

    public interface ISixFreedomDegreeMovementSystem
    {
        void MovePlatform(double dx, double dy, double dz, double pitch, double roll, double yaw);
    }

    public interface ISFMovementSystem
    {
        void SetAgentForce(AbstractPulseAgent agent, double force);
        void AddObstacle(AbstractPulseAgent agent, IList<PulseVector2> obstacle);
    }
}
