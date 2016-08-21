using System;
using Pulse.MultiagentEngine.Containers;

namespace Pulse.Common.Model.Environment.World
{
    public abstract class AbstractGeoWorld : SimWorld
    {
        protected AbstractGeoWorld(string name) : base(name) { }

        protected AbstractGeoWorld(DateTime start, string name) : this(name)
        {
            GeoTime = new GeoClockContainer(start);
        }

        protected AbstractGeoWorld(GeoClockContainer clock, string name) : this(name)
        {
            GeoTime = clock;
        }

        public GeoClockContainer GeoTime { get; set; }

        public abstract GeoWorldGeneralInfo GetGeoWorldInfo();
    }
}
