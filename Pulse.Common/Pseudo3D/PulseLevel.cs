using System.Collections.Generic;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Pseudo3D
{
    public class PulseLevel
    {
        public int Floor { set; get; }
        public IList<PulseVector2[]> Obstacles { set; get; }
        public IList<IPointOfInterest> PointsOfInterest { get; set; }
        public IList<Zone> Zones { get; set; }
        /// <summary>
        /// Pois and portals
        /// </summary>
        public IDictionary<PointOfInterestType, List<IPointOfInterest>> TypedPointsOfInterest { get; set; }
        public IList<IExternalPortal> ExternalPortals { set; get; }
        public IList<ILevelPortal> LevelPortals { set; get; } 
    }
}
