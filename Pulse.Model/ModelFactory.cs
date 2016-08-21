using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Util;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Model.AgentNetwork;
using Pulse.Model.Agents;
using Pulse.Model.Config;
using Pulse.Model.Environment;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Engine;
using Pulse.Scenery.Cinema.Data;
using Pulse.Scenery.Cinema.Role;
using Pulse.Scenery.Corridor.Data;
using Pulse.Scenery.Corridor_T.Data;
using Pulse.Scenery.Krestovsky;
using Pulse.Scenery.Luchegorsk.Data;
using Pulse.Scenery.MovingPlatform;
using Pulse.Scenery.Pulkovo;
using Pulse.Scenery.Pulkovo.Data;
using Pulse.Scenery.SeaStationRough;
using Pulse.Scenery.Ship;
using Pulse.Scenery.StPetersburg.Net;
using Pulse.Scenery.StPetersburg.Net.Data;
using Pulse.Scenery.SubwayStation.Vasileostrovskaya;
using Pulse.Scenery.SubwayStation.Vasileostrovskaya.ValidationExperiment;
using Pulse.Scenery.Test28;
using Pulse.Scenery.Train;
using Pulse.Scenery.Train.Data;
using Pulse.Scenery.Train.Roles;
using Pulse.Scenery.Train.Route;
using Pulse.Scenery.Vasilyevsky;
using Pulse.Scenery.Vasilyevsky.Infrastructure.Aggregated;
using Pulse.Scenery.Vasilyevsky.Infrastructure.Osm;
using Pulse.Scenery.Vasilyevsky.Portals;
using Pulse.Social;
using Pulse.Social.Data;
using Pulse.Social.Population.General;

namespace Pulse.Model
{
    public class ModelFactory : AbstractPulseFactory
    {
        public event EventHandler StateChange;

        protected virtual void OnStateChange()
        {
            EventHandler handler = StateChange;
            if (handler != null) handler(this, EventArgs.Empty);
        }
        
        public PulseEngine GetEngine(PulseScenarioConfig config, IPulseAgentFactory agentFactory, SimulationRunMode simulationRunMode)
        {
            var pccr = new PhysicalCapabilityClassReader(config.DataDir.Value);
            pccr.Initialize();

            var la = new LegendAggregator();
            var plgnf = new PluginFactory(config, la);
            var gcu = new GeoCartesUtil(config.MapConfig.MinGeo, config.MapConfig.MetersPerMapUnit);
            //var name = config.SceneryBlock.Value;
            var name = "removed param";
            PulseScenery sb = null;
            AbstractExternalPortalBuilder prtlm = null;

            BuildScenery(config, name, gcu, plgnf, ref prtlm, ref sb);
            sb.Initialize();

            la.Legendables.Add(sb);
            
            var secr = new SocialEconomyClassReader(config.DataDir.Value, sb);
            secr.Initialize();
            var cmr = new ClassMappingReader(config.DataDir.Value, pccr, secr);
            cmr.Initialize();
            var mbf = new MovementSystemFactory(config, sb);
            mbf.Initialize();

            var af = agentFactory;
            var pm = new PopulationManager(sb, cmr, af, mbf, config.GeoWorldInfo, gcu, plgnf, null);
            pm.Initialize();
            pm.InitPopulation(config.InitialPopulation.Value);

            var anm = new AgentNetworkManager(sb.Levels.SelectMany(l => l.Value.PointsOfInterest).ToList());
            anm.Initialize();
            
            var world = new PulseWorld(config);

            var map = new PulseMap(config.SimulationProperties, config.MapConfig, gcu, sb, anm, plgnf, world, mbf);
            map.RegisterPugins(plgnf.GetPluginsForMap());
            map.Load();
            world.Map = map;

            if (prtlm != null)
                prtlm.ApplyEnvironment(pm, map);

            la.Legendables.Add(GetRoleManager(name, pm));
            la.Legendables.Add(pm);


            var agen = new SimpleAgentsGenerator(pm, config.InitialPopulation.Value);
            var engine = new PulseEngine(world, config.SimulationProperties, agen, la,
                simulationRunMode);

            return engine;
        }

        private ILegendable GetRoleManager(string name, PopulationManager pm)
        {
            switch (name)
            {
                case "Vasilyevsky":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Pulkovo":
                    return new AirportRoleLegend();
                case "Train":
                    return new TrainRoleLegend();
                case "Cinema":
                    return new CinemaRoleLegend();
                case "Luchegorsk":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Ship":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "SeaStationRough":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "SubwayStation":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "SubwayStationValidation":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "NetCity":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Corridor":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Corridor_T":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Moving_Platform":
                    return new SocioRoleLegend(pm.SocioClasses);
                case "Krestovsky":
                    return new SocioRoleLegend(pm.SocioClasses);
                default:
                    throw new Exception();
            }
        }

        private void BuildScenery(PulseScenarioConfig config, string name, GeoCartesUtil gcu, PluginFactory plgnf,
            ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            switch (name)
            {
                case "Vasilyevsky":
                    BuildVasilyevskyScenery(config, gcu, plgnf, ref prtlm, ref sb);
                    break;

                case "Pulkovo":
                    BuildPulkovoScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Train":
                    BuildTrainScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Cinema":
                    BuildCinemaScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Luchegorsk":
                    BuildLuchegorskScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Ship":
                    BuildShipScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "SeaStationRough":
                    BuildSeaStationRoughScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "SubwayStation":
                    BuildSubwayStationScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "SubwayStationValidation":
                    BuildSubwayStationValidationScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "NetCity":
                    BuildNetCityScenery(config, gcu, plgnf, ref prtlm, ref sb);
                    break;

                case "Corridor":
                    BuildCorridorScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Corridor_T":
                    BuildCorridorTScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Moving_Platform":
                    BuildMovingPlatformScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                case "Krestovsky":
                    BuildKrestovskyScenery(config, plgnf, ref prtlm, ref sb);
                    break;

                default:
                    throw new Exception("Unknown or unsupported scenery:" + name);
            }
        }

