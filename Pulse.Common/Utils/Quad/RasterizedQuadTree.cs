using System;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Utils.Quad
{
    public class RasterizedQuadTree<T> : AbstractQuadTree<T>
    {
        public RasterizedQuadTree(PulseVector2 bottomLeftCorner, double size, double minSize = 0) : base(bottomLeftCorner, size, minSize)
        {
        }

        public RasterizedQuadTree(PulseVector2 bottomLeftCorner, double size, double minSize, double maxSize) : base(bottomLeftCorner, size, minSize, maxSize)
        {
        }

        public void AddItems(T[] items, Func<T, PulseVector2[]> getPolygon, Func<T, bool> quadrantSplitCondition, Func<T, bool> quadrantGlueCondition)
        {
            foreach (var item in items)
            {
                AddItem(item, Root, getPolygon, quadrantSplitCondition, quadrantGlueCondition);
            }
        }

        public void AddItem(T item, Quadrant<T> root, Func<T, PulseVector2[]> getPolygon, Func<T, bool> quadrantSplitCondition, Func<T, bool> quadrantGlueCondition)
        {
            AddItemRec(item, root, getPolygon, quadrantSplitCondition, quadrantGlueCondition);
        }

        private void AddItemRec(T item, Quadrant<T> quadrant, Func<T, PulseVector2[]> getPolygon, Func<T, bool> quadrantSplitCondition, Func<T, bool> quadrantGlueCondition)
        {
            var intersect = ClipperUtil.IsPolygonInPolygon(getPolygon(item), quadrant.GetPolygon());
            if (!intersect)
                intersect = ClipperUtil.IsIntersectPolygon(getPolygon(item), quadrant.GetPolygon());
//            if (!intersect)
//                intersect = ClipperUtil.IsPolygonInPolygon(quadrant.GetPolygon(), getPolygon(item));

            if (quadrantSplitCondition(item) && intersect)
            {
                if (quadrant.Size / 2 > MinSize)
                {
                    quadrant.SplitQuadrant();
                    AddItemRec(item, quadrant.BottomLeft, getPolygon, quadrantSplitCondition, quadrantGlueCondition);
                    AddItemRec(item, quadrant.BottomRight, getPolygon, quadrantSplitCondition, quadrantGlueCondition);
                    AddItemRec(item, quadrant.TopLeft, getPolygon, quadrantSplitCondition, quadrantGlueCondition);
                    AddItemRec(item, quadrant.TopRight, getPolygon, quadrantSplitCondition, quadrantGlueCondition);

                    if (quadrantGlueCondition(item))
                    { 
                        quadrant.GlueQuadrant();
                    }
                }
                else
                {
                    quadrant.Data = item;
                }
            }
        } 
    }
}
