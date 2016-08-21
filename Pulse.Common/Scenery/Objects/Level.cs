using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Scenery.Objects
{
    public class Level : AbstractPulseObject
    {
        public int LevelFloor { set; get; }
        public ICollection<Obstacle> Obstacles { set; get; }
        public ICollection<Edge> Edges { get; set; }
        public IDictionary<long, Vertex> Vertices { get; set; }
        public ICollection<IPointOfInterest> PointsOfInterest { set; get; }
        public ICollection<LocalPortal> Portals { set; get; }
        public ICollection<Zone> Zones { set; get; }
        public ICollection<StaffPoint> StaffPoints { set; get; }

        public Level()
        {
            Obstacles = new Collection<Obstacle>();
            Edges = new Collection<Edge>();
            Vertices = new Dictionary<long, Vertex>();
            PointsOfInterest = new Collection<IPointOfInterest>();
            Portals = new Collection<LocalPortal>();
            Zones = new Collection<Zone>();
            StaffPoints = new Collection<StaffPoint>();
        }
    }
}
