using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulse.MultiagentEngine.Map;
using Pulse.MultiagentEngine.Utils;

namespace Pulse.Common.Utils.Quad
{
    public enum Directions { H = 0, U = 1, R = 2, D = 3, L = 4, UR = 12, DR = 32, UL = 14, DL = 34 };
    public enum Quadrants { Root = 0, BottomLeft = 1, BottomRight = 2, TopLeft = 3, TopRight = 4 };

    public abstract class AbstractQuadTree<T>
    {
        private readonly Dictionary<Quadrants, Dictionary<Directions, Pair<Quadrants, Directions>>> _directionTable;

        public Quadrant<T> Root { set; get; }
        public double MinSize { set; get; }
        public double MaxSize { set; get; }

        public IEnumerable<Quadrant<T>> QuadrantsChildless { get { return GetChildlessQuadrants(); } }

        #region GetChildlessQuadrants

        private IEnumerable<Quadrant<T>> GetChildlessQuadrants()
        {
            var childlessQuadrants = GetChildlessQuadrantsMemoryOptimizedRec(Root, 4);
            return childlessQuadrants;
        }

        private C5.LinkedList<Quadrant<T>> GetChildlessQuadrantsMemoryOptimizedRec(Quadrant<T> quadrant, int optimizationDepth)
        {
            if (optimizationDepth > 0)
            {
                optimizationDepth--;
                var bl = GetChildlessQuadrantsMemoryOptimizedRec(quadrant.BottomLeft, optimizationDepth);
                var br = GetChildlessQuadrantsMemoryOptimizedRec(quadrant.BottomRight, optimizationDepth);
                var tl = GetChildlessQuadrantsMemoryOptimizedRec(quadrant.TopLeft, optimizationDepth);
                var tr = GetChildlessQuadrantsMemoryOptimizedRec(quadrant.TopRight, optimizationDepth);

                bl.AddAll(br);
                bl.AddAll(tl);
                bl.AddAll(tr);

                return bl;
            }
            else
            {
                var bl = GetChildlessQuadrantsRec(quadrant.BottomLeft);
                var br = GetChildlessQuadrantsRec(quadrant.BottomRight);
                var tl = GetChildlessQuadrantsRec(quadrant.TopLeft);
                var tr = GetChildlessQuadrantsRec(quadrant.TopRight);

                bl.AddAll(br);
                bl.AddAll(tl);
                bl.AddAll(tr);

                return bl;
            }
        }

        private C5.LinkedList<Quadrant<T>> GetChildlessQuadrantsRec(Quadrant<T> quadrant)
        {
            if (quadrant.Children.All(c => c == null))
            {
                return new C5.LinkedList<Quadrant<T>> {quadrant};
            }
            else
            {
                var bl = GetChildlessQuadrantsRec(quadrant.BottomLeft);
                var br = GetChildlessQuadrantsRec(quadrant.BottomRight);
                var tl = GetChildlessQuadrantsRec(quadrant.TopLeft);
                var tr = GetChildlessQuadrantsRec(quadrant.TopRight);

                bl.AddAll(br);
                bl.AddAll(tl);
                bl.AddAll(tr);

                return bl;
            }
        }

        #endregion
        
        protected AbstractQuadTree(PulseVector2 bottomLeftCorner, double size, double minSize=0)
        {
            _directionTable = BuildNeighborFSM();

            Root = new Quadrant<T>();
            Root.BottomLeftCorner = bottomLeftCorner;
            Root.TopRightCorner = new PulseVector2(bottomLeftCorner.X + size, bottomLeftCorner.Y + size);
            Root.Level = 0;
            Root.Tree = this;

            Root.GlobalPosition = new Quadrants[0];

            MinSize = minSize;
        }

        protected AbstractQuadTree(PulseVector2 bottomLeftCorner, double size, double minSize, double maxSize)
            : this(bottomLeftCorner, size, minSize)
        {
            MaxSize = maxSize;
            SplitToMaxSize(Root);
        }

        private void SplitToMaxSize(Quadrant<T> quadrant)
        {
            if (quadrant.Size <= MaxSize) return;
            quadrant.SplitQuadrant();
            SplitToMaxSize(quadrant.BottomLeft);
            SplitToMaxSize(quadrant.BottomRight);
            SplitToMaxSize(quadrant.TopLeft);
            SplitToMaxSize(quadrant.TopRight);
        }

