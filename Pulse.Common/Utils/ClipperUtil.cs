using System;
using System.Linq;
using Pulse.MultiagentEngine.Map;
using System.Xml.XPath;

namespace Pulse.Common.Utils
{
    public class ClipperUtil
    {
        public static bool IsAtLeastOnePointInAnother(PulseVector2[] polygon1, PulseVector2[] polygon2)
        {
            var pinp = polygon1.Select(point => IsPointInPolygon(point, polygon2)).Any(pointRes => pointRes);
            if (!pinp)
                pinp = polygon2.Select(point => IsPointInPolygon(point, polygon1)).Any(pointRes => pointRes);

            return pinp;
        }

        public static bool IsPolygonInPolygon(PulseVector2[] inner, PulseVector2[] outer)
        {
            return inner.Select(point => IsPointInPolygon(point, outer)).Aggregate(true, (current, pointRes) => current & pointRes);
        }

        public static bool IsPointInPolygon(PulseVector2 point, PulseVector2[] polygon)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if ((((polygon[i].Y <= point.Y) && (point.Y < polygon[j].Y)) ||
                    ((polygon[j].Y <= point.Y) && (point.Y < polygon[i].Y))) &&
                    (point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                    c = !c;
            }
            return c;
        }

        public static bool IsIntersectPolygon(PulseVector2[] polygon1, PulseVector2[] polygon2)
        {
            for (int i = 0; i < polygon1.Length - 1; i++)
                for (int j = 0; j < polygon2.Length - 1; j++)
                {
                    var line1 = new[] {polygon1[i], polygon1[i + 1]};
                    var line2 = new[] {polygon2[j], polygon2[j + 1]};
                    if (IsIntersectSegment(line1, line2))
                        return true;
                }            
            return false;
        }

        public static bool IsIntersectSegment(PulseVector2[] line1, PulseVector2[] line2)
        {
            var s1_x = line1[1].X - line1[0].X;
            var s1_y = line1[1].Y - line1[0].Y;

            var s2_x = line2[1].X - line2[0].X;
            var s2_y = line2[1].Y - line2[0].Y;

            var s = (-s1_y * (line1[0].X - line2[0].X) + s1_x * (line1[0].Y - line2[0].Y)) / (-s2_x * s1_y + s1_x * s2_y);
            var t = ( s2_x * (line1[0].Y - line2[0].Y) - s2_y * (line1[0].X - line2[0].X)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
//                var i = IntersectSegments(line1, line2);
                return true;
            }

//            True в случае наложения отрезков
//            if (Double.IsNaN(s) && Double.IsNaN(t))
//                return true;

            return false;
        }

        public static bool IsSegmentIntersectPolygon(PulseVector2[] segment, PulseVector2[] polygon)
        {
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                var polygonLine = new[] {polygon[i], polygon[i + 1]};
                if (IsIntersectSegment(segment, polygonLine))
                    return true;
            }

            return false;
        }

        public static PulseVector2 IntersectSegments(PulseVector2[] line1, PulseVector2[] line2)
        {
            var a0 = line1[1].Y - line1[0].Y;
            var b0 = line1[0].X - line1[1].X;
            var c0 = a0*line1[0].X + b0*line1[0].Y;

            var a1 = line2[1].Y - line2[0].Y;
            var b1 = line2[0].X - line2[1].X;
            var c1 = a1*line2[0].X + b1*line2[0].Y;

            var delta = a0*b1 - a1*b0;
            if (delta == 0)
                return default(PulseVector2);

            var x = (b1*c0 - b0*c1) / delta;
            var y = (a0*c1 - a1*c0) / delta;
            var interPoint = new PulseVector2(x, y);

            return interPoint;
        }

