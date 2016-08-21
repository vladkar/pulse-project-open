using System.Linq;
using MultiagentEngine.Map;

namespace VirtualSociety.Model.Tree
{
    public class ClipperUtil
    {
        public static bool IsAtLeastOnePointInAnother(Coords[] polygon1, Coords[] polygon2)
        {
            var pinp = polygon1.Select(point => IsPointInPolygon(point, polygon2)).Any(pointRes => pointRes);
            if (!pinp)
                pinp = polygon2.Select(point => IsPointInPolygon(point, polygon1)).Any(pointRes => pointRes);

            return pinp;
        }

        public static bool IsPolygonInPolygon(Coords[] polygon1, Coords[] polygon2)
        {
            return polygon1.Select(point => IsPointInPolygon(point, polygon2)).Aggregate(true, (current, pointRes) => current & pointRes);
        }

        public static bool IsPointInPolygon(Coords point, Coords[] polygon)
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

        public static bool IsIntersectPolygon(Coords[] polygon1, Coords[] polygon2)
        {
            for (int i = 0; i < polygon1.Length - 1; i++)
                for (int j = 0; j < polygon2.Length - 1; j++)
                {
                    var line1 = new Coords[]{polygon1[i], polygon1[i+1]};
                    var line2 = new Coords[]{polygon2[j], polygon2[j+1]};
                    if (IsIntersectSegment(line1, line2))
                        return true;
                }            
            return false;
        }

        public static bool IsIntersectSegment(Coords[] line1, Coords[] line2)
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

            return false;
        }

        public static Coords IntersectSegments(Coords[] line1, Coords[] line2)
        {
            var a0 = line1[1].Y - line1[0].Y;
            var b0 = line1[0].X - line1[1].X;
            var c0 = a0*line1[0].X + b0*line1[0].Y;

            var a1 = line2[1].Y - line2[0].Y;
            var b1 = line2[0].X - line2[1].X;
            var c1 = a1*line2[0].X + b1*line2[0].Y;

            var delta = a0*b1 - a1*b0;
            if (delta == 0)
                return null;

            var x = (b1*c0 - b0*c1) / delta;
            var y = (a0*c1 - a1*c0) / delta;
            var interPoint = new Coords(x, y);

            return interPoint;
        }
    }
}