        public Quadrant<T> FindQuadrantByCode(int code)
        {
            Quadrant<T> current = Root;

            if (code != 0)
            {
                var arrCode = IntToDigitArray(code);                
                for (int i = 0; i < arrCode.Length; i++)
                {
                    current = current.GetDirectChildBySubCode((Quadrants)arrCode[i]);
                }
            }

            return current;
        }

        private Directions InverseDirection(Directions direction)
        {
            switch (direction)
            {
                case Directions.D:
                    return Directions.U;
                case Directions.U:
                    return Directions.D;
                case Directions.R:
                    return Directions.L;
                case Directions.L:
                    return Directions.R;

                case Directions.DL:
                    return Directions.UR;
                case Directions.DR:
                    return Directions.UL;
                case Directions.UL:
                    return Directions.DR;
                case Directions.UR:
                    return Directions.DL;

                default:
                    throw new Exception("InverseDirection support only R L U D DL DR UL UR");
            }
        }

        public Quadrant<T> FindQuadrantByCode2(Quadrants[] code)
        {
            var current = Root;
            Quadrant<T> temp = null;

            if (!(code.Length == 0 || code.Length == 1 & code[0] == Quadrants.Root))
            {
                for (int i = 0; i < code.Length; i++)
                {
                    temp = current.GetDirectChildBySubCode2(code[i]);
                    if (temp == null)
                        return current;
                    current = temp;
                }
            }
            return current;
        }

        public Quadrant<T> FindQuadrantByCode(Quadrants[] code)
        {
            var current = Root;

            if (code.Length == 0 || code.Length == 1 & code[0] == Quadrants.Root) return current;
            foreach (var t in code)
            {
                try
                {
                    current = current.GetDirectChildBySubCode(t);
                }
                catch (Exception e)
                {
                    return current;
                }
            }
            return current;
        }

        private int[] IntToDigitArray(int n)
        {
            if (n == 0) return new int[0];

            var digits = new List<int>();

            for (; n != 0; n /= 10)
                digits.Add(n % 10);

            var arr = digits.ToArray();
            Array.Reverse(arr);
            return arr;
        }

        public void CalculateNeighbors()
        {
            CalculateQuadrantNeighborsSimpleParallel(Root);
        }

        public void CalculateQuadrantNeighbors(Quadrant<T> quadrant)
        {
            if (quadrant == null)
                return;

            quadrant.Neighbors = quadrant.Tree.GetNeighbors(quadrant).ToArray();

            CalculateQuadrantNeighbors(quadrant.BottomLeft);
            CalculateQuadrantNeighbors(quadrant.BottomRight);
            CalculateQuadrantNeighbors(quadrant.TopLeft);
            CalculateQuadrantNeighbors(quadrant.TopRight);
        }

        public void CalculateQuadrantNeighborsSimpleParallel(Quadrant<T> quadrant)
        {
            if (quadrant == null)
                return;

            quadrant.Neighbors = quadrant.Tree.GetNeighbors(quadrant).ToArray();

            var list = new List<Quadrant<T>>
            {
                quadrant.BottomLeft,
                quadrant.BottomRight,
                quadrant.TopLeft,
                quadrant.TopRight
            };

            list.AsParallel().ForAll(CalculateQuadrantNeighbors);
        }


