using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection;
using Pulse.Plugin.SubModel;
using Pulse.Plugin.Traffic;
using Pulse.Scenery.MovingPlatform.ComputationalPlugin;
using Pulse.Scenery.SubwayStation.Abstract.Plugin;
using Pulse.Scenery.SubwayStation.Vasileostrovskaya.ValidationExperiment.ComputationalPlugin;

namespace Pulse.Model.Config
{
    public class JsonPulseScenarioConfig : PulseJsonScenarioConfig
    {
        public JsonPulseScenarioConfig(string fileName) : base (fileName)
        {
            InitSenario();
        }

        private void InitSenario()
        {
            var jsnClasses = RawJsonConfig;

            var dir = GetInputDir() + Path.DirectorySeparatorChar + jsnClasses["Scenery"].Value<string>() + Path.DirectorySeparatorChar;
            DataDir = new BaseConfigField<string>(dir);
            Name = new BaseConfigField<string>(jsnClasses["ScenarioName"].Value<string>());
            VisNiceName = new BaseConfigField<string>(jsnClasses["VisName"].Value<string>());
            TimeStep = new BaseConfigField<double>(jsnClasses["TimeStep"].Value<double>());
            ToSecondsMultiplier = new BaseConfigField<double>(jsnClasses["ToSecondsMultiplier"].Value<int>());
            TimeStart = new BaseConfigField<DateTime>(jsnClasses["TimeStart"].Value<DateTime>());
            InitialPopulation = new BaseConfigField<int>(jsnClasses["InitialPopulation"].Value<int>());
            PreferredCoordinates = new BaseConfigField<string>(jsnClasses["PreferredCoordinates"].Value<string>());

            //SceneryBlock = new BaseConfigField<string>(jsnClasses["SimulationType"].Value<string>());

            MapPointMin = new BaseConfigField<PulseVector2>(new PulseVector2
            {
                X = jsnClasses["MapPointDL"]["X"].Value<int>(),
                Y = jsnClasses["MapPointDL"]["Y"].Value<int>()
            });
            MapPointMax = new BaseConfigField<PulseVector2>(new PulseVector2
            {
                X = jsnClasses["MapPointUR"]["X"].Value<int>(),
                Y = jsnClasses["MapPointUR"]["Y"].Value<int>()
            });
            MetersPerMapUnit = new BaseConfigField<double>(jsnClasses["ToMetersMultiplier"].Value<double>());
            GeoPointMin = new BaseConfigField<GeoCoords>(new GeoCoords
            {
                Lat = jsnClasses["GeoPointDL"]["Lat"].Value<double>(),
                Lon = jsnClasses["GeoPointDL"]["Lon"].Value<double>()
            });
            GeoPointMax = new BaseConfigField<GeoCoords>(new GeoCoords
            {
                Lat = jsnClasses["GeoPointUR"]["Lat"].Value<double>(),
                Lon = jsnClasses["GeoPointUR"]["Lon"].Value<double>()
            });

            Plugins = new BaseConfigField<string[]>(jsnClasses["ActivePlugins"].Values<string>().ToArray());
            PluginsConfig = new Dictionary<string, PulsePluginScenarioConfig>();

            JToken ms;
            IDictionary<string, string> msDict;
            if (jsnClasses.TryGetValue("MovementSystems", out ms) && ms.Children().Any())
                msDict = ms.Children<JProperty>().ToDictionary<JProperty, string, string>(c => c.Name, c => c.Value.Value<string>());
            else
                msDict = new Dictionary<string, string> { { "pedestrian", "default" } };

            if (Plugins.Value.Contains("SubModel"))
            {
                msDict["sub_model"] = "sub_model";
            }

            MovementSystems = new BaseConfigField<IDictionary<string, string>>(msDict);

            if (Plugins.Value.Contains("SimpleInfection"))
            {
                var simpleInfectionConfig = new SimpleInfectionScenarioConfig(this);
                simpleInfectionConfig.InfectionName = new BaseConfigField<string>(jsnClasses["Plugins"]["SimpleInfection"]["InfectionName"].Value<string>());
                simpleInfectionConfig.InfectionInitialization = new BaseConfigField<string>(jsnClasses["Plugins"]["SimpleInfection"]["InfectionInitialization"].Value<string>());
                PluginsConfig.Add(simpleInfectionConfig.Name.Value, simpleInfectionConfig);
            }

            if (Plugins.Value.Contains("Traffic"))
            {
                var evacuationConfig = new TrafficScenarioConfig(this);
                PluginsConfig.Add(evacuationConfig.Name.Value, evacuationConfig);
            }

            if (Plugins.Value.Contains("MovingPlatformPlugin"))
            {
                var movingPlatformConfig = new MovingPlatformScenarioConfig(this);
                PluginsConfig.Add(movingPlatformConfig.Name.Value, movingPlatformConfig);
            }

            if (Plugins.Value.Contains("VasStatPlugin"))
            {
                var vasStatConfig = new VasStatScenarioConfig(this);
                vasStatConfig.OutDir = new BaseConfigField<string>(jsnClasses["Plugins"]["VasStatPlugin"]["OutDir"].Value<string>());
                vasStatConfig.ExpSubdir = new BaseConfigField<string>(jsnClasses["Plugins"]["VasStatPlugin"]["ExpSubdir"].Value<string>());
                vasStatConfig.StatTime = new BaseConfigField<double>(jsnClasses["Plugins"]["VasStatPlugin"]["StatTime"].Value<double>());
                vasStatConfig.StatTimeStart = new BaseConfigField<double>(jsnClasses["Plugins"]["VasStatPlugin"]["StatTimeStart"].Value<double>());
                vasStatConfig.StatTimeStep = new BaseConfigField<double>(jsnClasses["Plugins"]["VasStatPlugin"]["StatTimeStep"].Value<double>());
                PluginsConfig.Add(vasStatConfig.Name.Value, vasStatConfig);
            }

            if (Plugins.Value.Contains("SubModel"))
            {
                var evacuationConfig = new SubModelScenarioConfig(this);
                var mpLat = jsnClasses["Plugins"]["SubModel"]["MappingPointBL"]["Lat"].Value<double>();
                var mpLon = jsnClasses["Plugins"]["SubModel"]["MappingPointBL"]["Lon"].Value<double>();
                evacuationConfig.MappingPointBL = new BaseConfigField<GeoCoords>(new GeoCoords(mpLat, mpLon));
                evacuationConfig.SubScenario = new BaseConfigField<string>(jsnClasses["Plugins"]["SubModel"]["Scenario"].Value<string>());
                evacuationConfig.SubModel = new BaseConfigField<string>(jsnClasses["Plugins"]["SubModel"]["Type"].Value<string>());
                PluginsConfig.Add(evacuationConfig.Name.Value, evacuationConfig);
            }

            if (Plugins.Value.Contains("SubwayStation"))
            {
                var pluginConfig = new SubwayStationPluginConfig(this);
                PluginsConfig.Add(pluginConfig.Name.Value, pluginConfig);
            }

            if (jsnClasses["SF"] != null)
            {
                SfConfig["NeighborDist"] = jsnClasses["SF"]["NeighborDist"].Value<string>();
                SfConfig["MaxNeighbors"] = jsnClasses["SF"]["MaxNeighbors"].Value<string>();
                SfConfig["TimeHorizon"] = jsnClasses["SF"]["TimeHorizon"].Value<string>();
                SfConfig["ObsHorizon"] = jsnClasses["SF"]["ObsHorizon"].Value<string>();
                SfConfig["Radius"] = jsnClasses["SF"]["Radius"].Value<string>();
                SfConfig["MaxSpeed"] = jsnClasses["SF"]["MaxSpeed"].Value<string>();
                SfConfig["AccelerationCoefficient"] = jsnClasses["SF"]["AccelerationCoefficient"].Value<string>();
                SfConfig["RepulsiveAgent"] = jsnClasses["SF"]["RepulsiveAgent"].Value<string>();
                SfConfig["RepulsiveAgentFactor"] = jsnClasses["SF"]["RepulsiveAgentFactor"].Value<string>();
                SfConfig["RepulsiveObstacle"] = jsnClasses["SF"]["RepulsiveObstacle"].Value<string>();
                SfConfig["RepulsiveObstacleFactor"] = jsnClasses["SF"]["RepulsiveObstacleFactor"].Value<string>();
                SfConfig["Perception"] = jsnClasses["SF"]["Perception"].Value<string>();
                SfConfig["RelaxationTime"] = jsnClasses["SF"]["RelaxationTime"].Value<string>();
                SfConfig["Friction"] = jsnClasses["SF"]["Friction"].Value<string>();
                SfConfig["PlatformFactor"] = jsnClasses["SF"]["PlatformFactor"].Value<string>();
            }
        }


