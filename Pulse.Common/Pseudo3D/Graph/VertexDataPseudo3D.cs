using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Pseudo3D.Graph
{
    public class VertexDataPseudo3D
    {
        public PulseVector2 Point { set; get; }
        public int Floor { set; get; }
        public string Description { set; get; }
    }

    public class VertexDataPseudo3DPortal : VertexDataPseudo3D
    {
        public LevelPortalTransporter Transporter { set; get; }
        public ILevelPortal Exit { set; get; }
        public ILevelPortal Entrance { set; get; }
    }
}
