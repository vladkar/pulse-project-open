using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace City.Models
{
    public class Polygon
    {
        public int Id { get; set; }
        public List<DVector2> listPoint { get; set; }

        public bool IsLast;

        public Polygon(int id)
        {
            this.Id = id;
            listPoint = new List<DVector2>();
            IsLast = true;
        }

        private bool IsIntersectionListPoint(DVector2 mousePosition)
        {
            if (listPoint.Count <= 2) return false;

            var firstPoint = listPoint.Last();
            var secondPoint = mousePosition;
            for (int i = 1; i < listPoint.Count - 1; i++)
            {
                if (IsIntersection(firstPoint, secondPoint, listPoint[i - 1], listPoint[i]))
                    return true;
            }
            return false;
        }


        private bool IsIntersection(DVector2 start1, DVector2 end1, DVector2 start2, DVector2 end2)
        {
            DVector2 dir1 = end1 - start1;
            DVector2 dir2 = end2 - start2;

            //считаем уравнения прямых проходящих через отрезки
            double a1 = -dir1.Y;
            double b1 = +dir1.X;
            double d1 = -(a1 * start1.X + b1 * start1.Y);

            double a2 = -dir2.Y;
            double b2 = +dir2.X;
            double d2 = -(a2 * start2.X + b2 * start2.Y);

            //подставляем концы отрезков, для выяснения в каких полуплоскотях они
            double seg1_line2_start = a2 * start1.X + b2 * start1.Y + d2;
            double seg1_line2_end = a2 * end1.X + b2 * end1.Y + d2;

            double seg2_line1_start = a1 * start2.X + b1 * start2.Y + d1;
            double seg2_line1_end = a1 * end2.X + b1 * end2.Y + d1;

            //если концы одного отрезка имеют один знак, значит он в одной полуплоскости и пересечения нет.
            if (seg1_line2_start * seg1_line2_end >= 0 || seg2_line1_start * seg2_line1_end >= 0)
                return false;
            return true;
        }

        public bool IsPointInPolygon(DVector2 newPoint)
        {
            bool result = false;
            int j = listPoint.Count() - 1;
            for (int i = 0; i < listPoint.Count(); i++)
            {
                if (listPoint[i].Y < newPoint.Y && listPoint[j].Y >= newPoint.Y || listPoint[j].Y < newPoint.Y && listPoint[i].Y >= newPoint.Y)
                {
                    if (listPoint[i].X + (newPoint.Y - listPoint[i].Y) / (listPoint[j].Y - listPoint[i].Y) * (listPoint[j].X - listPoint[i].X) < newPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        public bool IsСontour(DVector2 mousePosition)
        {
            return listPoint.Count > 2 && (mousePosition - listPoint.First()).LengthSquared() < 0.00000000000001f && !IsIntersectionListPoint(mousePosition);
        }

        public void cancelLastPoint()
        {
            this.listPoint.RemoveAt(listPoint.Count-1);
        }

        public void Draw(SpriteLayer sb, Texture beamTexture, Vector2 mousePosition, GlobeCamera camera)
        {
            sb.Clear();
            if (IsLast)
            {
                for (int i = 1; i < listPoint.Count; i++)
                {
                    sb.DrawBeam(beamTexture, getScreenPosition(listPoint[i - 1], camera), getScreenPosition(listPoint[i], camera), Color.White, Color.White, 4);
                }
                DVector2 mousePositionSpherical;
                camera.ScreenToSpherical(mousePosition.X, mousePosition.Y, out mousePositionSpherical);
                Color currentColor = this.IsСontour(mousePositionSpherical) ? Color.Red : Color.White;
                if (listPoint.Count > 0)
                    sb.DrawBeam(beamTexture, getScreenPosition(listPoint.Last(), camera), mousePosition,
                        new Color(currentColor.ToVector3(), 0.8f), new Color(currentColor.ToVector3(), 0.8f), 4);

                if (listPoint.Count > 1)
                    sb.DrawBeam(beamTexture, getScreenPosition(listPoint.First(),camera), mousePosition,
                        new Color(currentColor.ToVector3(), 0.5f), new Color(currentColor.ToVector3(), 0.5f), 4);
            }

//            else
//            {
//                if (listPoint.Count > 2)
//                    sb.DrawBeam(beamTexture, getScreenPosition(listPoint.First(), camera), getScreenPosition(listPoint.Last(), camera), Color.White, Color.White, 4);
//            }
        }

        public void AddPoint(DVector2 mousePosition)
        {
            if (this.IsIntersectionListPoint(mousePosition)) return;
            listPoint.Add(mousePosition);
        }

        public Vector2 getScreenPosition(DVector2 point, GlobeCamera camera)
        {
            var cartesianCoor = GeoHelper.SphericalToCartesian(point, camera.EarthRadius);
            var screenPoint = camera.CartesianToScreen(cartesianCoor);
            return screenPoint;
        }
    }
}
