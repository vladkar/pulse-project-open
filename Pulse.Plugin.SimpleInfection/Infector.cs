using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Containers;
using Pulse.Plugin.SimpleInfection.Body;
using Pulse.Plugin.SimpleInfection.Infection;

namespace Pulse.Plugin.SimpleInfection
{
    public class Infector
    {
        public string InfectionInitialization { get; set; }

        private SimpleInfectionPluginMap _map;
        private InfectionStateManager _ifsf;

        public Infector(SimpleInfectionPluginMap map, InfectionStateManager ifsf, string infectionInitialization)
        {
            _map = map;
            _ifsf = ifsf;
            InfectionInitialization = infectionInitialization;
        }
        

        public void InfectPulkovoTiolets()
        {
            var toilets =
                _map.Map.Levels.SelectMany(
                    l => l.Value.PointsOfInterest.Where(p => p.Types.Any(t => t.Name == "toilet")));

            foreach (var toilet in toilets)
            {
                var plgn = (SimpleInfectionPluginBuilding)toilet.PluginsContainer.Plugins["SimpleInfection"];
                plgn.LastInfectionContact = _map.Map.World.GeoTime.GeoTime;
                plgn.IsInfected = true;
            }
        }

        public void InfectVasilIslandEbolaChild()
        {
            var children =
                _map.Map.AgentRegistry.OfType<AbstractPulseAgent>().Where(a => a.PhysicalCapabilityClass.Name == "6-16");

            _map.NullPatients = new List<SimpleInfectionPluginAgent>();
            var nullPatientsCount = 10;

            foreach (var child in children.Take(nullPatientsCount))
            {
                var plgn = (SimpleInfectionPluginAgent)child.PluginsContainer.Plugins["SimpleInfection"];
                plgn.InfectionStage = _ifsf.GetInfectedStage(child.WorldKnowledge.Clock);
                _map.NullPatients.Add(plgn);
            }
        }

        public void InfectLuchegorsk()
        {
            var children =
                _map.Map.AgentRegistry.OfType<AbstractPulseAgent>().Where(a => a.PhysicalCapabilityClass.Name == "17-29");

            _map.NullPatients = new List<SimpleInfectionPluginAgent>();
            var nullPatientsCount = 10;

            foreach (var child in children.Take(nullPatientsCount))
            {
                var plgn = (SimpleInfectionPluginAgent)child.PluginsContainer.Plugins["SimpleInfection"];
                plgn.InfectionStage = _ifsf.GetInfectedStage(child.WorldKnowledge.Clock);
                _map.NullPatients.Add(plgn);
            }
        }

        private static int _infected = 0;
        private void InfectTrain()
        {
            var ar = _map.Map.AgentRegistry;
            ar.AgentAdditionEvent += TrainPassangersInfection;
            _map.NullPatients = new List<SimpleInfectionPluginAgent>();
        }

        private void TrainPassangersInfection(object sender, AgentRegistry.AgentAdditionEventArgs agentAdditionEventArgs)
        {
            var targetGroup =
                agentAdditionEventArgs.NewAgents.OfType<AbstractPulseAgent>();

            if (!targetGroup.Any()) return;
            foreach (var agent in targetGroup)
            {
                if (RandomUtil.PlayProbability(0.1))
                    InfectPassanger(agent);
            }
        }


        private void HospitaslPassangersInfection(object sender, AgentRegistry.AgentAdditionEventArgs agentAdditionEventArgs)
        {
            var targetGroup =
                agentAdditionEventArgs.NewAgents.OfType<AbstractPulseAgent>();

            if (!targetGroup.Any())return;
            foreach (var agent in targetGroup)
            {
                if (agent.Role.Name== "StudentHospital Simple Agent Role")
                InfectPassanger(agent);
            }
        }

        private void InfectPassanger(AbstractPulseAgent agent)
        {
           // int nullPatientsCount = 11;

           // if (_infected > nullPatientsCount) return;
            var plgn = (SimpleInfectionPluginAgent) agent.PluginsContainer.Plugins["SimpleInfection"];
            plgn.InfectionStage = _ifsf.GetInfectedStage();
            plgn.InfectionStage.StartTime = agent.WorldKnowledge.Clock;
              _map.NullPatients.Add(plgn);
            _infected++;
        }

        private void InfectCinema()
        {
            throw new NotImplementedException();
        }

        public void InitialInfect()
        {
            switch (InfectionInitialization)
            {
                case "children":
                    InfectVasilIslandEbolaChild();
                    break;
                case "toilets":
                    InfectPulkovoTiolets();
                    break;
                case "train":
                    InfectTrain();
                    break;
                case "cinema":
                    InfectTrain();
                    break;
                case "luchegorsk":
                    InfectLuchegorsk();
                    break;
                case "student_hospital":
                    InfectStudentHospital();
                    break;
                case "null":
                    InfectNull();
                    break;
                default:
                    throw new Exception("Unknown infection spread init conditions");
            }
        }


        // TODO INFECT HOSPITAL
        private void InfectStudentHospital()
        {
            var ar = _map.Map.AgentRegistry;

            ar.AgentAdditionEvent += HospitaslPassangersInfection;
            _map.NullPatients = new List<SimpleInfectionPluginAgent>();

            //throw new NotImplementedException();
        }

        private void InfectNull()
        {
        }
    }
}