        public static PulseVector2 GetCentroid(PulseVector2[] polygon)
        {
            var accumulatedArea = 0.0d;
            var centerX = 0.0d;
            var centerY = 0.0d;

            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var temp = polygon[i].X * polygon[j].Y - polygon[j].X * polygon[i].Y;
                accumulatedArea += temp;
                centerX += (polygon[i].X + polygon[j].X) * temp;
                centerY += (polygon[i].Y + polygon[j].Y) * temp;
            }

//            if (accumulatedArea < 1E-7f)
//                return new Coords(0, 0);  // Avoid division by zero

            accumulatedArea *= 3d;

            return new PulseVector2(centerX / accumulatedArea, centerY / accumulatedArea);
        }

        public static PulseVector2 GetDistancePointOnGgment(PulseVector2 p1, PulseVector2 p2, double distance)
        {
            var product = Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
            var x = p1.X + distance * (p2.X - p1.X) / product;
            var y = p1.Y + distance * (p2.Y - p1.Y) / product;

            return new PulseVector2(x, y);
        }

        public static bool IsClockWise(PulseVector2[] polygon)
        {
            int i, j, k;
            int count = 0;
            double z;

            if (polygon.Length < 3)
                throw new Exception("This is not a polygon: should be at least 3 points");

            for (i = 0; i < polygon.Length; i++)
            {
                j = (i + 1) % polygon.Length;
                k = (i + 2) % polygon.Length;
                z = (polygon[j].X - polygon[i].X)*(polygon[k].Y - polygon[j].Y);
                z -= (polygon[j].Y - polygon[i].Y)*(polygon[k].X - polygon[j].X);
                if (z < 0)
                    count--;
                else if (z > 0)
                    count++;
            }
            if (count > 0)
                return (false);
            else if (count < 0)
                return (true);
            else
                throw new Exception("incomputable (colinear points)");
        }

        public static PulseVector2 GetRandomPointOnCircle(PulseVector2 center, double radius)
        {
            var d = radius * RandomUtil.RandomDouble();
            var a = RandomUtil.RandomInt(0, 360);

            return new PulseVector2(center.X + d * Math.Cos(a), center.Y + d * Math.Sin(a));
        }
        
//        public static Coords GetRandomPointOnSquare(Coords[] squarePolygon)
//        {
//            if (squarePolygon.Length != 4) throw new Exception("Square polygon required");
//
//            float x2 = float.MinValue, y2 = float.MinValue, x1 = float.MaxValue, y1 = float.MaxValue;
//            foreach (var c in squarePolygon)
//            {
//                if (c.X < x1)
//                    x1 = (float)c.X;
//
//                if (c.X > x2)
//                    x2 = (float)c.X;
//
//                if (c.Y < y1)
//                    y1 = (float)c.Y;
//
//                if (c.Y > y2)
//                    y2 = (float)c.Y;
//            };
//
//            return new Coords(x1 + RandomUtil.RandomDouble() * (x2 - x1), y1 + RandomUtil.RandomDouble() * (y2 - y1));
//        }

        public static PulseVector2 GetRandomPointOnSquare(PulseVector2[] polygon)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

            foreach (var p in polygon)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            return new PulseVector2(minX + RandomUtil.RandomDouble() * (maxX - minX), minY + RandomUtil.RandomDouble() * (maxY - minY));
        }

        public static PulseVector2 GetRandomPointOnSquare(PulseVector2[] polygon, Random r)
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

            foreach (var p in polygon)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }

            return new PulseVector2(minX + r.NextDouble() * (maxX - minX), minY + r.NextDouble() * (maxY - minY));
        }

        public static PulseVector2 GetRandomPointOnPolygon(PulseVector2[] polygon, Random r)
        {
            while (true)
            {
                var curP = GetRandomPointOnSquare(polygon, r);
                if (IsPointInPolygon(curP, polygon))
                    return curP;
            }
        }

        public static PulseVector2 GetRandomPointOnPolygon(PulseVector2[] polygon)
        {
            while (true)
            {
                var curP = GetRandomPointOnSquare(polygon);
                if (IsPointInPolygon(curP, polygon))
                    return curP;
            }
        }
    }
}
