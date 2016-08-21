using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Graph;
using Pulse.Common.Utils.Quad;
using Pulse.MultiagentEngine.Map;

namespace Pulse.GraphUtil
{

    public class QuadTreeConfig
    {
        public PulseVector2 BottomLeftCorner { get; set; }
        public double Size { get; set; }
        public double MinSize { get; set; }
        public double MaxSize { get; set; }
    }

    

    public class MultilevelQuadTreeGraphLoader : AbstractDataBroker
    {
//        private PulseObject _pulseObject;
        private Dictionary<int, PulseLevel> _levels;
        private Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> _getInitBfsQuadrantFunc;
//        private Dictionary<int, PulseLevel> dictionary;
//        private PulseObject pulseObject;
        private Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc;
        private QuadTreeConfig _qtConfig;

        public IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LeveledSubGraphs { get; set; }
        
        public MultilevelQuadTreeGraphLoader(Dictionary<int, PulseLevel> levels, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc)
        {
            _levels = levels;
            _getInitBfsQuadrantFunc = getInitBfsQuadrantFunc;
        }

        public MultilevelQuadTreeGraphLoader(Dictionary<int, PulseLevel> levels, Func<RasterizedQuadTree<RasterizeQuadTreeDataWrapper>, PulseLevel, Quadrant<RasterizeQuadTreeDataWrapper>[]> getInitBfsQuadrantFunc, QuadTreeConfig qtConfig) : this(levels, getInitBfsQuadrantFunc)
        {
            _qtConfig = qtConfig;
        }

        public MultilevelQuadTreeGraphLoader(Dictionary<int, PulseLevel> levels, QuadTreeConfig qtConfig) 
        {
            _levels = levels;
            _qtConfig = qtConfig;
            _getInitBfsQuadrantFunc = GetInitBfsQuadrantByRandomPortal;
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantByRandomPortal(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.ExternalPortals.Select(p => p).Distinct());
            return new[] { poisQuadrants.RandomChoise().Value.RandomChoise() };
        }

        public static Quadrant<RasterizeQuadTreeDataWrapper>[] GetInitBfsQuadrantByRandomPoi(
            RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, PulseLevel level)
        {
            var poisQuadrants = GetPoisQuadrants(levelTree, level.PointsOfInterest.Select(p => p).Distinct());
            return new[] { poisQuadrants.RandomChoise().Value.RandomChoise() };
        }

        public static IDictionary<IPointOfInterest, Quadrant<RasterizeQuadTreeDataWrapper>[]> GetPoisQuadrants(RasterizedQuadTree<RasterizeQuadTreeDataWrapper> levelTree, IEnumerable<IPointOfInterest> pois)
        {
            var quadrants = levelTree.QuadrantsChildless;
            var poisQuadrants = new Dictionary<IPointOfInterest, Quadrant<RasterizeQuadTreeDataWrapper>[]>();

            foreach (var poi in pois)
            {
                var poiQuadrants = quadrants.Where(quadrant =>
                {
                    // 1. quadrant data required
                    var data = quadrant.Data;
                    if (data == null)
                        return false;

                    // 2. is quadrant poi typed quadrant?
                    var poiW = data as PoiRasterizeQuadTreeDataWrapper;
                    if (poiW == null)
                        return false;

                    // 3. is quadrant belongs current poi?
                    return poiW.PointOfInterest == poi;
                }).ToArray();

                if (poiQuadrants.Length > 0)
                {
                    poisQuadrants[poi] = poiQuadrants;
                }
            }

            return poisQuadrants;
        }

