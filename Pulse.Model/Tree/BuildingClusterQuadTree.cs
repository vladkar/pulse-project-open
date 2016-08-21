using System.Collections.Generic;
using System.Linq;
using MultiagentEngine.Map;

namespace VirtualSociety.Model.Tree
{
    public class BuildingClusterQuadTree : PolygonalQuadTree
    {
        public IList<VsBuilding> PolygonObjects { get; set; }
        //Func<T, Coords[]> GetPolygonFunc { get; set; }

        public IDictionary<VsBuilding, IList<BuildingClusterQuadrant>> PolygonClusters { get; set; }

        public BuildingClusterQuadTree(Coords min, double size, double minSize = 0)
            : base(min, size, minSize)
        {
        }

        public BuildingClusterQuadTree(Coords min, double size, double minSize, double maxSize)
            : base(min, size, minSize, maxSize)
        {
        }

        public override Quadrant AdequateQuadrantFactoryMethod()
        {
            return new BuildingClusterQuadrant();
        }

        public void AddObstacles(IList<VsBuilding> polygonObjects)
        {
            var count = 0;
            foreach (var o in polygonObjects)
            {
                count++;
                System.Console.WriteLine("Buildings add to quadtree: {0}/{1}", count, polygonObjects.Count);
                AddObstacle(o, Root);
            }
        }

        public void AddObstacle(VsBuilding building, Quadrant root)
        {
            var insc = new ClipperUtil();
            AddObstacleRec(building, root);
        }

        private void AddObstacleRec(VsBuilding building, Quadrant quadrant)
        {
            var intersect = ClipperUtil.IsAtLeastOnePointInAnother(building.MapPolygon, quadrant.GetPolygon());
            if (!intersect)
                intersect = ClipperUtil.IsIntersectPolygon(building.MapPolygon, quadrant.GetPolygon());
            if (!((BuildingClusterQuadrant)quadrant).IsObstacle && intersect)
            {
                if (quadrant.Size / 2 > MinSize)
                {
                    quadrant.SplitQuadrant();
                    AddObstacleRec(building, quadrant.BottomLeft);
                    AddObstacleRec(building, quadrant.BottomRight);
                    AddObstacleRec(building, quadrant.TopLeft);
                    AddObstacleRec(building, quadrant.TopRight);

                    //be removed from cluster if glue
                    var tempChildren = quadrant.Children;
                    
                    if (((BuildingClusterQuadrant)quadrant.BottomLeft).IsObstacle &&
                        ((BuildingClusterQuadrant)quadrant.BottomRight).IsObstacle &&
                        ((BuildingClusterQuadrant)quadrant.TopLeft).IsObstacle &&
                        ((BuildingClusterQuadrant)quadrant.TopRight).IsObstacle)
                    {
                        quadrant.GlueQuadrant();
                        ((BuildingClusterQuadrant)quadrant).IsObstacle = true;

                        foreach (var child in tempChildren)
                        {
                            if (child != null)
                                foreach (var b in ((BuildingClusterQuadrant) child).Buildings)
                                    if (b.QuadrantCluster.Contains(child))
                                        b.QuadrantCluster.Remove((BuildingClusterQuadrant)child);

                        }
                        building.QuadrantCluster.Add((BuildingClusterQuadrant) quadrant);
                        ((BuildingClusterQuadrant)quadrant).Buildings.Add(building);
                    }
                }
                else
                {
                    ((BuildingClusterQuadrant)quadrant).IsObstacle = true;
                    ((BuildingClusterQuadrant)quadrant).Buildings.Add(building);
                    building.QuadrantCluster.Add((BuildingClusterQuadrant)quadrant);
                }
            }
        }
    }
}
