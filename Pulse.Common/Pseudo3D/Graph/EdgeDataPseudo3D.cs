namespace Pulse.Common.Pseudo3D.Graph
{
    public interface IWeightedEdgeData
    {
        double Weight { set; get; }
    }

    public class EdgeDataPseudo3D : IWeightedEdgeData
    {
        public int Floor { set; get; }
        public double Weight { set; get; }
    }
}
