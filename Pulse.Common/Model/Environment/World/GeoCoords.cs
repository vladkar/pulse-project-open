using System;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.World
{
    [Serializable]
    public class GeoCoords
    {
        public  GeoCoords()
        {
        }

        public GeoCoords(double lat, double lon)
        {
            Lon = lon;
            Lat = lat;
        }

        public double Lon { get; set; }
        public double Lat { get; set; }


        /// <summary>
        /// Distance to point B in meters
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
        public double DistanceTo(GeoCoords B)
        {
            var R = 6371;
            var dLat = (B.Lat - Lat)*Math.PI/180;
            var dLon = (B.Lon - Lon)*Math.PI/180;
            var rLatA = Math.PI/180*Lat;
            var rLatB = Math.PI/180*B.Lat;

            var a = Math.Sin(dLat/2)*Math.Sin(dLat/2) +
                    Math.Sin(dLon/2)*Math.Sin(dLon/2)*Math.Cos(rLatA)*Math.Cos(rLatB);

            var c = 2*Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = R*c;

            return d*1000;
        }

        public override string ToString()
        {
            return String.Format("(Lon: {0}, Lat: {1})", Lon, Lat);
        }

        public static bool operator ==(GeoCoords left, GeoCoords right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GeoCoords left, GeoCoords right)
        {
            return !Equals(left, right);
        }

        public bool Equals(GeoCoords other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            //TODO round
            return other.Lat.Equals(Lat) && other.Lon.Equals(Lon);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(GeoCoords)) return false;
            return Equals((GeoCoords)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Lat.GetHashCode()*397) ^ Lon.GetHashCode();
            }
        }

        public PulseVector2 ToCoords()
        {
            return new PulseVector2(Lon, Lat);
        }
    }
}
