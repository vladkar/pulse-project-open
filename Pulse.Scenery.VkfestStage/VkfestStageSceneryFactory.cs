using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NavigField;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Legend;
using Pulse.Common.NavField;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.PluginSystem.Util;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Quad;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageSceneryFactory : ISceneryFactory
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

            var offset = new PulseVector2(0, 0);

            var pjrCfg = new PulseJsonReaderConfig
            {
                Levels = true,
                Way = false,
                Obstacle = true,
                Poi = true,
                Portal = true,
                Zone = false,
                Offset = offset,
                Navfield = false
            };

            #endregion


            var pjr = new PulseJsonReader("Pulse json reader", pjrCfg, config.DataDir.Value, inFileRel);
            pjr.Initialize();

            foreach (var level in pjr.PulseObject.Levels)
            {
                level.Value.PointsOfInterest.ToList().ForEach(p => { p.Point = ClipperUtil.GetCentroid(p.Polygon); p.NavgationBlock = new PoiNavigationBlockPoint(p.Point); });
                level.Value.Portals.ToList().ForEach(p => { p.Point = ClipperUtil.GetCentroid(p.Polygon); p.NavgationBlock = new PoiNavigationBlockPoint(p.Point); });
            }

            pjr.PulseObject.Levels.Values.First().PointsOfInterest.ToList().ForEach(p => p.Point = ClipperUtil.GetCentroid(p.Polygon));
            pjr.PulseObject.Levels.Values.First().Portals.ToList().ForEach(p => p.Point = ClipperUtil.GetCentroid(p.Polygon));

            prtlm = new SimplePortalBuilder(pjr.PulseObject, GetSimplePortalLoaders());
            prtlm.Initialize();

            var slm = new SimpleLevelsBuilder(pjr.PulseObject, prtlm.Portals, null);
            slm.Initialize();

            foreach (var zone in slm.Levels.First().Value.Zones)
            {
                foreach (var poi in slm.Levels.First().Value.PointsOfInterest)
                {
                    if (ClipperUtil.IsPolygonInPolygon(poi.Polygon, zone.Polygon))
                    {
                        zone.PointsOfInterests.Add(poi);
                        poi.Zone = zone;
                    }
                }

                foreach (var portal in slm.Levels.First().Value.ExternalPortals)
                {
                    if (ClipperUtil.IsPolygonInPolygon(portal.Polygon, zone.Polygon))
                    {
                        zone.PointsOfInterests.Add(portal);
                        portal.Zone = zone;
                    }
                }
            }

            sb = new PulseScenery
            {
                Levels = slm.Levels,
                Navigator = Navigators.NAVFIELD
            };

            RegisterPlugins(sb, plgnf);


            // TODO IMPORTANT four LINES
