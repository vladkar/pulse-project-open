using System;

namespace Pulse.Common.Model.Environment.World
{
    public class GeoClockContainer
    {
        /// <summary>
        /// Simulation Geo (y:m:d:h:m:s) time now
        /// </summary>
        public DateTime GeoTime { get; set; }



        public GeoClockContainer(DateTime start)
        {
            GeoTime = start;
        }
    }
}