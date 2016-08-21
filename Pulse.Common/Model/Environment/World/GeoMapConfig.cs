using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.World
{
    public class GeoMapConfig
    {
        public PulseVector2 Min { set; get; }
        public PulseVector2 Max { set; get; }
        public GeoCoords MinGeo { set; get; }
        public GeoCoords MaxGeo { set; get; }
        public double MetersPerMapUnit { set; get; }

        public GeoMapConfig(PulseVector2 min, PulseVector2 max, GeoCoords minGeo, double metersPerMapUnit)
        {
            Min = min;
            Max = max;
            MinGeo = minGeo;
            MetersPerMapUnit = metersPerMapUnit;
        }

        public GeoMapConfig(PulseVector2 min, PulseVector2 max, GeoCoords minGeo, GeoCoords maxGeo, double metersPerMapUnit) : this(min, max, minGeo, metersPerMapUnit)
        {
            MaxGeo = maxGeo;
        }
    }
}