using System.Collections.Generic;

namespace Pulse.Common.Utils.Algorithms.Astar
{
    public class AstarNodeDataComparer<T1, T2> : IComparer<AstarNodeData<T1, T2>>
    {
        public int Compare(AstarNodeData<T1, T2> x, AstarNodeData<T1, T2> y)
        {
            if (x.F > y.F)
                return 1;
            if (x.F < y.F)
                return -1;

            //reference equals
            if (x == y)
                return 0;

            return x.Vertex.Id.CompareTo(y.Vertex.Id);
        }
    }
}