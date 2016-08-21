using System.Collections.Generic;

namespace Pulse.Common.Utils.Graph
{
    public class Vertex<T1, T2>
    {
        public long Id { get; set; }
        public ISet<Edge<T1, T2>> Edges { set; get; }
        public T1 NodeData { set; get; }

        public static IdUtil2 _idUtil = new IdUtil2();

        public Vertex()
        {
            Id = _idUtil.NextId();
            Edges = new HashSet<Edge<T1, T2>>();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Vertex<T1, T2>)) return false;
            return Id == (obj as Vertex<T1, T2>).Id;
        }
    }
}