        private static string GetInputDir()
        {


            var assemblFile = Assembly.GetExecutingAssembly().Location;
            var assenblDir = Path.GetDirectoryName(assemblFile);
            var solutionDir = Directory.GetParent(assenblDir).Parent.Parent.FullName;

            #region fusion content

            var dataFContentDir = $@"{solutionDir}\Content\Data\";

            if (Directory.Exists(dataFContentDir))
                return dataFContentDir;

            #endregion

            #region  Deployed Assembly

            var dataAssemblyDirLocation = assenblDir + Path.DirectorySeparatorChar + "InputData" + Path.DirectorySeparatorChar;
            var isDataInAssemblyDir = Directory.Exists(dataAssemblyDirLocation);

            if (isDataInAssemblyDir)
                return dataAssemblyDirLocation;

            #endregion

            #region Visual Studio API

//            var solutionDir = Directory.GetParent(assenblDir).Parent.Parent.FullName;
//            var dataInVisualStudioLocation = solutionDir + Path.DirectorySeparatorChar + "Pulse.Model" +
//                                             Path.DirectorySeparatorChar + "InputData" + Path.DirectorySeparatorChar;
//            var isDataInVisualStudioDir = Directory.Exists(dataInVisualStudioLocation);
//            
//            if (isDataInVisualStudioDir)
//                return dataInVisualStudioLocation;

            #endregion

            #region Visual Studio API alt platform

//            var solutionDirP = Directory.GetParent(assenblDir).Parent.Parent.Parent.FullName;
//            var dataInVisualStudioLocationP = solutionDirP + Path.DirectorySeparatorChar + "Pulse.Model" +
//                                             Path.DirectorySeparatorChar + "InputData" + Path.DirectorySeparatorChar;
//            var isDataInVisualStudioDirP = Directory.Exists(dataInVisualStudioLocationP);
//
//            if (isDataInVisualStudioDirP)
//                return dataInVisualStudioLocationP;

            #endregion

            #region Visual Studio Visualizer

//            var solutionDirVis = Directory.GetParent(assenblDir).Parent.Parent.Parent.Parent.FullName;
//            var dataInVisualStudioLocationVis = solutionDirVis + Path.DirectorySeparatorChar + "Pulse.Model" +
//                                             Path.DirectorySeparatorChar + "InputData";
//            var isDataInVisualStudioDirVis = Directory.Exists(dataInVisualStudioLocationVis);
//            
//            if (isDataInVisualStudioDirVis)
//                return dataInVisualStudioLocationVis;

            #endregion

            #region Hardcode @deprecated

            //            var staticPath = @"C:\work\Downloaded\MobileExtreme3\VirtualSociety.Model\InputData";
//            var isDataInStaticPath = Directory.Exists(staticPath);
//            if (isDataInStaticPath)
//                return staticPath;
            #endregion

            throw new Exception("Input Data directory not found");
        }
    }
}