        public static int thr = 0;
        public static List<Task> tasks = new List<Task>();
        public void CalculateQuadrantNeighbors2(Quadrant<T> quadrant)
        {
            if (quadrant == null)
                return;

            quadrant.Neighbors = quadrant.Tree.GetNeighbors(quadrant).ToArray();


            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.BottomLeft);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.BottomRight);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.TopLeft);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.TopRight);
        }

        private void CalculateQuadrantNeighborsParallelProxy(Action<Quadrant<T>> calculateQuadrantNeighbors, Quadrant<T> quadrant)
        {
            if (thr >= 8)
                calculateQuadrantNeighbors(quadrant);
            else
            {
                Task[] task = {null};
                thr++;

                tasks.Add(task[0]);
                task[0] = new Task(() =>
                {
                    calculateQuadrantNeighbors(quadrant);
                    thr--;

                    lock (tasks)
                    {
                        tasks.Remove(task[0]);
                    }
                    
                });

                tasks.Add(task[0]);

                task[0].Start();
            }
        }


        // TODO Optimize: Quadrants enum in global code? (think)
        public IList<Quadrant<T>> GetNeighbors(Quadrant<T> quadrant)
        {
            //var code = IntToDigitArray(Quadrant<T>.GlobalPosition);
            var neighbors = new List<Quadrant<T>>();

            var codedNeighbors = new List<Pair<Directions, Quadrants[]>>();
            foreach (var direction in (Directions[])Enum.GetValues(typeof(Directions)))
                if (direction != 0)
                {
                    var neighbor = new Quadrants[quadrant.GlobalPosition.Length];
                    var dir = direction;
                    
                    quadrant.GlobalPosition.CopyTo(neighbor, 0);

                    for (int j = quadrant.GlobalPosition.Length - 1; j >= 0; j--)
                    {
                        var currentCode = quadrant.GlobalPosition[j];

                        var temp = _directionTable[currentCode][dir];

                        
                        neighbor[j] = temp.First;
                        dir = temp.Second;
                        if (dir == Directions.H)
                            break;
                    }

                    var currentNeighbor = new Pair<Directions, Quadrants[]>(direction, neighbor);
                    codedNeighbors.Add(currentNeighbor);
                }

            foreach (var codedNeighbor in codedNeighbors)
            {
                const double tolerance = 0.0000001;
                var dir = codedNeighbor.First;
                var leftBound = Math.Abs(quadrant.BottomLeftCorner.X - Root.BottomLeftCorner.X) < tolerance;
                var rightBound = Math.Abs(quadrant.TopRightCorner.X - Root.TopRightCorner.X) < tolerance;
                var bottomBound = Math.Abs(quadrant.BottomLeftCorner.Y - Root.BottomLeftCorner.Y) < tolerance;
                var upBound = Math.Abs(quadrant.TopRightCorner.Y - Root.TopRightCorner.Y) < tolerance;

                var l_null = leftBound & (dir == Directions.L || dir == Directions.DL || dir == Directions.UL);
                var r_null = rightBound & (dir == Directions.R || dir == Directions.DR || dir == Directions.UR);
                var d_null = bottomBound & (dir == Directions.D || dir == Directions.DL || dir == Directions.DR);
                var u_null = upBound & (dir == Directions.U || dir == Directions.UL || dir == Directions.UR);

                Quadrant<T> realQuadrant;
                if (l_null | r_null | d_null | u_null)
                    realQuadrant = null;
                else
                    realQuadrant = FindQuadrantByCode2(codedNeighbor.Second);

                try
                {
                    neighbors.AddRange(realQuadrant.GetBorders(InverseDirection(dir)));
                } catch (Exception e)
                {
                    var t = e;
                    //TODO
                }
            }

            return neighbors;
        }

        public Dictionary<Quadrants, Dictionary<Directions, Pair<Quadrants, Directions>>> BuildNeighborFSM()
        {
            //
            //  Quadrants:
            //  3 4
            //  1 2
            //
            //  directions
            //  0 - halt (neighbor Quadrant<T> in same Parent Quadrant<T>)
            //  1 - up
            //  2 - right
            //  3 - bottom
            //  4 - left
            //
            
            var directionTable = new Dictionary<Quadrants, Dictionary<Directions, Pair<Quadrants, Directions>>>();

            directionTable.Add(Quadrants.BottomLeft, new Dictionary<Directions,Pair<Quadrants,Directions>>());
            directionTable.Add(Quadrants.BottomRight, new Dictionary<Directions,Pair<Quadrants,Directions>>());
            directionTable.Add(Quadrants.TopRight, new Dictionary<Directions,Pair<Quadrants,Directions>>());
            directionTable.Add(Quadrants.TopLeft, new Dictionary<Directions, Pair<Quadrants, Directions>>());

            directionTable[Quadrants.BottomLeft].Add(Directions.U, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.H));
            directionTable[Quadrants.BottomLeft].Add(Directions.D, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.D));
            directionTable[Quadrants.BottomLeft].Add(Directions.L, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.L));
            directionTable[Quadrants.BottomLeft].Add(Directions.R, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.H));
            directionTable[Quadrants.BottomLeft].Add(Directions.UR, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.H));
            directionTable[Quadrants.BottomLeft].Add(Directions.UL, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.L));
            directionTable[Quadrants.BottomLeft].Add(Directions.DR, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.D));
            directionTable[Quadrants.BottomLeft].Add(Directions.DL, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.DL));

            directionTable[Quadrants.BottomRight].Add(Directions.U, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.H));
            directionTable[Quadrants.BottomRight].Add(Directions.D, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.D));
            directionTable[Quadrants.BottomRight].Add(Directions.L, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.H));
            directionTable[Quadrants.BottomRight].Add(Directions.R, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.R));
            directionTable[Quadrants.BottomRight].Add(Directions.UR, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.R));
            directionTable[Quadrants.BottomRight].Add(Directions.UL, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.H));
            directionTable[Quadrants.BottomRight].Add(Directions.DR, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.DR));
            directionTable[Quadrants.BottomRight].Add(Directions.DL, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.D));

            directionTable[Quadrants.TopLeft].Add(Directions.U, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.U));
            directionTable[Quadrants.TopLeft].Add(Directions.D, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.H));
            directionTable[Quadrants.TopLeft].Add(Directions.L, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.L));
            directionTable[Quadrants.TopLeft].Add(Directions.R, new Pair<Quadrants, Directions>(Quadrants.TopRight, Directions.H));
            directionTable[Quadrants.TopLeft].Add(Directions.UR, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.U));
            directionTable[Quadrants.TopLeft].Add(Directions.UL, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.UL));
            directionTable[Quadrants.TopLeft].Add(Directions.DR, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.H));
            directionTable[Quadrants.TopLeft].Add(Directions.DL, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.L));

            directionTable[Quadrants.TopRight].Add(Directions.U, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.U));
            directionTable[Quadrants.TopRight].Add(Directions.D, new Pair<Quadrants, Directions>(Quadrants.BottomRight, Directions.H));
            directionTable[Quadrants.TopRight].Add(Directions.L, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.H));
            directionTable[Quadrants.TopRight].Add(Directions.R, new Pair<Quadrants, Directions>(Quadrants.TopLeft, Directions.R));
            directionTable[Quadrants.TopRight].Add(Directions.UR, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.UR));
            directionTable[Quadrants.TopRight].Add(Directions.UL, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.U));
            directionTable[Quadrants.TopRight].Add(Directions.DR, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.R));
            directionTable[Quadrants.TopRight].Add(Directions.DL, new Pair<Quadrants, Directions>(Quadrants.BottomLeft, Directions.H));

            return directionTable;
        }

        public Quadrant<T> GetNode(PulseVector2 coords)
        {
            if (coords.X >= Root.BottomLeftCorner.X & coords.X < Root.TopRightCorner.X & coords.Y >= Root.BottomLeftCorner.Y & coords.Y < Root.TopRightCorner.Y)
            {
                return GetNodeRec(coords, Root);
            }
            throw new ArgumentOutOfRangeException("The point is out of tree bounds");
        }

        public Quadrant<T> GetNodeRec(PulseVector2 coords, Quadrant<T> quadrant)
        {
            if (coords.X < quadrant.BottomLeftCorner.X + quadrant.Size / 2)
                if (coords.Y < quadrant.BottomLeftCorner.Y + quadrant.Size / 2)
                    return (quadrant.BottomLeft != null) ? GetNodeRec(coords, quadrant.BottomLeft) : quadrant;
                else
                    return (quadrant.TopLeft != null) ? GetNodeRec(coords, quadrant.TopLeft) : quadrant;    
            else
                if (coords.Y < quadrant.BottomLeftCorner.Y + quadrant.Size / 2)
                    return (quadrant.BottomRight != null) ? GetNodeRec(coords, quadrant.BottomRight) : quadrant;
                else
                    return (quadrant.TopRight != null) ? GetNodeRec(coords, quadrant.TopRight) : quadrant;
        }

        public double GetDistanceRough(Quadrant<T> quadrant1, Quadrant<T> quadrant2)
        {
            var p1 = new PulseVector2(quadrant1.BottomLeftCorner.X + quadrant1.Size / 2, quadrant1.BottomLeftCorner.Y + quadrant1.Size / 2);
            var p2 = new PulseVector2(quadrant2.BottomLeftCorner.X + quadrant2.Size / 2, quadrant2.BottomLeftCorner.Y + quadrant2.Size / 2);
            return p1.DistanceTo(p2);
        }
    }
}
