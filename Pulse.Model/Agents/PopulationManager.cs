using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.NavField;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Model.AgentNetwork;
using Pulse.Model.MovementSystems;
using Pulse.Model.Navigation;
using Pulse.Scenery.StPetersburg.Net;
using Pulse.Social.Population.General;

namespace Pulse.Model.Agents
{
    public class PopulationManager : AbstractDataBroker, IPopulationManager, ILegendable
    {
        public IList<AbstractPulseAgent> Population { get; }
        public IDictionary<ISocialEconomyClass, int> ClassRaiting { get; private set; }
        public IDictionary<ISocialEconomyClass, IDictionary<IPhysicalCapabilityClass, int>> MappingTable { get; private set; }

        public ISet<ISocialEconomyClass> SocioClasses { get; private set; }
        public ISet<IPhysicalCapabilityClass> PhysicalClasses { get; private set; } 

        private readonly PulseScenery _sb;
        private readonly ClassMappingReader _cmr;
        private readonly GeoWorldGeneralInfo _gwi;
        private AgentNetworkManager _anm;
        private readonly IPulseAgentFactory _af;
        private readonly GeoCartesUtil _gcu;
        private long _currentId = 1;
        private PluginFactory _plgnf;
        private MovementSystemFactory _msf;
        private AgentNavigatorFactory _anf;


        public PopulationManager(PulseScenery sb, ClassMappingReader cmr, IPulseAgentFactory af, MovementSystemFactory msf, GeoWorldGeneralInfo worldInfo, GeoCartesUtil gcu, PluginFactory plgnf, AgentNavigatorFactory anf)
            : base()
        {
            _sb = sb;
            _cmr = cmr;
            _gwi = worldInfo;
            _af = af;
            _msf = msf;
            _gcu = gcu;
            Population = new List<AbstractPulseAgent>();
            _plgnf = plgnf;
            _anf = anf;
            Name = "Population manager";
        }
        
        protected override void LoadData()
        {
            if (!_sb.IsLoaded)
                _sb.Initialize();
            if (!_cmr.IsLoaded)
                _cmr.Initialize();

            ClassRaiting = _cmr.ClassRaiting;
            MappingTable = _cmr.MappingTable;

            SocioClasses = MappingTable.Keys.ToHashSet();
            PhysicalClasses = MappingTable.Values.First().Keys.ToHashSet();
        }

        public void InitPopulation(int count)
        {
            for (int i = 0; i < count; i++)
                Population.Add(CreateAgentAtHome());
        }

        public AbstractPulseAgent CreateAgentAtHome()
        {
            var agent = CreateAgent();

            var candHomeBuilds = new List<IPointOfInterest>();
            foreach (var t in agent.SocialEconomyClass.HomeTypes)
            {
                candHomeBuilds.AddRange(_sb.TypedPointsOfInterest[t]);
            }

            agent.Role = new CitizenRole(agent);
            //agent.Role.Initialize();
            agent.Role.AbstractClassSchedule = agent.SocialEconomyClass.AbstractClassSchedules.ProportionChoise(sh => sh.Raiting);
            agent.Home = candHomeBuilds.ProportionChoise(b => ((IBuilding)b).PoepleAmount);
            if (agent.Home is IBuilding)
                (agent.Home as IBuilding).Housemates.Add(agent);

            agent.Move(agent.Home.TravelPoint);
            agent.ChangeLevel(agent.Home.Level);
            agent.StartInteractWithPoi(agent.Home);
            return agent;
        }
        
        public AbstractPulseAgent CreateAgent()
        {
            var a = CreteEmptyAgent();
            a.SocialEconomyClass = ClassRaiting.ProportionChoise(pair => pair.Value).Key;
            a.PhysicalCapabilityClass = MappingTable[a.SocialEconomyClass].ProportionChoise(pair => pair.Value).Key;
            a.Sex = RandomUtil.PlayProbability(40) ? Sex.Male : Sex.Female;

            a.Role = new CitizenRole(a);
            //a.Role.Initialize();
            a.Role.AbstractClassSchedule = a.SocialEconomyClass.AbstractClassSchedules.ProportionChoise(sh => sh.Raiting);


            //TODO
            a.IsAlive = true;
            a.WorldKnowledge = new WorldKnowledge(_sb, _gwi, _gcu);
            //            a.Navigator = new RoadGraphNavigatorPseudo3D(_sb.Graph);
            //            a.Navigator = new PoiTreeNavfieldNavigator(_sb);
            a.Navigator = _anf.GetNavigator(a);

            a.Initialize();

            return a;
        }

        public virtual AbstractPulseAgent CreteEmptyAgent()
        {
            return _af.CreateAgent(_currentId++, _msf, _plgnf.GetPluginsForAgent());
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>>
            {
                {"PhysicalClasses", GetPhysicalClassesLegend()},
                {"SocioClasses", GetSocioClassesLegend()}
            };
        }

        private IList<LegendElement> GetSocioClassesLegend()
        {
            return SocioClasses.Select(cl => new LegendElement
            {
                NiceName = cl.Name,
                Id = cl.Id,
                Name = cl.Name
            }).ToList();
        }

        private IList<LegendElement> GetPhysicalClassesLegend()
        {
            return PhysicalClasses.Select(cl => new LegendElement
            {
                NiceName = cl.Name,
                Id = cl.Id,
                Name = cl.Name
            }).ToList();
        }
    }


    public class AgentNavigatorFactory
    {
        public Navigators Navigator { set; get; }
        private PulseScenery _sb;

        public AgentNavigatorFactory(PulseScenery sb)
        {
            _sb = sb;
            Navigator = sb.Navigator;
        }

        public IPseudo3DAgentNavigator GetNavigator(AbstractPulseAgent a)
        {
            switch (Navigator)
            {
                case Navigators.GRAPH:
                    return new RoadGraphNavigatorPseudo3D(_sb.Graph);
                case Navigators.NAVFIELD:
                    return new PoiTreeNavfieldNavigator(_sb);
            }

            throw new NotImplementedException();
        }
    }
}
