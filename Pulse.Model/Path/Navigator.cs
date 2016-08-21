using System;
using System.Collections.Generic;
using System.Linq;
using MultiagentEngine.Map;
using VirtualSociety.Model.Tree;

namespace VirtualSociety.Model.Path
{
    public interface INavigator
    {
        IList<Coords> FindPath(Coords start, Coords end);
    }

    public class AstarNavigator : INavigator
    {
        private readonly PolygonalQuadTree _tree;

        public AstarNavigator(PolygonalQuadTree tree)
        {
            _tree = tree;
        }

        public IList<Coords> FindPath(Coords start, Coords end)
        {
            var startNode = (PolygonalQuadrant)_tree.GetNode(start);
            var endNode = (PolygonalQuadrant)_tree.GetNode(end);

            // TODO REMOVE
            if (endNode.IsObstacle || startNode.IsObstacle)
//                return null;
                throw new Exception("Start or goal is obstacle");

            
            //var openList2 = new TreeSet<AstarQuadTreeNodeData>(new AstarNodeComparer());
            var openList2 = new SortedSet<AstarQuadTreeNodeData>(new AstarNodeComparer());
            

            var closedList = new Dictionary<long, AstarQuadTreeNodeData>();

            var startNodeData = new AstarQuadTreeNodeData();
            startNodeData.G = 0;
            startNodeData.H = startNode.Center.DistanceTo(endNode.Center);
            startNodeData.Quadrant = startNode;

            openList2.Add(startNodeData);

            while (openList2.Count != 0)
            {
                //var x = openList2.FindMin();
                var x = openList2.Min;

                if (x.Quadrant == endNode)
                    return CompletePath(startNode, x, start, end);

                openList2.Remove(x);
                closedList.Add(x.Quadrant.GlobalPositionNum, x);

                var neighbors = x.Quadrant.Neighbors;
                foreach (var quadrant in neighbors)
                {
                    var yQuadrant = (PolygonalQuadrant)quadrant;
                    if (closedList.ContainsKey(yQuadrant.GlobalPositionNum) | yQuadrant.IsObstacle)
                        continue;

                    var tentativeG = x.G + x.Quadrant.Center.DistanceTo(yQuadrant.Center);
                    var tentativeOk = true;
                    AstarQuadTreeNodeData y = null;
                    //if (openList2.Where(yTemp => yTemp.Quadrant.GlobalPositionNum == yQuadrant.GlobalPositionNum).ToArray().Length == 0)

//                    var containsYs =
//                        openList2.FindAll(yTemp => yTemp.Quadrant.GlobalPositionNum == yQuadrant.GlobalPositionNum)
//                            .ToArray();
                    var containsYs =
                        openList2.Where(yTemp => yTemp.Quadrant.GlobalPositionNum == yQuadrant.GlobalPositionNum)
                            .ToArray();

                    y = containsYs.Length > 0 ? containsYs[0] : null;

                    if (y == null)
                    {
                        y = new AstarQuadTreeNodeData();
                        y.Quadrant = yQuadrant;
                        y.AstarParent = x;
                        y.G = y.Quadrant.Center.DistanceTo(startNode.Center);
                        y.H = y.Quadrant.Center.DistanceTo(endNode.Center);
                        openList2.Add(y);
                    }
                    else
                    {
                        if (!(tentativeG < y.G))
                        {
                            tentativeOk = false;
                        }
                    }

                    if (tentativeOk)
                    {
                        openList2.Remove(y);
                        y.AstarParent = x;
                        y.G = tentativeG;
                        y.H = y.Quadrant.Center.DistanceTo(endNode.Center);
                        openList2.Add(y);
                    }
                }
            }
            throw new Exception("Impossible path");
        }

        private IList<Coords> CompletePath(Quadrant startNode, AstarQuadTreeNodeData endNodeData, Coords start, Coords end)
        {
            //            var path = new List<Coords> {end};
            IList<Coords> path = new C5.LinkedList<Coords>();

            if (!end.Equals(endNodeData.Quadrant.Center))
                path.Add(end);


            var current = endNodeData;
            while (current != null)
            {
                path.Add(current.Quadrant.Center);

                current = current.AstarParent;

            }

            //path.Add(startNode.Center);
            if (!start.Equals(startNode.Center))
                path.Add(start);

            return path;
        }
    }

    public class AstarQuadTreeNodeData : IQuadrantData
    {
        public Quadrant Quadrant { get; set; }
        public AstarQuadTreeNodeData AstarParent { get; set; }
        public double H { get; set; }
        public double G { get; set; }
        public double F { get { return H + G; } }
    }

    public class AstarNodeComparer : IComparer<AstarQuadTreeNodeData>
    {
        public int Compare(AstarQuadTreeNodeData x, AstarQuadTreeNodeData y)
        {
            if (x.F > y.F)
                return 1;
            if (x.F < y.F)
                return -1;
            return 0;
        }
    }
}
