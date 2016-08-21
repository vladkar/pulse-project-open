using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.PluginSystem.Base;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Graph;
using Pulse.Plugin.SimpleInfection.Infection;
using Pulse.Plugin.SimpleInfection.Roles;
using System.Text;
using System.IO;


namespace Pulse.Plugin.SimpleInfection.Body
{
    public class SimpleInfectionPluginAgent : PluginBaseAgent, IUpdatablePlugin
    {
        private SimpleInfectionPluginMap _mapPlugin;
        private InfectionStateManager _ifsf;
        
        public IDictionary<long, InfectionContact> ActualContacts { set; get; }
        public IList<InfectionContact> Contacts { set; get; }
        public IDictionary<long, InfectionContact> ActualContactsHelp { set; get; }
        //public bool Immunity { set; get; }
        public BaseInfectionStage InfectionStage { set; get; }
        public int init = 0;
        public StringBuilder csv = new StringBuilder();
        //  public string newFileName = @"C:\Users\magistr\Documents\nir\4117 ain\2\2\1\1\Pulse.Model\InputData\StudentHospital\infection\inf.csv";
        public string fullPath = "";
        public SimpleInfectionPluginAgent()
        { }
        public SimpleInfectionPluginAgent(SimpleInfectionPluginMap mapPlugin, InfectionStateManager infectionStateManager)
        {
            Name = GlobalStrings.PluginName;
            _mapPlugin = mapPlugin;
            _ifsf = infectionStateManager;

            ActualContacts = new ConcurrentDictionary<long, InfectionContact>();
            ActualContactsHelp = new ConcurrentDictionary<long, InfectionContact>();
            Contacts = new List<InfectionContact>();

//            if (init == 0)
//            {
//               string partPath = @"Pulse.Model";
//                string newFileName = @"Pulse.Model\InputData\StudentHospital\infection\inf.csv";
//                var curDir = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(partPath).FullName).FullName).FullName).FullName).FullName).FullName;
//                 fullPath = Path.Combine(curDir, newFileName);
//                var names = string.Format("InfectedRole, InfectorRole, InfectedId, InfectorId, InfectionTime, ContactDuration, Success");
//                 csv.Append(names);
//                File.WriteAllText(fullPath, names);
//                init++;
//        }
        }

        public override void Initialize(AbstractPulseAgent agent)
        {
            base.Initialize(agent);
            Agent.OnEnterInBuilding += OnEnterInBuildingInfectionAction;


//            //TODO no infected if 5 < 10
//            if (RandomUtil.RandomInt(0, 5) == 10)
//                InfectionStage = _ifsf.GetInfectedStage(Agent.WorldKnowledge.Clock);
//            else
                InfectionStage = _ifsf.GetDefaultState();
        }

        public void OnEnterInBuildingInfectionAction(IPointOfInterest poi)
        {
            if (!_ifsf.GetInfectionInfo().InfectionTransmissionTypes.ContainsKey("direct_contact")) return;

            if (poi != null && poi.PluginsContainer != null)
            {
                if (InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.I1)
                {
                    InfectPoi(poi);
                }
                if (InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.S)
                {
                    MakeDirectPoiContact(poi);
                    ComputeDirectInfectionTransmission(poi);
                }
            }
        }

        private void InfectPoi(IPointOfInterest poi)
        {
            var poiPlgn = (SimpleInfectionPluginBuilding)poi.PluginsContainer.Plugins[GlobalStrings.PluginName];
            poiPlgn.IsInfected = true;
            poiPlgn.LastInfectionContact = Agent.WorldKnowledge.Clock;
            poiPlgn.LastInfector = this;
        }

        private void MakeDirectPoiContact(IPointOfInterest poi)
        {
            var buildingPlgn =
                (SimpleInfectionPluginBuilding) poi.PluginsContainer.Plugins[GlobalStrings.PluginName];

            var directContact =
                (DirectContactInfectionTransmission)
                    _ifsf.GetInfectionInfo().InfectionTransmissionTypes["direct_contact"];
            var activeInfectionPeriod = directContact.ActivePeriodMinutes;

            if (buildingPlgn.IsInfected &&
                Agent.WorldKnowledge.Clock - buildingPlgn.LastInfectionContact <=
                new TimeSpan(0, activeInfectionPeriod, 0))
                AddDirectPoiContact(poi, buildingPlgn);
        }

        private void AddDirectPoiContact(IPointOfInterest poi, SimpleInfectionPluginBuilding buildingPlgn)
        {
            if (InfectionStage.InfectionState > BaseInfectionStage.InfectionStates.IM)
                //if (RandomUtil.ToBeOrNot(1, 2))

            {
                var contact = new InfectionContact
                {
                    Infected = this,
                    Infector = buildingPlgn.LastInfector,
                    InfectionTime = Agent.WorldKnowledge.Clock,
                    ContactType = "direct_contact",
                    MapPoint = poi.TravelPoint,
                    Level = Agent.Level
                };

                //add node (if not exists)
                _mapPlugin.Contacts.CheckAndGetAgentVertex(Agent);
                //add edge
                _mapPlugin.Contacts.AddContact(contact);
            }
        }

