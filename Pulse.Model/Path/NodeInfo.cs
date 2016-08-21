namespace VirtualSociety.Model.Path
{
    public interface IPathNodeInfo { }

    public class AstarNodeInfo : IPathNodeInfo
    {
        public AstarNodeInfo Parent { set; get; }
        public int F { set; get; }
        public int G { set; get; }
    }
}
