using System;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Utils
{
    public class GeoCartesUtil
    {
        public GeoCoords MinGeo { get; set; }
        public double MetersPerMapUnit { get; set; } // сколько метров в 1 единице расстояния карты (размер клетки в метрах)

        public const int GeoRad = 6371;

        public GeoCartesUtil() { }

        public GeoCartesUtil(GeoCoords minGeo, double metersPerMapUnit)
        {
            MinGeo = minGeo;
            MetersPerMapUnit = metersPerMapUnit;
        }

        public Tuple<double, double> GetGeoCoordsTuple(double x, double y)
        {
            var distRaw = Math.Sqrt(x * x + y * y);
            var bearingRad = Math.Atan2(x, y);

            var lat1 = DegreeToRadian(MinGeo.Lat);
            var lon1 = DegreeToRadian(MinGeo.Lon);

            var dist = distRaw / 1000.0;

            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dist / GeoRad) + Math.Cos(lat1) * Math.Sin(dist / GeoRad) * Math.Cos(bearingRad));
            var lon2 = lon1 + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(dist / GeoRad) * Math.Cos(lat1), Math.Cos(dist / GeoRad) - Math.Sin(lat1) * Math.Sin(lat2));

            return new Tuple<double, double>(RadianToDegree(lat2), RadianToDegree(lon2));
        }

        public GeoCoords GetGeoCoords(double x, double y)
        {
            var distRaw = Math.Sqrt(x * x + y * y);
            var bearingRad = Math.Atan2(x, y);

            var lat1 = DegreeToRadian(MinGeo.Lat);
            var lon1 = DegreeToRadian(MinGeo.Lon);

            var dist = distRaw / 1000.0;

            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(dist / GeoRad) + Math.Cos(lat1) * Math.Sin(dist / GeoRad) * Math.Cos(bearingRad));
            var lon2 = lon1 + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(dist / GeoRad) * Math.Cos(lat1), Math.Cos(dist / GeoRad) - Math.Sin(lat1) * Math.Sin(lat2));

            return new GeoCoords(RadianToDegree(lat2), RadianToDegree(lon2));
        }

        public GeoCoords GetGeoCoords(PulseVector2 coords)
        {
            return GetGeoCoords(coords.X, coords.Y);
        }

        public PulseVector2 GetCoordsTuple(GeoCoords geoCoords)
        {
            var dLonAngle = DegreeToRadian(geoCoords.Lon - MinGeo.Lon);
            var rLat1Angle = DegreeToRadian(MinGeo.Lat);
            var rLat2Angle = DegreeToRadian(geoCoords.Lat);

            var yB = Math.Sin(dLonAngle) * Math.Cos(rLat2Angle);
            var xB = Math.Cos(rLat1Angle) * Math.Sin(rLat2Angle) - Math.Sin(rLat1Angle) * Math.Cos(rLat2Angle) * Math.Cos(dLonAngle);
            var angleRad = Math.Atan2(yB, xB);

            var dLat = DegreeToRadian(geoCoords.Lat - MinGeo.Lat);
            var dLon = DegreeToRadian(geoCoords.Lon - MinGeo.Lon);

            var rLatA = DegreeToRadian(MinGeo.Lat);
            var rLatB = DegreeToRadian(geoCoords.Lat);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLatA) * Math.Cos(rLatB);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = GeoRad * c * 1000;


            var x = d * Math.Sin(angleRad);
            var y = d * Math.Cos(angleRad);


            return new PulseVector2(Math.Round(MetersPerMapUnit * x, 6), Math.Round(MetersPerMapUnit * y, 6));
        }

        public Tuple<double, double> GetCoordsTuple(double Lon, double Lat)
        {
            var dLonAngle = DegreeToRadian(Lon - MinGeo.Lon);
            var rLat1Angle = DegreeToRadian(MinGeo.Lat);
            var rLat2Angle = DegreeToRadian(Lat);

            var yB = Math.Sin(dLonAngle) * Math.Cos(rLat2Angle);
            var xB = Math.Cos(rLat1Angle) * Math.Sin(rLat2Angle) - Math.Sin(rLat1Angle) * Math.Cos(rLat2Angle) * Math.Cos(dLonAngle);
            var angleRad = Math.Atan2(yB, xB);

            var dLat = DegreeToRadian(Lat - MinGeo.Lat);
            var dLon = DegreeToRadian(Lon - MinGeo.Lon);

            var rLatA = DegreeToRadian(MinGeo.Lat);
            var rLatB = DegreeToRadian(Lat);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(rLatA) * Math.Cos(rLatB);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = GeoRad * c * 1000;


            var x = d * Math.Sin(angleRad);
            var y = d * Math.Cos(angleRad);

            return new Tuple<double, double>(Math.Round(MetersPerMapUnit * x, 6), Math.Round(MetersPerMapUnit * y, 6));
        }

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }

    public enum CoordType { Map = 0, Geo = 1 }
}