        private void BuildKrestovskyScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            //            var sbuilder = new KrestovskySceneryBuilder(config, plgnf);
            //            sbuilder.Initialize();
            //
            //            sb = sbuilder.SceneryModule;
            //            prtlm = sbuilder.PortalBuilder;
        }

        private void BuildMovingPlatformScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var sssb = new MovingPlatformSceneryBuilder(config, plgnf);
            sssb.Initialize();

            sb = sssb.SceneryModule;
            prtlm = sssb.PortalBuilder;
        }

        private void BuildCorridorScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var psb = new CorridorSceneryBuilder(config, plgnf);
            psb.Initialize();

            sb = psb.SceneryModule;
            prtlm = psb.PortalBuilder;
        }
        private void BuildCorridorTScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var psb = new CorridorTSceneryBuilder(config, plgnf);
            psb.Initialize();

            sb = psb.SceneryModule;
            prtlm = psb.PortalBuilder;
        }
        private void BuildShipScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var psb = new ShipSceneryBuilder(config, plgnf);
            psb.Initialize();

            sb = psb.SceneryModule;
            prtlm = psb.PortalBuilder;
        }

        private void BuildSubwayStationScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var sssb = new VasileostrovskayaSceneryBuilder(config, plgnf);
            sssb.Initialize();

            sb = sssb.SceneryModule;
            prtlm = sssb.PortalBuilder;
        }

        private void BuildSubwayStationValidationScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var sssb = new VasileostrovskayaValidationSceneryBuilder(config, plgnf);
            sssb.Initialize();

            sb = sssb;
            prtlm = sssb.PortalBuilder;
        }

        private void BuildLuchegorskScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var lsb = new LuchegorskSceneryBuilder(config, plgnf);
            lsb.Initialize();

            sb = lsb.SceneryModule;
            prtlm = lsb.PortalBuilder;
        }

        private void BuildCinemaScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var csb = new CinemaSceneryBuilder(config, plgnf);
            csb.Initialize();

            sb = csb.SceneryModule;
            prtlm = csb.PortalBuilder;
        }

        private void BuildSeaStationRoughScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var sssb = new SeaStationSceneryBuilder(config, plgnf);
            sssb.Initialize();

            sb = sssb.SceneryModule;
            prtlm = sssb.PortalBuilder;
        }

        private void BuildTrainScenery(PulseScenarioConfig config, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var tbr = new TrainDataReader(config.DataDir.Value);
            var tclr = new TrainCarsListReader(config.DataDir.Value);

            var tc = new CarBuilder(tbr, tclr);
            tc.Initialize();

            var schr = new RouteRtiScheduleReader(config.DataDir.Value, config.TimeStart.Value);
            schr.Initialize();

            prtlm = new TrainPortalBuilder(tc.Train, schr.Schedule);
            prtlm.Initialize();
            sb = new TrainScenery(tc.Train, schr);
        }

        private static void BuildPulkovoScenery(PulseScenarioConfig config, PluginFactory plgnf,
            ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var psb = new PulkovoSceneryBuilder(config, plgnf);
            psb.Initialize();

            sb = psb.SceneryModule;
            prtlm = psb.PortalBuilder;
        }

        private void BuildVasilyevskyScenery(PulseScenarioConfig config, GeoCartesUtil gcu, PluginFactory plgnf,
            ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var abr = new AggregatedBuildingDataReader(config.DataDir.Value);
            abr.Initialize();
            var bm = new BuildingManager(gcu, abr, plgnf);
            bm.Initialize();
            var rr = new OsmRoadGraphDataReader(config.DataDir.Value, gcu);
            rr.Initialize();
            var prtlr = new GeoPortalDataReader(config.DataDir.Value);
            prtlm = new GeoPortalBuilder(prtlr, gcu);
            prtlm.Initialize();
            sb = new VasilIslandScenery(bm.Buildings, rr.Graph, prtlm.Portals);
        }

        private void BuildNetCityScenery(PulseScenarioConfig config, GeoCartesUtil gcu, PluginFactory plgnf, ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            var abr = new AggregatedBuildingDataReader(config.DataDir.Value);
            abr.Initialize();

            var poip = new PoiPreparer(gcu, abr, plgnf);
            poip.Initialize();

            sb = new NetCityScenery(poip.Buildings, new List<IExternalPortal>(), config, plgnf);
        }

        public override PulseScenarioConfig GetConfig(string scenario)
        {
            return new JsonPulseScenarioConfig(GetConfigFileName(scenario));
        }

        public string GetConfigFileName(string scenario)
        {
            return String.Format("Scenario_{0}.json", scenario);
        }

        public override PulseEngine GetEngine(PulseScenarioConfig config, SimulationRunMode simulationRunMode)
        {
            return GetEngine(config, new PulseAgentFactory(), simulationRunMode);
        }
    }
}
