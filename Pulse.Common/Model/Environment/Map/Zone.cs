using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.Map
{
    public class Zone : IUniqueObject
    {
        public string Name { set; get; }
        public string ObjectId { get; set; }
        public string Description { get; set; }
        public PulseVector2[] Polygon { get; set; }
        public ICollection<IPointOfInterest> PointsOfInterests { set; get; }

        public Zone()
        {
            PointsOfInterests = new List<IPointOfInterest>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}