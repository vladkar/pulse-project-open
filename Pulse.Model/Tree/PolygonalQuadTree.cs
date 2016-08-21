using System.Collections.Generic;
using MultiagentEngine.Map;
using VirtualSociety.Model.Path;

namespace VirtualSociety.Model.Tree
{
    public class PolygonalQuadTree : QuadTree
    {
        public INavigator Navigator { get; set; }

        public PolygonalQuadTree(Coords min, double size, double minSize = 0) : base(min, size, minSize)
        {
            Navigator = new AstarNavigator(this);
        }

        public PolygonalQuadTree(Coords min, double size, double minSize, double maxSize) : this(min, size, minSize)
        {
            MaxSize = maxSize;
        }

        public override Quadrant AdequateQuadrantFactoryMethod()
        {
            return new PolygonalQuadrant();
        }

        public IList<Coords[]> GetPolygons()
        {
            return GetQuadrantPolygonsRec(Root);
        }

        private List<Coords[]> GetQuadrantPolygonsRec(Quadrant quadrant)
        {
            if (quadrant == null)
                return null;

            var res = new List<Coords[]>[4];

            res[0] = GetQuadrantPolygonsRec(quadrant.BottomLeft);
            res[1] = GetQuadrantPolygonsRec(quadrant.BottomRight);
            res[2] = GetQuadrantPolygonsRec(quadrant.TopLeft);
            res[3] = GetQuadrantPolygonsRec(quadrant.TopRight);

            var result = new List<Coords[]>();
            if (((PolygonalQuadrant)quadrant).IsObstacle)
                result.Add(quadrant.GetPolygon());

            foreach (var subQuadrantPolygons in res)
            {
                if (subQuadrantPolygons != null)
                    result.AddRange(subQuadrantPolygons);
            }

            return result;
        }
        
        public void AddObstacles(IList<Coords[]> obstacles)
        {
            var count = 0;
            foreach (var obstacle in obstacles)
            {
                count ++;
                System.Console.WriteLine("{0}/{1}", count, obstacles.Count);
                AddObstacle(obstacle, Root);
            }
        }

        public void AddObstacle(Coords[] obstacle, Quadrant root)
        {
            AddObstacleRec(obstacle, root);
        }

        private void AddObstacleRec(Coords[] obstacle, Quadrant quadrant)
        {
//            var contains1 = insc.IsPolygonInPolygon(obstacle, quadrant.GetPolygon());
//            var contains2 = insc.IsPolygonInPolygon(quadrant.GetPolygon(), obstacle);
//            var intersect = insc.IsIntersectPolygon(obstacle, quadrant.GetPolygon());
//            if (!((PolygonalQuadrant)quadrant).isObstacle && (intersect | contains1 | contains2))

//            var intersect = insc.IsPolygonInPolygon(obstacle, quadrant.GetPolygon());
//            intersect = intersect || insc.IsPolygonInPolygon(quadrant.GetPolygon(), obstacle);
//            intersect = intersect || insc.IsIntersectPolygon(obstacle, quadrant.GetPolygon());

            var intersect = ClipperUtil.IsAtLeastOnePointInAnother(obstacle, quadrant.GetPolygon());
            if (!intersect)
                intersect = ClipperUtil.IsIntersectPolygon(obstacle, quadrant.GetPolygon());
            if (!((PolygonalQuadrant)quadrant).IsObstacle && intersect)
            {
                if (quadrant.Size / 2 > MinSize)
                {

                    quadrant.SplitQuadrant();
                    AddObstacleRec(obstacle, quadrant.BottomLeft);
                    AddObstacleRec(obstacle, quadrant.BottomRight);
                    AddObstacleRec(obstacle, quadrant.TopLeft);
                    AddObstacleRec(obstacle, quadrant.TopRight);

                    if (((PolygonalQuadrant)quadrant.BottomLeft).IsObstacle &&
                        ((PolygonalQuadrant)quadrant.BottomRight).IsObstacle &&
                        ((PolygonalQuadrant)quadrant.TopLeft).IsObstacle &&
                        ((PolygonalQuadrant)quadrant.TopRight).IsObstacle)
                    {
                        quadrant.GlueQuadrant();
                        ((PolygonalQuadrant)quadrant).IsObstacle = true;
                    }
                }
                else
                {
                    ((PolygonalQuadrant)quadrant).IsObstacle = true;
                }
            }
        }
    }
}