        private void ComputeDirectInfectionTransmission(IPointOfInterest poi)
        {
            var buildingPlgn =
                    (SimpleInfectionPluginBuilding)poi.PluginsContainer.Plugins[GlobalStrings.PluginName];

            var directContact =
                (DirectContactInfectionTransmission)
                    _ifsf.GetInfectionInfo().InfectionTransmissionTypes["direct_contact"];
            var activeInfectionPeriod = directContact.ActivePeriodMinutes;

            if (buildingPlgn.IsInfected &&
                Agent.WorldKnowledge.Clock - buildingPlgn.LastInfectionContact <=
                new TimeSpan(0, activeInfectionPeriod, 0))
//           if (buildingPlgn.IsInfected)
               if (InfectionStage.InfectionState > BaseInfectionStage.InfectionStates.IM)
                   //if (RandomUtil.ToBeOrNot(1, 2))
                   if (UpdateInfectionStage())
                       Contacts.Add(new InfectionContact
                       {
                           Infected = this,
                           Infector = buildingPlgn.LastInfector,
                           InfectionTime = Agent.WorldKnowledge.Clock,
                           ContactType = "direct_contact",
                           MapPoint = poi.TravelPoint,
                           Level = Agent.Level
                       });

        }

        public void Update(double timeStep, double time = -1)
        {
            var infectionPhaseUpdatePeriod = _ifsf.GetInfectionInfo().MatrixPhaseUpdatePeriodMinutes;
            
            switch (InfectionStage.InfectionState)
            {
                case BaseInfectionStage.InfectionStates.S:
                    if (!_ifsf.GetInfectionInfo().InfectionTransmissionTypes.ContainsKey("droplet")) break;
                    var dropletContact = (DropletInfectionTransmission)
                    _ifsf.GetInfectionInfo().InfectionTransmissionTypes["droplet"];
                    var poissonMeanInfectionTime = dropletContact.PoissonMeanMinutes;
                    var infectionUpdatePeriod = dropletContact.ContactUpdatePeriodMinutes;
                    ComputeDropletInfectionTransmission(infectionUpdatePeriod, poissonMeanInfectionTime, time);
                    break;
                default:
                    ComputeInfectionProgress(infectionPhaseUpdatePeriod, time);
                    break;
            }
        }



//        public void UpdateDroplet(AbstractPulseAgent agent, double timeStep, double time = -1)
//        {
//            //if (!_ifsf.GetInfectionInfo().InfectionTransmissionTypes.ContainsKey("droplet")) return;
//
//            var agentInfectionPlgn =
//                 (SimpleInfectionPluginAgent)agent.PluginsContainer.Plugins[GlobalStrings.PluginName];
//
//            if (agentInfectionPlgn.InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.I1)
//            {
//                var dropletTransmissionInfo = (DropletInfectionTransmission)_ifsf.GetInfectionInfo().InfectionTransmissionTypes["droplet"];
//                var infectionRadius = agentInfectionPlgn.InfectionStage.InfectionState >= BaseInfectionStage.InfectionStates.I2
//                    ? dropletTransmissionInfo.NoSymptomsRadius
//                    : dropletTransmissionInfo.SymptomsRadius;
//
//                var infectionRiskAgents = new List<SimpleInfectionPluginAgent>();
//                var iterator = AgentsKdTree2.NearestNeighbors(new double[] { agent.Point.X, agent.Point.Y }, 10, infectionRadius);
//                var infectionVulnerableStage = BaseInfectionStage.InfectionStates.S;
//
//                while (iterator.MoveNext())
//                {
//                    if (iterator.Current.InfectionStage.InfectionState == infectionVulnerableStage)
//                        infectionRiskAgents.Add(iterator.Current);
//                }
//
//                var contactSeconds = (int)(Map.World.GetGeoWorldInfo().ToSecondsMultiplier * timeStep);
//                var contactDuration = new TimeSpan(0, 0, contactSeconds);
//                CalculateAgenstInfection(agentInfectionPlgn, infectionRiskAgents, contactDuration, "droplet");
//            }
//        }



        private void ComputeInfectionProgress(int infectionPhaseUpdatePeriod, double time)
        {
            var worlsSecs = _mapPlugin.Map.World.GetGeoWorldInfo().ToSecondsMultiplier * time;
            var infectionPeriosSecs = infectionPhaseUpdatePeriod*60;

            if (Math.Abs(worlsSecs % infectionPeriosSecs) < 0.1)
                UpdateInfectionStage();
        }
        
