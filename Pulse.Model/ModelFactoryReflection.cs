using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Util;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Model.AgentNetwork;
using Pulse.Model.Agents;
using Pulse.Model.Config;
using Pulse.Model.Environment;
using Pulse.Model.MovementSystems;
using Pulse.MultiagentEngine.Engine;
using Pulse.Scenery.HarsiddhiTemple;
using Pulse.Scenery.Krestovsky;
using Pulse.Scenery.MahakalTemple;
using Pulse.Scenery.StudentHospital;
using Pulse.Scenery.StudentSportivnaya;
using Pulse.Scenery.StudentSubwayCar;
using Pulse.Scenery.SubwayStation.Abstract;
using Pulse.Scenery.Test28;
using Pulse.Scenery.Train;
using Pulse.Scenery.VkfestStage;
using Pulse.Social.Data;
using Pulse.Social.Population.General;

namespace Pulse.Model
{
    public class ModelFactoryReflection : AbstractPulseFactory
    {   
        public PulseEngine GetEngine(PulseScenarioConfig config, IPulseAgentFactory agentFactory, SimulationRunMode simulationRunMode)
        {
            var pccr = new PhysicalCapabilityClassReader(config.DataDir.Value);
            pccr.Initialize();

            var la = new LegendAggregator();
            var plgnf = new PluginFactory(config, la);
            var gcu = new GeoCartesUtil(config.MapConfig.MinGeo, config.MapConfig.MetersPerMapUnit);

            PulseScenery sb = null;
            AbstractExternalPortalBuilder prtlm = null;

            var psf = GetSceneryFactory(config);

            psf.BuildScenery(config, gcu, plgnf, ref prtlm, ref sb);
            sb.Initialize();

            la.Legendables.Add(sb);
            
            var secr = new SocialEconomyClassReader(config.DataDir.Value, sb);
            secr.Initialize();
            var cmr = new ClassMappingReader(config.DataDir.Value, pccr, secr);
            cmr.Initialize();
            var mbf = new MovementSystemFactory(config, sb);
            mbf.Initialize();

            var af = agentFactory;
            var anf = new AgentNavigatorFactory(sb);

            var pm = new PopulationManager(sb, cmr, af, mbf, config.GeoWorldInfo, gcu, plgnf, anf);
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

            la.Legendables.Add(psf.GetRoleManager("", pm));
            la.Legendables.Add(pm);

            var agen = new SimpleAgentsGenerator(pm, config.InitialPopulation.Value);
            var engine = new PulseEngine(world, config.SimulationProperties, agen, la, simulationRunMode);

            return engine;
        }

        private ISceneryFactory GetSceneryFactory(PulseScenarioConfig config)
        {
            var cfg = (config as JsonPulseScenarioConfig).RawJsonConfig;

            // Assembly loading: explicit
            //JToken targetAssemblyNameJ;
            //if (!cfg.TryGetValue("Assembly", out targetAssemblyNameJ)) throw new Exception($"Missing Assembly section in config");
            //var targetAssemblyName = targetAssemblyNameJ.ToString();

            // Assembly loading: implicit
            JToken sceneryNameJ;
            if (!cfg.TryGetValue("Scenery", out sceneryNameJ)) throw new Exception($"Missing Scenery section in config");
            var sceneryName = sceneryNameJ.ToString();
            var targetAssemblyName = $"Pulse.Scenery.{sceneryName}.dll";

            var curAssemblyDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var targetAssemblyPath = Path.Combine(curAssemblyDir, targetAssemblyName);
            if (!File.Exists(targetAssemblyPath)) throw new Exception($"Missing Scenery Assembly: {targetAssemblyName}");
            var targetAssembly = Assembly.LoadFile(targetAssemblyPath);
            var type = targetAssembly.GetTypes().First(t => t.GetInterfaces().Contains(typeof(ISceneryFactory)));
            var instance = Activator.CreateInstance(type) as ISceneryFactory;
            if (instance == null) throw new Exception($"Missing SceneryFactory: {targetAssemblyName}");

            return instance;
        }

        public override PulseScenarioConfig GetConfig(string scenario)
        {
            return new JsonPulseScenarioConfig(GetConfigFileName(scenario));
        }

        public string GetConfigFileName(string scenario)
        {
            var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "Config\\", "*", SearchOption.AllDirectories).ToList();

            if (File.Exists(files.First(f => Path.GetFileName(f).ToLower() == $"Scenario_{scenario}.json".ToLower())))
            {
                return files.First(f => Path.GetFileName(f).ToLower() == $"Scenario_{scenario}.json".ToLower());
            }
            else
            {
                throw new Exception($"Scnerio json config file not found: Scenario_{scenario}.json");
            }
        }

        public override PulseEngine GetEngine(PulseScenarioConfig config, SimulationRunMode simulationRunMode)
        {
            return GetEngine(config, new PulseAgentFactory(), simulationRunMode);
        }


        // TODO should be removed when autobuild system will be finished
        private void MethodForOkBuild()
        {
            object f;
            f = new KrestovskySceneryFactory();
            f = new Test28SceneryFactory();
            f = new StudentHospitalSceneryFactory();
            f = new StudentSportivnayaSceneryFactory();
            f = new StudentSubwayCarSceneryFactory();
            f = new AbstractSubwaySceneryFactory();
            f = new HarsiddhiSceneryFactory();
            f = new MahakalSceneryFactory();
            f = new TrainSceneryFactory();
            f = new VkfestStageSceneryFactory();
            var t = f;
        }
    }
}
