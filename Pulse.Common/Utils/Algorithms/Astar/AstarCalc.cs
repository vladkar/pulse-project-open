using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Utils.Graph;

namespace Pulse.Common.Utils.Algorithms.Astar
{
    public class TestCalc<T1, T2> : IGraphCalc<T1, T2>
    {
        public IList<Vertex<T1, T2>> CalculatePath(Vertex<T1, T2> start, Vertex<T1, T2> dest)
        {
            return new List<Vertex<T1, T2>> {start, dest};
        }
    }

    public class AstarCalc<T1, T2> : IGraphCalc<T1, T2> where T2: IWeightedEdgeData
    {
        public Graph<T1, T2> Graph { protected set; get; }
        public Func<T1, T1, double> DistanceFunction { protected set; get; }
        public Func<T1, T1, double> HeuristicFunction { protected set; get; }

        protected AstarCalc() { }

        public AstarCalc(Graph<T1, T2> graph, Func<T1, T1, double> distanceFunction)
            : this(graph, distanceFunction, distanceFunction)
        {
        }

        public AstarCalc(Graph<T1, T2> graph, Func<T1, T1, double> distanceFunction, Func<T1, T1, double> heuristicFunction)
        {
            Graph = graph;
            DistanceFunction = distanceFunction;
            HeuristicFunction = heuristicFunction;
        }

        public IList<Vertex<T1, T2>> CalculatePath(Vertex<T1, T2> start, Vertex<T1, T2> dest)
        {
            return CalculatePathIndex(start, dest);
        }

        private IList<Vertex<T1, T2>> CalculatePathIndex(Vertex<T1, T2> start, Vertex<T1, T2> dest)
        {
            var openList = new SortedSet<AstarNodeData<T1, T2>>(new AstarNodeDataComparer<T1, T2>());
            var openListSearch1 = new Dictionary<long, AstarNodeData<T1, T2>>();
            var closedList = new Dictionary<long, AstarNodeData<T1, T2>>();

            var startNodeData = new AstarNodeData<T1, T2>();
            startNodeData.G = 0;
            startNodeData.H = HeuristicFunction(start.NodeData, dest.NodeData);
            startNodeData.F = startNodeData.G + startNodeData.H;
            startNodeData.Vertex = start;

            openList.Add(startNodeData);
            openListSearch1.Add(startNodeData.Vertex.Id, startNodeData);

            while (openList.Count != 0)
            {
                var x = openList.Min;

                if (x.Vertex == dest)
                    return BuildPath(x);

                openList.Remove(x);
                openListSearch1.Remove(x.Vertex.Id);

//                if (!closedList.ContainsKey(x.Vertex.Id))
                    closedList.Add(x.Vertex.Id, x);

                var neighbors = x.Vertex.Edges;
                foreach (var edge in neighbors)
                {
                    var yVertex = x.Vertex.Id == edge.NodeFrom.Id ? edge.NodeTo : edge.NodeFrom;
                    if (closedList.ContainsKey(yVertex.Id))
                        continue;

                    //var tentativeG = x.G + DistanceFunction(x.Vertex.NodeData, yVertex.NodeData);
                    var tentativeG = x.G + edge.EdgeData.Weight;
                    var tentativeOk = true;

                    AstarNodeData<T1, T2> y;
                    openListSearch1.TryGetValue(yVertex.Id, out y);

                    if (y == null)
                    {
                        y = new AstarNodeData<T1, T2>();
                        y.Vertex = yVertex;
                        y.AstarParent = x;
                        y.G = DistanceFunction(y.Vertex.NodeData, start.NodeData);
                        y.H = HeuristicFunction(y.Vertex.NodeData, dest.NodeData);
                        y.F = y.H + y.G;
                        openList.Add(y);
                        openListSearch1[y.Vertex.Id] = y;
                    }
                    else
                    {
                        if (!(tentativeG < y.G))
                        {
                            tentativeOk = false;
                        }
                    }
                    if (tentativeOk)
                    {
                        openList.Remove(y);
                        openListSearch1.Remove(y.Vertex.Id);
                        y.AstarParent = x;
                        y.G = tentativeG;
                        y.H = HeuristicFunction(y.Vertex.NodeData, dest.NodeData);
                        y.F = y.H + y.G;
                        openList.Add(y);
                        openListSearch1[y.Vertex.Id] = y;
                    }
                }
            }

            throw new Exception("Impossible path");
        }
        
        private IList<Vertex<T1, T2>> BuildPath(AstarNodeData<T1, T2> endNodeData)
        {
            var linkedPath = new LinkedList<Vertex<T1, T2>>();
            var currentNodeData = endNodeData;

            while (currentNodeData != null)
            {
                linkedPath.AddFirst(currentNodeData.Vertex);
                currentNodeData = currentNodeData.AstarParent;
            }

            return linkedPath.ToList();
        }
    }
}