namespace Pulse.Common.Utils.Graph
{
    public class Edge<T1, T2>
    {
        public long Id { get; set; }
        public Vertex<T1, T2> NodeFrom { get; set; }
        public Vertex<T1, T2> NodeTo { get; set; }
        public T2 EdgeData { set; get; }
        
        public static  IdUtil2 _idUtil = new IdUtil2();

        public Edge()
        {
            Id = _idUtil.NextId();
        }

        public Edge(Vertex<T1, T2> from, Vertex<T1, T2> to) : this()
        {
            NodeFrom = from;
            NodeTo = to;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Edge<T1, T2>)) return false;
            return NodeFrom.Equals((obj as Edge<T1, T2>).NodeFrom) && NodeTo.Equals((obj as Edge<T1, T2>).NodeTo);
        }
    }
}