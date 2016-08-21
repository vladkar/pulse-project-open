using System.Collections.Generic;
using Pulse.Common.Utils;
using Pulse.Common.Utils.Graph;

namespace Pulse.Common.Model.Agent.SocialNetworks
{
    public class SocialGraph : Graph<SocialNodeData, SocialConnectionData>
    {
        public SocialGraph() : base() { }

        public SocialGraph(IEnumerable<Vertex<SocialNodeData, SocialConnectionData>> vertices,
            IEnumerable<Edge<SocialNodeData, SocialConnectionData>> edges)
            : base(edges)
        {
            foreach (var vertex in vertices)
            {
                AddVertex(vertex);
            }
        }
    }

    public class SocialNodeData
    {
        public AbstractPulseAgent Agent { set; get; }
    }

    public class SocialConnectionData
    {
        public EnumElement ConnectionType { set; get; }

        public override string ToString()
        {
            return ConnectionType.Value;
        }
    }

    public static class SocialConnectionType
    {
        public static EnumElement Family { get; private set; }
        public static EnumElement Friend { get; private set; }

        static SocialConnectionType()
        {
            Family = new EnumElement { Key = 1, Value = "Family" };
            Friend = new EnumElement { Key = 2, Value = "Friend" };
        }
    }
}