//                        var nfn = new PoiTreeNavfieldNavigator(sb);
//                        var t = nfn;
//                        if (true)
//                            NavFieldSerializerCsv(config, null, inFileRel, "obsolete", ref sb);

            PoiTreeNavfieldNavigator.NavFieldRegister = NavFieldDeserializerCsv(config);
            
        }
        

        private IDictionary<string, Func<ILevelPortal, IExternalPortal>> GetSimplePortalLoaders()
        {
            var intence = 1d;
            return new Dictionary<string, Func<ILevelPortal, IExternalPortal>>
            {
                {
                    "void hollow", portal =>
                    {
                        var ep = new ExternalPortal(portal);
                        ep.AgentGenerator = new VkfestStageAgentSimpleGenerator(ep, 3000);
//                        ep.AgentGenerator = new VkfestStageAgentBugProbeGenerator(ep);
                        return ep;
                    }
                },
                {
                    "probe", portal =>
                    {
                        var ep = new ExternalPortal(portal);
//                        ep.AgentGenerator = new VkfestStageAgentProbeGenerator(ep);
                        ep.AgentGenerator = new VkfestStageAgentSimpleGroupGenerator(ep, 1, 0);
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

        private Dictionary<string, INavfieldCalc> NavFieldDeserializerCsv(PulseScenarioConfig config)
        {
            var nfDir = Path.Combine(config.DataDir.Value, @"scenery\navfield");
            var navfieldstructure = new Dictionary<string, INavfieldCalc>();

            //            var outFile1Rel = @"scenery\navfield\nf_";
            //            var outFile2Rel = @"scenery\navfield\nf_prop_";

            var propFiles = Directory.GetFiles(nfDir, "*.json", SearchOption.TopDirectoryOnly);
            var fieldFiles = Directory.GetFiles(nfDir, "*.csv", SearchOption.TopDirectoryOnly);
            //                .Where(s => ext.Any(e => s.EndsWith(e))

            for (int f = 0; f < propFiles.Length; f++)
            {
                var propFile = propFiles[f];
                var fieldFile = fieldFiles[f];

                Regex regex = new Regex("nf_prop_(.*).json");
                var v = regex.Match(propFile);
                var poiId = v.Groups[1].ToString();

                var jProp = JObject.Parse(File.ReadAllText(propFile));



                var xSize = jProp["offset"]["xSize"].Value<int>();
                var ySize = jProp["offset"]["ySize"].Value<int>();

                var nfVar = new NavfieldCalc();

                var jOffset = jProp["size"];
                nfVar.NF = new NavigationFieldSpace(xSize, ySize);
                nfVar.NF._offset.X = jOffset["X"].Value<double>();
                nfVar.NF._offset.Y = jOffset["Y"].Value<double>();
                nfVar.NF._toPulseVector2Multiplier = jProp["toMeterMultiplier"].Value<double>();
                
                var nfGrid = new NavigationGridCell[xSize, ySize];

                //                var nfArrList = nfArr.Children().ToList();

                var reader = new StreamReader(File.OpenRead(fieldFile));
//                List<string> listA = new List<string>();
//                List<string> listB = new List<string>();
                var i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';').ToArray();

                    for (int j = 0; j < ySize; j++)
                    {

                        nfGrid[i, j] = new NavigationGridCell();
                        nfGrid[i, j].angle = Double.Parse(values[j]);
                        nfGrid[i, j].velocity = 1;
                    }
                    i++;
                }
                
                nfVar.NF.NavigFieldArray = nfGrid;

                navfieldstructure.Add(poiId, nfVar);
            }

            return navfieldstructure;
        }


        private void NavFieldSerializerCsv(PulseScenarioConfig config, IDictionary<string, INavfieldCalc> nfr,
            string inFileRel, string outFileRel, ref PulseScenery sb)
        {
            var inFileAbs = Path.Combine(config.DataDir.Value, inFileRel);
            var outFile1Rel = @"scenery\navfield\nf_";
            var outFile2Rel = @"scenery\navfield\nf_prop_";

            //var outFileAbs = Path.Combine(config.DataDir.Value, outFileRel);

            nfr = PoiTreeNavfieldNavigator.NavFieldRegister;

            foreach (var nfkvp in nfr)
            {
                var outFile1Abs = Path.Combine(config.DataDir.Value, $"{outFile1Rel}{nfkvp.Key}.csv");
                var outFile2Abs = Path.Combine(config.DataDir.Value, $"{outFile2Rel}{nfkvp.Key}.json");
                
                var nf = (nfkvp.Value as NavfieldCalc).NF;

                var jProp = new JObject();
                jProp["offset"] = new JObject(new JProperty("xSize", nf.xSize), new JProperty("ySize", nf.ySize));
                jProp["size"] = new JObject(new JProperty("X", nf._offset.X), new JProperty("Y", nf._offset.Y));
                jProp["toMeterMultiplier"] = nf._toPulseVector2Multiplier;

                using (var file = File.CreateText(outFile2Abs))
                using (var writer = new JsonTextWriter(file))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, jProp);
                }


                var strb = new StringBuilder();
                for (int i = 0; i < nf.xSize; i++)
                {
                    for (int j = 0; j < nf.ySize; j++)
                    {
                        strb.Append(nf.NavigFieldArray[i, j].angle.ToString());

                        if (j < nf.ySize - 1)
                            strb.Append(";");
                    }
                    strb.AppendLine();
                }

                File.WriteAllText(outFile1Abs, strb.ToString());
            }
        }
    }
}