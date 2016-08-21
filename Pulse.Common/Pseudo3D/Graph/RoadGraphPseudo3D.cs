using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Utils;
using Pulse.Common.Utils.BezgodovKdTree;
using Pulse.Common.Utils.Graph;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Pseudo3D.Graph
{
    public class RoadGraphPseudo3D<T1, T2> : Graph<T1, T2> 
        where T1 : VertexDataPseudo3D, new() 
        where T2 : EdgeDataPseudo3D, new()
    {
        private Dictionary<int, KdTree<Vertex<T1, T2>>> _kdTreeLeveled;
//        private Dictionary<int, Dictionary<Coords, Vertex<T1, T2>>> vertexRegisterLeveled;
        private Dictionary<int, Dictionary<PulseVector2, Vertex<T1, T2>>> _poiRegisterLeveled;

        public RoadGraphPseudo3D(IEnumerable<Edge<T1, T2>> edges)
            : base(edges)
        {
            _poiRegisterLeveled = new Dictionary<int, Dictionary<PulseVector2, Vertex<T1, T2>>>();
        }

        public RoadGraphPseudo3D()
            : base()
        {
            _poiRegisterLeveled = new Dictionary<int, Dictionary<PulseVector2, Vertex<T1, T2>>>();
        }

        public Vertex<T1, T2> FindClosest(PulseVector2 src)
        {
            Vertex<T1, T2> curMinNode = null;
            double curMinDist = 0;
            foreach (var curNode in Vertices.Values)
            {
                if (curMinNode == null)
                {
                    curMinNode = curNode;
                    curMinDist = src.DistanceSquared(curMinNode.NodeData.Point);
                    continue;
                }

                var curDist = curNode.NodeData.Point.DistanceSquared(src);
                if (curMinDist > curDist)
                {
                    curMinNode = curNode;
                    curMinDist = curDist;
                }
            }
            return curMinNode;
        }

//        public void BuildRegistry()
//        {
//            vertexRegisterLeveled = new Dictionary<int, Dictionary<Coords, Vertex<T1, T2>>>();
//
//            foreach (var vertex in Vertices.Values)
//            {
//                //not in register
//                if (vertex.NodeData.Point == null || vertex.NodeData.Floor == 0) continue;
//
//                if (!vertexRegisterLeveled.ContainsKey(vertex.NodeData.Floor))
//                    vertexRegisterLeveled.Add(vertex.NodeData.Floor,
//                        new Dictionary<Coords, Vertex<T1, T2>>());
//                vertexRegisterLeveled[vertex.NodeData.Floor].Add(vertex.NodeData.Point, vertex);
//            }
//        }

        public void BuildKdTree()
        {
            _kdTreeLeveled = new Dictionary<int, KdTree<Vertex<T1, T2>>>();

            foreach (var vertex in Vertices.Values)
            {
                if (vertex.NodeData.Point == null || vertex.NodeData.Floor == 0) continue;

                if (!_kdTreeLeveled.ContainsKey(vertex.NodeData.Floor))
                    _kdTreeLeveled.Add(vertex.NodeData.Floor, new KdTree<Vertex<T1, T2>>());
                _kdTreeLeveled[vertex.NodeData.Floor].Add(vertex.NodeData.Point, vertex);
            }
        }

        public Vertex<T1, T2> GetNearestWaypointSafe(int level, PulseVector2 target)
        {
            return GetNearestWaypointRegister(level, target) ?? GetNearestWaypointTree(level, target);
        }

        public Vertex<T1, T2> GetNearestWaypointRegister(int level, PulseVector2 target)
        {
            return _poiRegisterLeveled != null && _poiRegisterLeveled.ContainsKey(level) && _poiRegisterLeveled[level].ContainsKey(target) ? _poiRegisterLeveled[level][target] : null;
        }

        public Vertex<T1, T2> GetNearestWaypointTree(int level, PulseVector2 target)
        {
            Vertex<T1, T2> wp = null;
            _kdTreeLeveled[level].Nearest(target, ref wp);
            return wp;
        }

        public void AddPoisToGraph(IEnumerable<IPointOfInterest> pois)
        {
            var newVertices = new List<Vertex<T1, T2>>();
            var newEdges = new List<Edge<T1, T2>>();

            foreach (var poi in pois)
            {
                //todo 0 floor is temporary abstraction for pois, which is not on the map (invisible)
                if (poi.Level == 0)
                    continue;

                var etalon = new PulseVector2(0, 0);
                var poiVertex = new Vertex<T1, T2>
                {
                    Id = IdUtil.NextRandomId(),
                    NodeData = new T1
                    {
//                        Point = poi.Point ?? ClipperUtil.GetCentroid(poi.Polygon),
                        Point = poi.Point == etalon ? ClipperUtil.GetCentroid(poi.Polygon) : poi.Point,
                        Floor = poi.Level
                    },
                };
                var closestNode = GetNearestWaypointSafe(poiVertex.NodeData.Floor, poiVertex.NodeData.Point);
                if (closestNode.NodeData.Point.Equals(poiVertex.NodeData.Point))
                {
                    var buf = closestNode.NodeData.Point;
                    closestNode.NodeData.Point = new PulseVector2(buf.X + 0.000001, buf.Y);
                }
                newVertices.Add(poiVertex);
                var newEdge = new Edge<T1, T2>(poiVertex, closestNode);
                newEdge.EdgeData = new T2() {Weight = poiVertex.NodeData.Point.DistanceTo(closestNode.NodeData.Point)};
                newEdges.Add(newEdge);
                closestNode.Edges.Add(newEdge);
                poiVertex.Edges.Add(newEdge);

                var navBlock = new PoiNavigationBlockGraphv2<T1, T2>(poiVertex);
                poi.NavgationBlock = navBlock;



                if (!_poiRegisterLeveled.ContainsKey(poi.Level))
                    _poiRegisterLeveled[poi.Level] = new Dictionary<PulseVector2, Vertex<T1, T2>>();
                _poiRegisterLeveled[poi.Level][poi.TravelPoint] = poiVertex;
            }

            newVertices.ForEach(AddVertex);
            newEdges.ForEach(AddEdge);
        }
    }

    public class PoiNavigationBlockGraphv2<T1, T2> : IPointOfInterestNavgationBlock
        where T1 : VertexDataPseudo3D, new() 
        where T2 : EdgeDataPseudo3D
    {
        public Vertex<T1, T2> NavigationVertex { set; get; }

        public PoiNavigationBlockGraphv2(Vertex<T1, T2> poiVertex)
        {
            NavigationVertex = poiVertex;
        }

        public PulseVector2 GetNavigationPoint()
        {
            return NavigationVertex.NodeData.Point;
        }
    }
}
