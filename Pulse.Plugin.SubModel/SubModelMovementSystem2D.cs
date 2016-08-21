using System;
using System.Collections.Generic;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Plugin.SubModel.Body;

namespace Pulse.Plugin.SubModel
{
    public class SubModelMovementSystem2D : MovementSystem
    {
        public IDictionary<string, AbstractPulseAgent> SubModelAgenActivitytRegister { get; private set; }

        private SubModelPluginMap _mapPlugin;
        public SubModelPluginMap MapPlugin { set { _mapPlugin = value; } }

        public SubModelMovementSystem2D(PulseScenarioConfig config, PulseScenery sb)
            : base(config, sb)
        {
            SubModelAgenActivitytRegister = new Dictionary<string, AbstractPulseAgent>();
        }
        
        protected override void LoadData()
        {
        }

        public override void Update(double timeStep, double time)
        {
            var positions = _mapPlugin.GetAgetnsPositions();

            foreach (var position in positions)
            {
                AgentsRegister[position.Key].Move(position.Value);
            }
        }

        public override void AddAgent(AbstractPulseAgent agent, UnitTravelingActivity unit)
        {
            if (!AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister[agent.Id] = agent;

//                var sma = unit as GuestAgentActivity;
//                if (sma != null)
//                {
//                    SubModelAgenActivitytRegister[sma.OriginAgentId] = agent;
//                }
            }
        }

        public override void RemoveAgent(AbstractPulseAgent agent)
        {
            if (AgentsRegister.ContainsKey(agent.Id))
            {
                AgentsRegister.Remove(agent.Id);
            }
        }

        public override bool QueryVisibility(Pulse.MultiagentEngine.Map.PulseVector2 point, Pulse.MultiagentEngine.Map.PulseVector2 current, int level, float i)
        {
            throw new NotImplementedException();
        }

        public override void SetAgentMaxSpeed(AbstractPulseAgent agent, double maxSpeed)
        {
            throw new NotImplementedException();
        }

        public override void SetCommand(ICommand command)
        {
            throw new NotImplementedException();
        }
    }
}