        public void AddOrUpdateGraph(string inputFile, string outPutFile,
            IDictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> leveledSubGraphs, PulseVector2 offset = default(PulseVector2))
        {
            var jBuilding = JObject.Parse(File.ReadAllText(inputFile));

            foreach (var subGraphKvp in leveledSubGraphs)
            {
                var level = subGraphKvp.Key;
                var graph = subGraphKvp.Value;

                var jLevel = jBuilding["levels"].First(l => l["level"].Value<int>() == level);

                // no json graph
                if (jLevel.Children<JProperty>().All(c => c.Name != "way"))
                {
                    var ea = new JArray();
                    var ep = new JProperty("edges", ea);

                    var va = new JArray();
                    var vp = new JProperty("vertices", va);

                    var tp = new JProperty("type", "graph");

                    var wo = new JObject { tp, ep, vp };
                    var wp = new JProperty("way", wo);

                    (jLevel as JObject).Add(wp);
                }

                var jsonVertices = jBuilding["levels"].First(l => l["level"].Value<int>() == level)["way"]["vertices"] as JArray;
                var jsonEdges = jBuilding["levels"].First(l => l["level"].Value<int>() == level)["way"]["edges"] as JArray;

                jsonVertices.Children().ToList().ForEach(c => c.Remove());
                jsonEdges.Children().ToList().ForEach(c => c.Remove());


                foreach (var vertex in graph.Vertices)
                {
                    jsonVertices.Add(new JObject(new JProperty("Id", vertex.Value.Id),
                        new JProperty("Point",
                            new JObject(new JProperty("x", vertex.Value.NodeData.Point.X - offset.X),
                                new JProperty("y", vertex.Value.NodeData.Point.Y - offset.Y)))));
                }

                foreach (var edge in graph.Edges)
                {
                    jsonEdges.Add(new JObject(
                        new JProperty("from", edge.Value.NodeFrom.Id),
                        new JProperty("to", edge.Value.NodeTo.Id)));
                }
            }

            using (var file = File.CreateText(outPutFile))
            using (var writer = new JsonTextWriter(file))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, jBuilding);
            }
        }




        protected override void LoadData()
        {
            LeveledSubGraphs = LoadGraph(_levels);
        }

        private Dictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>> LoadGraph(Dictionary<int, PulseLevel> sbLevels)
             
        {
            var leveledQuadTrees = _levels.Keys.ToDictionary(level => level,
                level => new RasterizedQuadTree<RasterizeQuadTreeDataWrapper>(
                    _qtConfig.BottomLeftCorner,
                    _qtConfig.Size,
                    _qtConfig.MinSize,
                    _qtConfig.MaxSize
            ));

            var levelsSubGraphs = new Dictionary<int, RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>>();

            var getPoiPolygon = new Func<RasterizeQuadTreeDataWrapper, PulseVector2[]>(w =>
            {
                var poiW = w as PoiRasterizeQuadTreeDataWrapper;
                return poiW != null ? poiW.PointOfInterest.Polygon : new PulseVector2[] { };
            });

            var getObstaclePolygon = new Func<RasterizeQuadTreeDataWrapper, PulseVector2[]>(w =>
            {
                var obstW = w as ObstacleRasterizeQuadTreeDataWrapper;
                return obstW != null ? obstW.Obstacle : new PulseVector2[] { };
            });

            var quadrantSplitCondition = new Func<RasterizeQuadTreeDataWrapper, bool>(w => true);
            var quadrantGlueCondition = new Func<RasterizeQuadTreeDataWrapper, bool>(w => false);
            
            foreach (var treeLevelKvp in leveledQuadTrees)
            {
                var treeOstacles = sbLevels[treeLevelKvp.Key].Obstacles.Select(
                    o => new ObstacleRasterizeQuadTreeDataWrapper
                    {
                        State = QuadrantGraphNodeState.Obstacle,
                        Obstacle = o
                    }).ToArray<RasterizeQuadTreeDataWrapper>();

                var treePois = sbLevels[treeLevelKvp.Key].PointsOfInterest.Select(
                    poi => new PoiRasterizeQuadTreeDataWrapper()
                    {
                        State = QuadrantGraphNodeState.PointOfInterest,
                        PointOfInterest = poi
                    }).ToArray<RasterizeQuadTreeDataWrapper>();

                var treeExternalPortals = sbLevels[treeLevelKvp.Key].ExternalPortals.Select(
                    poi => new PoiRasterizeQuadTreeDataWrapper()
                    {
                        State = QuadrantGraphNodeState.PointOfInterest,
                        PointOfInterest = poi
                    }).ToArray<RasterizeQuadTreeDataWrapper>();
                
                treeLevelKvp.Value.AddItems(treePois, getPoiPolygon, quadrantSplitCondition, quadrantGlueCondition);
                treeLevelKvp.Value.AddItems(treeExternalPortals, getPoiPolygon, quadrantSplitCondition, quadrantGlueCondition);
                treeLevelKvp.Value.AddItems(treeOstacles, getObstaclePolygon, quadrantSplitCondition, quadrantGlueCondition);
                
                treeLevelKvp.Value.CalculateNeighbors();

                //adjacent islands
                var vertices = new Dictionary<long, Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>>();
                foreach (var initQuadrantBfs in _getInitBfsQuadrantFunc(treeLevelKvp.Value, sbLevels[treeLevelKvp.Key]))
                {
                    var adjacentVertices = GetAdjacentVerticesBfs(treeLevelKvp.Value, initQuadrantBfs).ToDictionary(v => v.Item2.GlobalPositionNum, v => v);
                    foreach (var vertex in adjacentVertices)
                    {
                        vertices[vertex.Key] = vertex.Value;
                    }
                }
                
                var edges = ConnectVertices(vertices);

                var graph = new RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D>
                {
                    Edges = edges.ToDictionary(e => e.Id, e => e),
                    Vertices = vertices.Select(v => v.Value.Item1).Distinct().ToDictionary(v => v.Id, v => v)
                };

                graph.BuildKdTree();
//                graph.AddPoisToGraph(sbLevels[treeLevelKvp.Key].PointsOfInterest);
//                graph.AddPoisToGraph(sbLevels[treeLevelKvp.Key].ExternalPortals);
//                graph.BuildKdTree();


                levelsSubGraphs[treeLevelKvp.Key] = graph;
            }

            return levelsSubGraphs;
        }

        #region BFS

        /*
         * 
         *  Breadth-first search http://en.wikipedia.org/wiki/Breadth-first_search
         *  
         *      1  procedure BFS(G,v) is
                2      let Q be a queue
                3      Q.enqueue(v)
                4      label v as discovered
                5      while Q is not empty
                6         v ← Q.dequeue()
                7         for all edges from v to w in G.adjacentEdges(v) do
                8             if w is not labeled as discovered
                9                 Q.enqueue(w)
                10                label w as discovered
         * 
         */
        private IEnumerable<Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>> GetAdjacentVerticesBfs(RasterizedQuadTree<RasterizeQuadTreeDataWrapper> G, Quadrant<RasterizeQuadTreeDataWrapper> vp)
        {

            /*
             * 
             * 
             * raw bfs implementation
             * 
             * 
             * 

            var v = vp;
            var q = new Queue<Quadrant<RasterizeQuadTreeDataWrapper>>();
            var discovered = new C5.LinkedList<Quadrant<RasterizeQuadTreeDataWrapper>>();
            q.Enqueue(v);
            discovered.Add(v);
            while (q.Count > 0)
            {
                v = q.Dequeue();
                foreach (var w in v.Neighbors)
                {
                    if (!discovered.Contains(w))
                    {
                        q.Enqueue(w);
                        discovered.Add(w);
                    }
                }
            }

             * 
             * 
             * 
             */


            var vertices = new HashSet<Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>>();


            var v = vp;
            var q = new Queue<Quadrant<RasterizeQuadTreeDataWrapper>>();
            var discovered = new HashSet<Quadrant<RasterizeQuadTreeDataWrapper>>();
            q.Enqueue(v);
            discovered.Add(v);
            var poi = (v.Data as PoiRasterizeQuadTreeDataWrapper).PointOfInterest;
            vertices.Add(new Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>(item1: new Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> { NodeData = new VertexDataPseudo3D { Floor = poi.Level, Point = v.Center } }, item2: v));
            while (q.Count > 0)
            {
                v = q.Dequeue();
                foreach (var w in v.Neighbors)
                {
                    if (w.Data != null && w.Data.State == QuadrantGraphNodeState.Obstacle) continue;
                    if (!discovered.Contains(w))
                    {
                        q.Enqueue(w);
                        discovered.Add(w);
                        vertices.Add(new Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>(item1: new Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> { NodeData = new VertexDataPseudo3D { Floor = poi.Level, Point = w.Center } }, item2: w));
                    }
                }
            }

            return vertices;
        }

        #endregion

        private IEnumerable<Edge<VertexDataPseudo3D, EdgeDataPseudo3D>> ConnectVertices(Dictionary<long, Tuple<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>, Quadrant<RasterizeQuadTreeDataWrapper>>> vertices)
        {
            var edges = new HashSet<Edge<VertexDataPseudo3D, EdgeDataPseudo3D>>(new UnDirectedEdgeComparer());

            foreach (var vertex in vertices)
            {
                foreach (var freeNeighbor in vertex.Value.Item2.Neighbors)
                {
                    if (freeNeighbor.Data != null && freeNeighbor.Data.State == QuadrantGraphNodeState.Obstacle) continue;
                    var nodeFrom = vertex.Value.Item1;
                    var vertexTo = vertices[freeNeighbor.GlobalPositionNum];
                    var nodeTo = vertexTo.Item1;

                    var edgeData = new EdgeDataPseudo3D
                    {
                        Floor = vertex.Value.Item2.Level,
                        Weight = vertex.Value.Item2.Center.DistanceTo(vertexTo.Item2.Center)
                    };

                    var edge = new Edge<VertexDataPseudo3D, EdgeDataPseudo3D>
                    {
                        NodeFrom = nodeFrom,
                        NodeTo = nodeTo,
                        EdgeData = edgeData
                    };

                    edge.NodeTo.Edges.Add(edge);
                    edge.NodeFrom.Edges.Add(edge);
                    edges.Add(edge);
                }
            }

            return edges;
        }
    }


    public enum QuadrantGraphNodeState { Obstacle, PointOfInterest }

    public abstract class RasterizeQuadTreeDataWrapper
    {
        public QuadrantGraphNodeState State { set; get; }
    }

    public class PoiRasterizeQuadTreeDataWrapper : RasterizeQuadTreeDataWrapper
    {
        public IPointOfInterest PointOfInterest { set; get; }
    }

    public class ObstacleRasterizeQuadTreeDataWrapper : RasterizeQuadTreeDataWrapper
    {
        public PulseVector2[] Obstacle { set; get; }
    }
}