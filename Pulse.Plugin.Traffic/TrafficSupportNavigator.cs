//extern alias traf;
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using traf.MultiagentEngine.Map;
//using Pulse.Common.Model;
//using Pulse.Common.Pseudo3D;
//using Pulse.Common.Pseudo3D.Graph;
//using Pulse.Common.Utils.Algorithms;
//using Pulse.Common.Utils.Algorithms.Astar;
//using Pulse.Common.Utils.Graph;
//
//
//namespace Pulse.Plugin.Traffic
//{
//    public class TrafficSupportNavigator : IPseudo3DAgentNavigator
//    {
//        private readonly RoadGraphPseudo3Dv2<VertexDataPseudo3D, EdgeDataPseudo3D> _graph;
//        private IGraphCalc<VertexDataPseudo3D, EdgeDataPseudo3D> _calc;
//
//        public TrafficSupportNavigator(RoadGraphPseudo3Dv2<VertexDataPseudo3D, EdgeDataPseudo3D> graph)
//        {
//            _graph = graph;
//            var distanceFunc = new Func<VertexDataPseudo3D, VertexDataPseudo3D, double>((x, y) => x.Point.DistanceTo(y.Point));
//            _calc = new AstarCalc<VertexDataPseudo3D, EdgeDataPseudo3D>(_graph, distanceFunc);
//        }
//
//        public IList<ITravelPath> GeneratePath(Coords start, Coords dest, int levelStart, int levelDest)
//        {
//            var startNode = _graph.GetNearestWaypointSafe(levelStart, start);
//            var endNode = _graph.GetNearestWaypointSafe(levelDest, dest);
//
//            var graphPath = _calc.CalculatePath(startNode, endNode);
//
//            return BuildComplexPath(graphPath, start, dest);
//        }
//
//        private IList<ITravelPath> BuildComplexPath(IList<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>> path, Coords start, Coords dest)
//        {
//            var segmentedComplexPath = new Dictionary<int, ICollection<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>>();
//            var seg = 1;
//            segmentedComplexPath.Add(seg, new List<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>());
//            segmentedComplexPath[seg].Add(path[0]);
//            for (var i = 1; i < path.Count; i++)
//            {
//                if (path[i].NodeData.GetType() != path[i - 1].NodeData.GetType())
//                {
//                    seg++;
//                    segmentedComplexPath.Add(seg, new List<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>());
//                }
//                segmentedComplexPath[seg].Add(path[i]);
//            }
//
//            IList<ITravelPath> complexPath = segmentedComplexPath.Select(e => ExtractMovementUnit(e.Value)).ToList();
//
//            var firstSeg = complexPath.First() as SimpleTravelPath;
//            var lastSeg = complexPath.Last() as SimpleTravelPath;
//
//            if (firstSeg != null)
//                if (firstSeg.SimplePath.First() != start)
//                    firstSeg.SimplePath.Insert(0, start);
//
//            if (lastSeg != null)
//                if (lastSeg.SimplePath.Last() != dest)
//                    lastSeg.SimplePath.Add(dest);
//
//            return complexPath;
//        }
//
//        private ITravelPath ExtractMovementUnit(IEnumerable<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>> nodes)
//        {
//            if (nodes.First().NodeData.GetType() == typeof(VertexDataPseudo3D))
//            {
//                return new SimpleTravelPath { SimplePath = nodes.Select(n => n.NodeData.Point).ToList() };
//            }
//
//            if (nodes.First().NodeData.GetType() == typeof(VertexDataPseudo3DPortal))
//            {
//                var enter = nodes.First().NodeData as VertexDataPseudo3DPortal;
//                var exit = nodes.Last().NodeData as VertexDataPseudo3DPortal;
//
//                return new PortalTravelPath { Enter = enter.Exit, Exit = exit.Exit, Transporter = enter.Transporter };
//            }
//
//            throw new Exception("todo!");
//        }
//    }
//}