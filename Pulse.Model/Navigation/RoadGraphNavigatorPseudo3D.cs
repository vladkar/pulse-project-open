using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Utils.Algorithms;
using Pulse.Common.Utils.Algorithms.Astar;
using Pulse.Common.Utils.Graph;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Navigation
{
    public class RoadGraphNavigatorPseudo3D : IPseudo3DAgentNavigator
    {
        private readonly RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> _graph;
        private IGraphCalc<VertexDataPseudo3D, EdgeDataPseudo3D> _calc;

        public RoadGraphNavigatorPseudo3D(RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> graph)
        {
            _graph = graph;
            var euclidFunc = new Func<VertexDataPseudo3D, VertexDataPseudo3D, double>((x, y) => x.Point.DistanceTo(y.Point));
            var distanceSquaredFunc = new Func<VertexDataPseudo3D, VertexDataPseudo3D, double>((x, y) => x.Point.DistanceSquared(y.Point));
            var manhHeuristic = new Func<VertexDataPseudo3D, VertexDataPseudo3D, double>((x, y) => Math.Abs(x.Point.X - y.Point.X) + Math.Abs(x.Point.Y - y.Point.Y));

            var distFunc = euclidFunc;
            var heuristicFunc = euclidFunc;

            _calc = new AstarCalc<VertexDataPseudo3D, EdgeDataPseudo3D>(_graph, distFunc, heuristicFunc);
//            _calc = new TestCalc<VertexDataPseudo3D, EdgeDataPseudo3D>();
        }

        public IList<ITravelPath> GeneratePath(PulseVector2 start, PulseVector2 dest, int levelStart, int levelDest)
        {
            Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> startNode;
            Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> endNode;

                startNode = _graph.GetNearestWaypointSafe(levelStart, start);
                endNode = _graph.GetNearestWaypointSafe(levelDest, dest);

                var graphPath = _calc.CalculatePath(startNode, endNode);

                return BuildComplexPath(graphPath, start, dest);
        }

        public PulseVector2 Getvelocity(PulseVector2 pos, PulseVector2 dest, int level)
        {
            throw new NotImplementedException();
        }

        private IList<ITravelPath> BuildComplexPath(IList<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>> path, PulseVector2 start, PulseVector2 dest)
        {
            var segmentedComplexPath = new Dictionary<int, ICollection<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>>();
            var seg = 1;
            segmentedComplexPath.Add(seg, new List<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>());
            segmentedComplexPath[seg].Add(path[0]);
            for (var i = 1; i < path.Count; i++)
            {
                if (path[i].NodeData.GetType() != path[i - 1].NodeData.GetType())
                {
                    seg++;
                    segmentedComplexPath.Add(seg, new List<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>>());
                }
                segmentedComplexPath[seg].Add(path[i]);
            }

            IList<ITravelPath> complexPath = segmentedComplexPath.Select(e => ExtractMovementUnit(e.Value)).ToList();

            var firstSeg = complexPath.First() as SimpleTravelPath;
            var lastSeg = complexPath.Last() as SimpleTravelPath;

            if (firstSeg != null)
                if (firstSeg.SimplePath.First() != start)
                    firstSeg.SimplePath.Insert(0, start);

            if (lastSeg != null)
                if (lastSeg.SimplePath.Last() != dest)
                    lastSeg.SimplePath.Add(dest);

            return complexPath;
        }

        private ITravelPath ExtractMovementUnit(IEnumerable<Vertex<VertexDataPseudo3D, EdgeDataPseudo3D>> nodes)
        {
            if (nodes.First().NodeData.GetType() == typeof(VertexDataPseudo3D))
            {
                return new SimpleTravelPath { SimplePath = nodes.Select(n => n.NodeData.Point).ToList() };
            }

            if (nodes.First().NodeData.GetType() == typeof(VertexDataPseudo3DPortal))
            {
                var enter = nodes.First().NodeData as VertexDataPseudo3DPortal;
                var exit = nodes.Last().NodeData as VertexDataPseudo3DPortal;

                return new PortalTravelPath { Enter = enter.Exit, Exit = exit.Exit, Transporter = enter.Transporter };
            }

            throw new Exception("todo!");
        }
    }
}