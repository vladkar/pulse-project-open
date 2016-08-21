using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Engine;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.Map;
using Pulse.Social.Population;

namespace Pulse.Plugin.SubModel.Body
{
    public class SubModelPluginMap : PluginBaseMap, IUpdatablePluginMap, ILegendable
    {
        private PulseEngine _engine;
        private SubModelScenarioConfig _config;
        private List<AbstractPulseAgent> _newAgets;
        private List<AbstractPulseAgent> _deadAgets;
        private PulseVector2 _offset;
        private SubModelMovementSystem2D _gms;
        private IDictionary<long, AbstractPulseAgent> _guestRegister;

        public SubModelPluginMap(SubModelScenarioConfig config)
        {
            Name = GlobalStrings.PluginName;
            _config = config;
        }

        public void Update(double timeStep, double time)
        {
            _newAgets = new List<AbstractPulseAgent>();
            _deadAgets = new List<AbstractPulseAgent>();

            _engine.DoStep();


            //TODO correct agent clone (social class, role, etc)
            foreach (var guestAgent in _newAgets)
            {
                var newAgent = (Map.World.Engine.GenerationModel as IPulseAgentGenerationModel).CreateNewAgent();
                newAgent.Role = new SubModelRole(newAgent);

                newAgent.Point = GetPoint(guestAgent);
                newAgent.Level = guestAgent.Level;
                newAgent.IsInsideBuilding = false;
                newAgent.CurrentActivity = null;
                newAgent.PhysicalCapabilityClass = guestAgent.PhysicalCapabilityClass;
                newAgent.PhysicalCapabilityClass = new PhysicalCapabilityClass {Id = 7891};
                

                var candHomeBuilds = new List<IPointOfInterest>();
                foreach (var t in newAgent.SocialEconomyClass.HomeTypes)
                {
                    candHomeBuilds.AddRange(Map.Scenery.TypedPointsOfInterest[t]);
                }
                newAgent.Home = candHomeBuilds.ProportionChoise(b => ((IBuilding) b).PoepleAmount);
                if (newAgent.Home is IBuilding)
                    (newAgent.Home as IBuilding).Housemates.Add(newAgent);

                newAgent.CurrentActivity = new GuestAgentActivity
                {
                    OriginWorldName = guestAgent.World.Name,
                    OriginAgentId = guestAgent.Id
                };

                (Map.World as PulseWorld).AddNewAgent(newAgent);
                _guestRegister[guestAgent.Id] = newAgent;
                newAgent.SetMovementSystem(newAgent.CurrentActivity as UnitTravelingActivity);
            }

            foreach (var guestAgent in _deadAgets)
            {
                var agent = _guestRegister[guestAgent.Id];
                agent.DoneActivity();
//                agent.Role = new SocialRole(agent);
                _guestRegister[guestAgent.Id].Kill("host_agent_dead");
                _guestRegister.Remove(guestAgent.Id);
            }

            if (_deadAgets.Any())
                Debug.Assert(true);
//            _engine.World.Agents

        }

        private PulseVector2 GetPoint(AbstractPulseAgent guestAgent)
        {
            return guestAgent.Point + _offset;
        }

        public void UpdateAgent(AbstractPulseAgent agent, double timeStep, double time = -1)
        {
        }

        public IDictionary<long, PulseVector2> GetAgetnsPositions()
        {
            return _engine.World.Agents.OfType<AbstractPulseAgent>().ToDictionary(a => _guestRegister[a.Id].Id, a => GetPoint(a));
        } 

        public override void Initialize(IPulseMap map)
        {
            base.Initialize(map);

            _gms = Map.GetMovementSystem(MovementSystemTypes.SUB_MODEL) as SubModelMovementSystem2D;
            _gms.MapPlugin = this;

            var f = AbstractPulseFactory.GetInstance();
            var subConfig = f.GetConfig(_config.SubScenario.Value);
            _engine = f.GetEngine(subConfig, SimulationRunMode.StepByStep) as PulseEngine;
            _engine.Start();

            _engine.World.Agents.AgentAdditionEvent += (sender, args) => _newAgets.AddRange(args.NewAgents.OfType<AbstractPulseAgent>().ToList());
            _engine.World.DeadAgents.AgentAdditionEvent += (sender, args) => _deadAgets.AddRange(args.NewAgents.OfType<AbstractPulseAgent>().ToList());

            _offset = Map.MapUtils.GetCoordsTuple(_config.MappingPointBL.Value);

            _guestRegister = new Dictionary<long, AbstractPulseAgent>();
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>>();
        }
    }
}
