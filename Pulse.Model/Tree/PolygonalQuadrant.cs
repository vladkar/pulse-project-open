using VirtualSociety.Model.Path;

namespace VirtualSociety.Model.Tree
{
    public class PolygonalQuadrant : Quadrant
    {
        public bool IsObstacle { set; get; }

//        public ObstacleType Obstacle { set; get; }

        public PolygonalQuadrant()
        {
            IsObstacle = false;
            Data = new AstarQuadTreeNodeData();
        }
    }

//    public enum ObstacleType
//    {
//        Clear = 0,
//        Obstacle = 1,
//        Water = 2
//    }
}
