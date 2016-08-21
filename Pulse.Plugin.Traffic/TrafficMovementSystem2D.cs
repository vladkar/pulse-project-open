using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using EmbeddableTraffic;
using MultiagentEngine.Map;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Utils;
using Pulse.Plugin.Traffic.Body;

namespace Pulse.Plugin.Traffic
{
    public class TrafficMovementSystem2D : MovementSystem
    {
        public IDictionary<string, VehicleActivity> VehicleAgenActivitytRegister { get; private set; }

        private TrafficPluginMap _mapPlugin;
        public TrafficPluginMap MapPlugin { set { _mapPlugin = value; } }

        public TrafficMovementSystem2D(PulseScenarioConfig config, PulseScenery sb)
            : base(config, sb)
        {
            VehicleAgenActivitytRegister = new Dictionary<string, VehicleActivity>();
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

                var vehActivity = unit as VehicleActivity;
                if (vehActivity != null)
                {
                    _mapPlugin.AddAgent(agent, vehActivity.VehicleOrigin, vehActivity.TotalDestination);
                }
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
