using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment;
using Pulse.Plugin.SimpleInfection.Infection;
using KDTree;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;

namespace Pulse.Plugin.SimpleInfection.Body
{

    public class SimpleInfectionPluginMap : PluginBaseMap, IUpdatablePluginMap, ILegendable
    {
        public KDTree<SimpleInfectionPluginAgent> AgentsKdTree2 { set; get; }
        public IList<SimpleInfectionPluginAgent> NullPatients { set; get; }

        private InfectionStateManager _ifsf;
        private Infector _infector;


        //infection network
        public ContactNetwork Contacts { set; get; }

        public int f = 0;
        public SimpleInfectionPluginMap(SimpleInfectionScenarioConfig config, InfectionStateManager infectionStateManager)
        {
            Name = GlobalStrings.PluginName;
            _ifsf = infectionStateManager;
            _infector = new Infector(this, _ifsf, config.InfectionInitialization.Value);

            //network
            Contacts = new ContactNetwork();
        }

        public void Update(double timeStep, double time)
        {
            InfectInAction();

            //network
//            UpdateNetwork(Contacts, time);

            
            if (!_ifsf.GetInfectionInfo().InfectionTransmissionTypes.ContainsKey("droplet")) return;
            AgentsKdTree2 = new KDTree<SimpleInfectionPluginAgent>(2);
            var agentInfectionPlgn= new SimpleInfectionPluginAgent();
            foreach (var agent in Map.AgentRegistry.OfType<AbstractPulseAgent>().Where(a => a.IsInsideBuilding == false))  
            {
                agentInfectionPlgn =
                    (SimpleInfectionPluginAgent) agent.PluginsContainer.Plugins[GlobalStrings.PluginName];
                AgentsKdTree2.AddPoint(new double[] { agent.Point.X, agent.Point.Y }, agentInfectionPlgn);
                //if (agentInfectionPlgn.InfectionStage.InfectionState.ToString().Equals("I2"))
                //{
                //    Console.WriteLine("Агент заражен I2: " + agent.Id);
                //}
            }
        
            //дописывать для мониторинга в консоль...100 заразившихся агентов
           
        }

//        private void UpdateNetwork(ContactNetwork contacts, double time)
//        {
//            var worlsSecs = Map.World.GetGeoWorldInfo().ToSecondsMultiplier * time;
//            var infectionPhaseUpdatePeriod = _ifsf.GetInfectionInfo().MatrixPhaseUpdatePeriodMinutes;
//            var infectionPeriosSecs = infectionPhaseUpdatePeriod * 60;
//            if (Math.Abs(worlsSecs%infectionPeriosSecs) < 0.1)
//            {
//                var targets =
//                    Contacts.Graph.Edges.Values.Where(e => e.EdgeData.ConnectionType == AgentConnectionTypes.Contact);
//                targets.ToList().ForEach(t => (t.NodeTo.NodeData.Agent.PluginsContainer.Plugins["SimpleInfection"] as SimpleInfectionPluginAgent).);
//            }
//                
//        }

        private int count = 0;
        private void InfectInAction()
        {
            if (count == 0)
            {
                _infector.InitialInfect();
                count ++;
            }
        }

        //TODO move it to agent
        public void UpdateAgent(AbstractPulseAgent agent, double timeStep, double time = -1)
        {
            if (!_ifsf.GetInfectionInfo().InfectionTransmissionTypes.ContainsKey("droplet")) return;

            var agentInfectionPlgn =
                 (SimpleInfectionPluginAgent) agent.PluginsContainer.Plugins[GlobalStrings.PluginName];

            if (agentInfectionPlgn.InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.I1)
            {
                var dropletContact = (DropletInfectionTransmission)
                    _ifsf.GetInfectionInfo().InfectionTransmissionTypes["droplet"];
                var infectionRadius = agentInfectionPlgn.InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.I2
                    ? dropletContact.NoSymptomsRadius
                    : dropletContact.SymptomsRadius;

                var infectionRiskAgents = new List<SimpleInfectionPluginAgent>();
                var iterator = AgentsKdTree2.NearestNeighbors(new double[] { agent.Point.X, agent.Point.Y }, 10, infectionRadius);
                var infectionReadyStage = BaseInfectionStage.InfectionStates.S;
                while (iterator.MoveNext())
                {
                    if (iterator.Current.InfectionStage.InfectionState == infectionReadyStage)
                        infectionRiskAgents.Add(iterator.Current);
                }

                var contactSeconds = (int) (timeStep* Map.World.GeoTime.GeoTime.Second);
                var contactMilliSeconds = (int)(timeStep * Map.World.GeoTime.GeoTime.Millisecond);
                var contactDuration = new TimeSpan(0,0, 0, contactSeconds, contactMilliSeconds);
                CalculateAgenstInfection(agentInfectionPlgn, infectionRiskAgents, contactDuration, "droplet");
            }
        }

        private void CalculateAgenstInfection(SimpleInfectionPluginAgent infector, IEnumerable<SimpleInfectionPluginAgent> agentsToInfect, TimeSpan contactDuration, string contactType)
        {
            foreach (var agentToInfect in agentsToInfect)
            {
                CalculateAgentInfection(infector, agentToInfect, contactDuration, contactType);
            }
        }

        private void CalculateAgentInfection(SimpleInfectionPluginAgent infector, SimpleInfectionPluginAgent agentToInfect, TimeSpan contactDuration, string contactType)
        {
            var contact = new InfectionContact
            {
                Infector = infector,
                Infected = agentToInfect,
                InfectionTime = Map.World.GeoTime.GeoTime,
                ContactDuration = contactDuration,
                ContactType = contactType,
                MapPoint = infector.Agent.Point,
                Level = agentToInfect.Agent.Level
            };
            //if (agentToInfect.ActualContacts.ContainsKey(infector.Agent.Id) && !agentToInfect.ActualContacts[infector.Agent.Id].Infected.Agent.Id.Equals(contact.Infected.Agent.Id))
            //    agentToInfect.ActualContacts.Add(infector.Agent.Id, contact);
             if (!agentToInfect.ActualContacts.ContainsKey(infector.Agent.Id))
                agentToInfect.ActualContacts.Add(infector.Agent.Id, contact);
                else 
                    agentToInfect.ActualContacts[infector.Agent.Id].ContactDuration += contactDuration;
            // }
        }

        public override void Initialize(IPulseMap map)
        {
            base.Initialize(map);
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return _ifsf.GetLegend();
        }
    }
}
