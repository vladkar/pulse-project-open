namespace Pulse.Common.Model.Environment.World
{
    public class GeoWorldGeneralInfo
    {
        public GeoClockContainer GeoTime { set; get; }
        public double MetersPerMapUnit { get; set; }
        public double ToSecondsMultiplier { get; set; }
    }
}