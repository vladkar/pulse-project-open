using System.Collections.Generic;
using Pulse.Common.Utils.Graph;

namespace Pulse.Common.Utils.Algorithms
{
    public interface IGraphCalc<T1, T2>
    {
        IList<Vertex<T1, T2>> CalculatePath(Vertex<T1, T2> start, Vertex<T1, T2> dest);
    }
}
