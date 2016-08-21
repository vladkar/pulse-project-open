using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Pulse.Common.ConfigSystem;
using Pulse.Common.DeltaStamp;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Model.Legend;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Graph;
using Pulse.MultiagentEngine.Engine;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Generation;
using Pulse.MultiagentEngine.Map;
using Pulse.MultiagentEngine.Settings;

namespace Pulse.Common.Engine
{
    public class PulseEngine : SimulationEngine, ILegendable
    {
        public IPulseDelta Delta;

        private ILegendable _legendAggregator;
        private DeltaManager _deltaManager;

        public PulseScenarioConfig ScenarioConfig
        {
            get { return (World as PulseWorld).ScenarioConfig; }
        }


        public PulseEngine(PulseWorld world, SimulationProperties properties, IAgentGenerationModel generationModel,
            ILegendable legendAggregator, SimulationRunMode runMode) : base(world, properties, generationModel, runMode)
        {
            _legendAggregator = legendAggregator;
            //_deltaManager = new DeltaManager();

            Delta = new PulseDelta {Id = (long) Counters.IterationNumber};
        }

        public TimestampedData<IInfectionData> GetInfectionData()
        {
            var ret = new TimestampedData<IInfectionData>(World.Time, (World.Map as IPulseMap).GetInfectionData());
            return ret;
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return _legendAggregator.GetLegend();
        }

        public DateTime ConvertTime(double time)
        {
            var world = ((PulseWorld) World);
            return world.ScenarioConfig.TimeStart.Value + TimeSpan.FromSeconds(time);
        }

        protected override void Step()
        {
            //Delta = new PulseDelta { Id = (long)Counters.IterationNumber };

            base.Step();

            //var secStep = Properties.TimeProperties.ToSecondsMultiplier*Properties.TimeProperties.TimeStep;
            var secTime = Properties.TimeProperties.ToSecondsMultiplier*World.Time;

            var world = ((PulseWorld) World);

            world.GeoTime.GeoTime = world.ScenarioConfig.TimeStart.Value + TimeSpan.FromSeconds(secTime);


            //_deltaManager.Update(Delta);

            LogStep(world);
        }

        private void ZoneIntersection(PulseWorld world)
        {
            var map = World.Map as IPulseMap;
            var obstacles = map.Levels.First().Value.Obstacles;
            var agents = World.Agents.OfType<AbstractPulseAgent>();
            var zones= map.Levels.First().Value.Zones;

            var zone_r = zones.First(z => z.Name == "01_zone_right"); // straight
            var zone_l = zones.First(z => z.Name == "01_zone_left"); // snake

            var agents_r = agents.Where(a => ClipperUtil.IsPointInPolygon(a.Point, zone_r.Polygon)).ToList();
            var agents_l = agents.Where(a => ClipperUtil.IsPointInPolygon(a.Point, zone_l.Polygon)).ToList();
            
            if (agents_r.Any())
                Debug.Assert(true);

            if (agents_l.Any())
                Debug.Assert(true);
        }

        private void CrossBodyObstacleIntersection(PulseWorld world)
        {
            var map = World.Map as IPulseMap;
            var obstacles = map.Levels.First().Value.Obstacles;
            var agents = World.Agents.OfType<AbstractPulseAgent>();

            Func<AbstractPulseAgent, PulseVector2[]> getCross = agent =>
            {
                var rad = 0.2;
                var rSqrt = Math.Sqrt(rad);

                var dl = agent.Point + new PulseVector2(-rSqrt, -rSqrt);
                var dr = agent.Point + new PulseVector2(-rSqrt, rSqrt);
                var ul = agent.Point + new PulseVector2(rSqrt, -rSqrt);
                var ur = agent.Point + new PulseVector2(rSqrt, rSqrt);

                return new PulseVector2[] {dl, ur, dr, ul};
            };

            foreach (var agent in agents)
            {
                var agentCross = getCross(agent);
                foreach (var obstacle in obstacles)
                {
                    var dlur = ClipperUtil.IsSegmentIntersectPolygon(new[] {agentCross[0], agentCross[1]}, obstacle);
                    var drul = ClipperUtil.IsSegmentIntersectPolygon(new[] {agentCross[2], agentCross[3]}, obstacle);

                    if (dlur | drul)
                        Debug.Assert(true);
                }
            }
        }

