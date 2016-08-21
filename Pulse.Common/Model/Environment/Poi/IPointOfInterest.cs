using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Pulse.Common.Model.Agent;
using Pulse.Common.PluginSystem.Interface;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Utils.Graph;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.Poi
{
    public interface IPointOfInterest : IPluginable, IPseudo3DObject, IZonable, IUniqueObject
    {
        PulseVector2 TravelPoint { get; }
        PulseVector2[] Polygon { get; set; }
        IList<PointOfInterestType> Types { set; get; }

        IPointOfInterestNavgationBlock NavgationBlock { set; get; }

        PulseVector2 GetTravelPoint(AbstractPulseAgent agent, Random r);
    }

    public interface IPointOfInterestNavgationBlock
    {
        PulseVector2 GetNavigationPoint();
    }

    public class PoiNavigationBlockGraph : IPointOfInterestNavgationBlock
    {
        public Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> NavigationVertex { set; get; }

        public PoiNavigationBlockGraph(Vertex<VertexDataPseudo3D, EdgeDataPseudo3D> poiVertex)
        {
            NavigationVertex = poiVertex;
        }

        public PulseVector2 GetNavigationPoint()
        {
            return NavigationVertex.NodeData.Point;
        }
    }

    public class PoiNavigationBlockPoint : IPointOfInterestNavgationBlock
    {
        public PulseVector2 Point { set; get; }

        public PoiNavigationBlockPoint(PulseVector2 point)
        {
            Point = point;
        }

        public PulseVector2 GetNavigationPoint()
        {
            return Point;
        }
    }
}
