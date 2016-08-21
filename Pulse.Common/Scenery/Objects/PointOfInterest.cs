using System;
using System.Collections.Generic;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Objects
{
    public class PointOfInterest : AbstractPulseObject, IPointOfInterest
    {
        public PluginsContainer PluginsContainer { get; set; }
        public PulseVector2 TravelPoint { get { return GetEntrance(); } }
        public PulseVector2 Point { get; set; }
        public PulseVector2[] Polygon { get; set; }
        public IList<PointOfInterestType> Types { get; set; }
        public IPointOfInterestNavgationBlock NavgationBlock { get; set; }
        public int Level { get; set; }
        public Zone Zone { get; set; }

        public PulseVector2 GetTravelPoint(AbstractPulseAgent agen, Random r)
        {
            return ClipperUtil.GetRandomPointOnPolygon(Polygon);
        }

        private PulseVector2 GetEntrance()
        {
            return NavgationBlock.GetNavigationPoint();
        }
        
        public override string ToString()
        {
            return String.Format("ID: {0}, floor: {1}, point: {2}", ObjectId, Level, TravelPoint);
        }
    }
}
