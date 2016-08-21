using System.Collections.Generic;
using Pulse.Common.Pseudo3D.Graph;

namespace Pulse.Common.Utils.Graph
{
    public class DirectedEdgeComparer : IEqualityComparer<Edge<VertexDataPseudo3D, EdgeDataPseudo3D>>
    {
        public bool Equals(Edge<VertexDataPseudo3D, EdgeDataPseudo3D> one, Edge<VertexDataPseudo3D, EdgeDataPseudo3D> two)
        {
            return one.NodeFrom.Equals(two.NodeFrom) && one.NodeTo.Equals(two.NodeTo);

        }

        public int GetHashCode(Edge<VertexDataPseudo3D, EdgeDataPseudo3D> item)
        {
            return item.NodeFrom.GetHashCode() + item.NodeTo.GetHashCode();
        }
    }

    public class UnDirectedEdgeComparer : IEqualityComparer<Edge<VertexDataPseudo3D, EdgeDataPseudo3D>>
    {
        public bool Equals(Edge<VertexDataPseudo3D, EdgeDataPseudo3D> one, Edge<VertexDataPseudo3D, EdgeDataPseudo3D> two)
        {
            return (one.NodeFrom.Equals(two.NodeFrom) && one.NodeTo.Equals(two.NodeTo)) || (one.NodeFrom.Equals(two.NodeTo) && one.NodeTo.Equals(two.NodeFrom));

        }

        public int GetHashCode(Edge<VertexDataPseudo3D, EdgeDataPseudo3D> item)
        {
            return item.NodeFrom.GetHashCode() + item.NodeTo.GetHashCode();

        }
    }
}