        private void SaveSchedNet(PulseWorld world)
        {
            var agents = world.Agents.OfType<AbstractPulseAgent>();
            var file = @"C:\Users\vladislav\Google Диск\2015.06.04_Urban_neyworks\exp\exp_outflow_per_node_3.json";

            var poiG = new Graph<IPointOfInterest, int>();
            foreach (var a in agents)
            {
                var sch = a.Role.PlannedDailyAgentSchedule.PlannedDailySchedule;

                var home = a.Home;
                var vh = poiG.FindVertex(home);

                    if (vh == null)
                        poiG.AddVertex(new Vertex<IPointOfInterest, int>{NodeData = home});

                foreach (var act in sch)
                {
                    var v = poiG.FindVertex(act.Poi);

                    if (v == null)
                        poiG.AddVertex(new Vertex<IPointOfInterest, int>{NodeData = act.Poi});
                }

                var schL = Enumerable.ToList<PlannedActivity>(sch);

                for (int i = 0; i < schL.Count; i++)
                {
                    IPointOfInterest e1;
                    IPointOfInterest e2;

                    if (i == 0)
                    {
                        e1 = a.Home;
                        e2 = schL[i].Poi;
                    }
                    else if (i == schL.Count - 1)
                    {
                        e1 = schL[i].Poi;
                        e2 = a.Home;
                    }
                    else
                    {
                        e1 = schL[i].Poi;
                        e2 = schL[i + 1].Poi;
                    }

                    var e = poiG.FindEdge(e1, e2);
                    if (e == null)
                    {
                        var v1 = poiG.FindVertex(e1);
                        var v2 = poiG.FindVertex(e2);

                        poiG.AddEdge(new Edge<IPointOfInterest, int> {EdgeData = 1, NodeFrom = v1, NodeTo = v2});
                    }
                    else
                        e.EdgeData += 1;
                }
            }

            var jo = JsonConvert.SerializeObject(new
            {
                Edges = poiG.Edges.Select(e => new
                {
                    Id = Convert.ToInt32(e.Key),
                    From = e.Value.NodeFrom.Id,
                    To = e.Value.NodeTo.Id,
                    Weight = e.Value.EdgeData
                }),
                Vertices = poiG.Vertices.Select(v => new
                {
                    Id = Convert.ToInt32(v.Key),
                    Type = v.Value.NodeData.Types.First().Name
                })
            });

            var sb = new StringBuilder();
            sb.Append(jo);

            using (var sw = File.AppendText(file))
            {
                sw.WriteLine(sb.ToString());
            }
            
//            var nodeRanks = poiG.Vertices.Select(v => v.Value.Edges.Count).OrderBy(v => v);
//            var edgesRanks = poiG.Edges.Values.GroupBy(ed => ed.EdgeData).ToDictionary(eg => eg.Key, eg => eg.Count());

//            var topEdges = poiG.Edges.Where(e => e.Value.EdgeData > 1);
        }

        private void LogStep(PulseWorld world)
        {
            if (Counters.IterationNumber%100 == 0)
                Log.Info(string.Format("Step {0}. GeoTime: {1}, Time: {2}, Agents: {3}", Counters.IterationNumber,
                    world.GeoTime.GeoTime, world.Time,
                    world.Agents.Count));
        }

        private void StatTest(PulseWorld world)
        {
            var agents = world.Agents.OfType<AbstractPulseAgent>();
            var file = @"C:\Users\vladislav\Google Диск\2015.06.04_Urban_neyworks\exp\exp_02.json__";

            var stepData = new Dictionary<string, int>();
            foreach (var a in agents)
            {
                if (!stepData.ContainsKey(a.CurrentActivity.GetType().Name))
                    stepData[a.CurrentActivity.GetType().Name] = 1;
                else
                    stepData[a.CurrentActivity.GetType().Name] += 1;
            }

            var jo = JsonConvert.SerializeObject(new {Time = world.GeoTime.GeoTime, Data = stepData});
            var sb = new StringBuilder();
            sb.Append(jo);
            sb.AppendLine(",");

            using (var sw = File.AppendText(file))
            {
                sw.WriteLine(sb.ToString());
            }
        }
    }
}