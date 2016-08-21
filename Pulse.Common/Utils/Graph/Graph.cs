using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulse.Common.Utils.Graph
{
    public class 
        Graph<T1, T2>
    {
        public IDictionary<long, Vertex<T1, T2>> Vertices { set; get; }
        public IDictionary<long, Edge<T1, T2>> Edges { set; get; }

        public Graph()
        {
            Vertices = new Dictionary<long, Vertex<T1, T2>>();
            Edges = new Dictionary<long, Edge<T1, T2>>();
        }

        public Graph(IEnumerable<Edge<T1, T2>> edges) : this()
        {
            ApplyEdges(edges);
        }

        public void ApplyEdges(IEnumerable<Edge<T1, T2>> edges)
        {
            var edgesList = edges.ToList();
            var vertex = edgesList.AsParallel().Aggregate(new LinkedList<Vertex<T1, T2>>(), (set, item) =>
            {
                set.AddLast(item.NodeFrom);
                set.AddLast(item.NodeTo);
                return set;
            });
            var verticesSet = vertex.Distinct().ToDictionary(v =>v.Id, v => v);
            Edges = edgesList.Distinct().ToDictionary(e => e.Id, e => e);
            Vertices = FillEdgesForNodesAsParallel(edgesList, verticesSet);
        }

        private static IDictionary<string, Vertex<T1, T2>> FillEdgesForNodes(IEnumerable<Edge<T1, T2>> edges, Dictionary<string, Vertex<T1, T2>> vertex)
        {
            foreach (var node in vertex.Values)
                foreach (var edge in edges)
                    if (edge.NodeFrom.Id == node.Id | edge.NodeTo.Id == node.Id)
                        node.Edges.Add(edge);
            return vertex;
        }

        private static IDictionary<long, Vertex<T1, T2>> FillEdgesForNodesAsParallel(IEnumerable<Edge<T1, T2>> edges, Dictionary<long, Vertex<T1, T2>> vertex)
        {
            vertex.Values.AsParallel().ForAll(node =>
            {
                foreach (var edge in edges)
                    if (edge.NodeFrom.Id == node.Id | edge.NodeTo.Id == node.Id)
                        node.Edges.Add(edge);
            });

            return vertex;
        }

        private static IDictionary<string, Vertex<T1, T2>> FillEdgesForNodesAsParallelLinq(IEnumerable<Edge<T1, T2>> edges, Dictionary<string, Vertex<T1, T2>> vertex)
        {
            vertex.Values.AsParallel().ForAll(node =>
            {
                foreach (var edge in edges.Where(edge => edge.NodeFrom.Id == node.Id | edge.NodeTo.Id == node.Id))
                    node.Edges.Add(edge);
            });

            return vertex;
        }

        /*
         * Do not use
         * */
        private static IDictionary<string, Vertex<T1, T2>> FillEdgesForNodesIndexTest(IEnumerable<Edge<T1, T2>> edges, Dictionary<string, Vertex<T1, T2>> vertices)
        {
            vertices.AsParallel().ForAll(vp => vp.Value.Edges = edges.Where(e => e.NodeFrom.Id == vp.Value.Id | e.NodeTo.Id == vp.Value.Id).ToHashSet());
            
            return vertices;
        }

        public void AddVertex(Vertex<T1, T2> vertex)
        {
            if (!Vertices.Values.Contains(vertex))
                Vertices.Add(vertex.Id, vertex);
        }

        public Vertex<T1, T2> FindVertex(T1 vertexData)
        {
            return Vertices.FirstOrDefault(v => v.Value.NodeData.Equals(vertexData)).Value;
        }

        public void AddEdge(Edge<T1, T2> edge)
        {
            if (!Vertices.Values.Contains(edge.NodeFrom))
                Vertices.Add(edge.NodeFrom.Id, edge.NodeFrom);
            if (!Vertices.Values.Contains(edge.NodeTo))
                Vertices.Add(edge.NodeTo.Id, edge.NodeTo);

            if (!edge.NodeFrom.Edges.Contains(edge))
                edge.NodeFrom.Edges.Add(edge);
            if (!edge.NodeTo.Edges.Contains(edge))
                edge.NodeTo.Edges.Add(edge);
            
            Edges.Add(edge.Id, edge);

        }

        public Edge<T1, T2> FindEdge(T1 e1, T1 e2)
        {
            return Edges.Values.FirstOrDefault(e => e.NodeFrom.NodeData.Equals(e1) && e.NodeTo.NodeData.Equals(e2));
        }
    }
}
