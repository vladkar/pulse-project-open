using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Objects
{
    public class Edge
    {
        public Vertex From { get; set; }
        public Vertex To { get; set; }
    }

    public class Vertex
    {
        public long Id { get; set; }
        public PulseVector2 Point { get; set; }
    }
}