        private void ComputeDropletInfectionTransmission(int infectionUpdatePeriod, int poissonMeanInfectionTime, double time)
        {
            if (ActualContacts.Count > 0)
            {
                var worldSecs = _mapPlugin.Map.World.GetGeoWorldInfo().ToSecondsMultiplier * time;
                var infectionPeriosSecs = infectionUpdatePeriod;
                if (Math.Abs(worldSecs % infectionPeriosSecs) < 0.1)// почему 0.1
                {
                    int i = 0;
                    var toDel = new LinkedList<InfectionContact>(); //расчет воздушно-капел. контактов
                    foreach (var infectionContact in ActualContacts.Values)
                    {
                        Contacts.Add(infectionContact);
                        ComputeDropletInfectionContact(poissonMeanInfectionTime, infectionContact);
                        
                        toDel.AddLast(infectionContact);
                        if (!ActualContactsHelp.ContainsKey(infectionContact.Infector.Agent.Id))
                        {
                            ++i;
                            ActualContactsHelp.Add(infectionContact.Infector.Agent.Id, infectionContact);
                            //WriteInfectionInfo(infectionContact);
                            
                        }
                        else if (ActualContactsHelp.ContainsKey(infectionContact.Infector.Agent.Id) && (!ActualContactsHelp[infectionContact.Infector.Agent.Id].Infected.Agent.Id.Equals(infectionContact.Infected.Agent.Id)))
                            {
                            ActualContactsHelp.Add(infectionContact.Infector.Agent.Id, infectionContact);
                            //WriteInfectionInfo(infectionContact);
                            ++i;
                        }
                        else if((ActualContactsHelp.ContainsKey(infectionContact.Infector.Agent.Id) && ActualContactsHelp[infectionContact.Infector.Agent.Id].Infected.Agent.Id.Equals(infectionContact.Infected.Agent.Id)))
                        {
                            int secHelp = ActualContactsHelp[infectionContact.Infector.Agent.Id].InfectionTime.Second;
                            int secInf = infectionContact.InfectionTime.Second;
                            int diff = secHelp - secInf;
                            if (Math.Abs(diff) > 2)
                            {
                                ActualContactsHelp.Remove(infectionContact.Infector.Agent.Id);
                                ActualContactsHelp.Add(infectionContact.Infector.Agent.Id, infectionContact);
                                //WriteInfectionInfo(infectionContact);
                            }
                        }
                    }

                    foreach (var contact in toDel)
                    {
                        ActualContacts.Remove(contact.Infector.Agent.Id);

                    }
                }
            }
        }

        public void WriteInfectionInfo(InfectionContact infectionContact)
        {
                    var linenameInfected = infectionContact.Infected.Agent.Role.GetType().ToString();
                    var linenameInfector = infectionContact.Infector.Agent.Role.GetType().ToString();
                    if (linenameInfected.Contains("Doctors"))
                     { 
                        linenameInfected = "Doctors";
                       }
                    if (linenameInfected.Contains("Nurses"))
                        linenameInfected = "Nurses";
                    if (linenameInfected.Contains("Sanit"))
                        linenameInfected = "Sanit";
            if (linenameInfector.Contains("Doctors"))
            {
                linenameInfector = "Doctors";
            }
            else if (linenameInfector.Contains("Nurses"))
                linenameInfector = "Nurses";
            else if (linenameInfector.Contains("Sanit"))
                linenameInfector = "Sanit";
            else linenameInfector = "Patient";
            if ((infectionContact.ContactDuration.Seconds != 0) || (infectionContact.ContactDuration.Minutes != 0) || (infectionContact.ContactDuration.Milliseconds != 0))
                    {
                        var newLine = string.Format("\n{0},{1},{2},{3},{4},{5},{6}", linenameInfected, linenameInfector, infectionContact.Infected.Agent.Id,  infectionContact.Infector.Agent.Id, infectionContact.InfectionTime, infectionContact.ContactDuration, infectionContact.Success);
                        csv.Append(newLine);
                        File.AppendAllText(fullPath, newLine);
                    }
        }


        private void ComputeDropletInfectionContact(int poissonMeanInfectionTime, InfectionContact infectionContact)///СМОТРЕТЬ ЗДЕСЬ
        {
            var duration = (int) infectionContact.ContactDuration.TotalSeconds;
            var poissonProb = alglib.poissondistr.poissondistribution(duration, poissonMeanInfectionTime);

            if (!RandomUtil.ResolveProbability(poissonProb)) return;
            if (InfectionStage.InfectionState == BaseInfectionStage.InfectionStates.IM) return;
            if (UpdateInfectionStage())
            {
                infectionContact.Success = true;
                Contacts.Add(infectionContact);


            }
        }

        private bool UpdateInfectionStage()
        {
            if (InfectionStage.InfectionState == BaseInfectionStage.InfectionStates.IM)
                return false;

            var initialState = InfectionStage.InfectionState;
            InfectionStage = _ifsf.UpdateStage(InfectionStage, Agent.WorldKnowledge.Clock);

            if (initialState == BaseInfectionStage.InfectionStates.I2 && InfectionStage.InfectionState == BaseInfectionStage.InfectionStates.T) { 
                Agent.Role = new HomePatient(Agent);
                Agent.CurrentActivity = null;
            }

            if (initialState == BaseInfectionStage.InfectionStates.T && InfectionStage.InfectionState == BaseInfectionStage.InfectionStates.R)
            {
                Agent.Role = new HospitalPatient(Agent);
                Agent.CurrentActivity = null;
            }

            return InfectionStage.InfectionState != initialState;
        }
    }
}