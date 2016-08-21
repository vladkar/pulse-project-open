using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiagentEngine.Pulse.Map;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Legend;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.PluginSystem.Util;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.GraphUtil;

namespace $pulsepre$$safeprojectname$
{
    public class $safeprojectname$SceneryFactory : ISceneryFactory
    {
        public ILegendable GetRoleManager(string name, IPopulationManager pm)
        {
            return null; // new SocioRoleLegend(pm.SocioClasses);
        }

        public void BuildScenery(PulseScenarioConfig config, GeoCartesUtil gcu, AbstractPulsePluginFactory plgnf,
            ref AbstractExternalPortalBuilder prtlm, ref PulseScenery sb)
        {
            #region config

            var inFileRel = @"scenery\map.json";
            var outFileRel = @"scenery\map_qtg.json";

            var offset = new Coords(0, 0);

            var qtConfig = new QuadTreeConfig
            {
                BottomLeftCorner = new Coords(-20, -20),
                Size = 160,
                MinSize = 0.5,
                MaxSize = 4
            };

            var pjrCfg = new PulseJsonReaderConfig
            {
                Levels = true,
                Way = true,
                Obstacle = true,
                Poi = true,
                Portal = true,
                Zone = false,
                Offset = offset
            };

            #endregion

            AutoGraphCheck(config, offset, qtConfig, inFileRel, outFileRel);


            var pjr = new PulseJsonReader("Pulse json reader", pjrCfg, config.DataDir.Value, outFileRel);
            pjr.Initialize();

            prtlm = new SimplePortalBuilder(pjr.PulseObject, GetSimplePortalLoaders());
            prtlm.Initialize();

            var slm = new SimpleLevelsBuilder(pjr.PulseObject, prtlm.Portals, null);
            slm.Initialize();

            var ssgp = new SimpleGraphBuilder(slm.Levels, pjr.PulseObject);
            ssgp.Initialize();

            sb = new PulseScenery
            {
                Levels = slm.Levels,
                Graph = ssgp.Graph
            };

            RegisterPlugins(sb, plgnf);
        }

        private void AutoGraphCheck(PulseScenarioConfig config, Coords offset, QuadTreeConfig qtConfig, string inFileRel, string outFileRel)
        {
            var inFileAbs = Path.Combine(config.DataDir.Value, inFileRel);
            var outFileAbs = Path.Combine(config.DataDir.Value, outFileRel);


            if (File.Exists(inFileAbs))
            {
                var pjrCfgMap = new PulseJsonReaderConfig
                {
                    Levels = true,
                    Obstacle = true,
                    Poi = true,
                    Portal = true,
                    Offset = offset
                };

                var pjrMap = new PulseJsonReader("Pulse json reader", pjrCfgMap, config.DataDir.Value, inFileRel);
                pjrMap.Initialize();

                if (File.Exists(outFileAbs))
                {
                    var pjrQtgHyp = new PulseJsonReader("Pulse json reader", pjrCfgMap, config.DataDir.Value, outFileRel);
                    pjrQtgHyp.Initialize();

                    if (pjrMap.PulseObject.Version == pjrQtgHyp.PulseObject.Version)
                    {
                        return;
                    }
                }


                var prtlmMap = new SimplePortalBuilder(pjrMap.PulseObject, GetSimplePortalLoaders());
                prtlmMap.Initialize();

                var slmMap = new SimpleLevelsBuilder(pjrMap.PulseObject, prtlmMap.Portals, null);
                slmMap.Initialize();

                var maxx = slmMap.Levels.First().Value.Obstacles.Max(o => o.Max(p => p.X)) + 1;
                var maxy = slmMap.Levels.First().Value.Obstacles.Max(o => o.Max(p => p.Y)) + 1;

                var minx = slmMap.Levels.First().Value.Obstacles.Min(o => o.Min(p => p.X)) + 1;
                var miny = slmMap.Levels.First().Value.Obstacles.Min(o => o.Min(p => p.Y)) + 1;


                var qtgl = new MultilevelQuadTreeGraphLoader(slmMap.Levels, qtConfig);
                qtgl.Initialize();

                qtgl.AddOrUpdateGraph(inFileAbs, outFileAbs, qtgl.LeveledSubGraphs);
            }
        }

        private IDictionary<string, Func<ILevelPortal, IExternalPortal>> GetSimplePortalLoaders()
        {
            return new Dictionary<string, Func<ILevelPortal, IExternalPortal>>
            {
                {"example_killer", portal => new ExternalPortal(portal)},

                {"example_generator", portal =>
                    {
                        var ep = new ExternalPortal(portal);
                        ep.AgentGenerator = new $safeprojectname$SimpleAgentsGenerator(ep);
                        return ep;
                    }
                }
            };
        }

        private void RegisterPlugins(PulseScenery building, AbstractPulsePluginFactory plgnf)
        {
            building.Levels.Values.ToList().ForEach(l => l.PointsOfInterest.ToList().ForEach(poi => poi.RegisterPugins(plgnf.GetPluginsForBuilding())));
            building.Levels.Values.ToList().ForEach(l => l.ExternalPortals.ToList().ForEach(p => p.RegisterPugins(plgnf.GetPluginsForBuilding())));
            building.Levels.Values.ToList().ForEach(l => l.LevelPortals.ToList().ForEach(p => p.RegisterPugins(plgnf.GetPluginsForBuilding())));
        }
    }
}