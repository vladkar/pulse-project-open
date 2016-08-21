using Pulse.Common.Utils.Graph;

namespace Pulse.Common.Utils.Algorithms.Astar
{
    public class AstarNodeData<T1, T2>
    {
        public Vertex<T1, T2> Vertex { get; set; }
        public AstarNodeData<T1, T2> AstarParent { get; set; }

        // do not convert to auto property
        private double _h;
        public double H { get { return _h; } set { _h = value; } }
        private double _g;
        public double G { get { return _g; } set { _g = value; } }
        private double _f;
        public double F { get { return _f; } set { _f = value; } }
    }
}