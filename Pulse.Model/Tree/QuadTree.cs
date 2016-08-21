using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiagentEngine.Map;
using MultiagentEngine.Utils;

namespace VirtualSociety.Model.Tree
{
    public enum Directions { H = 0, U = 1, R = 2, D = 3, L = 4, UR = 12, DR = 32, UL = 14, DL = 34 };
    public enum Quadrants { Root = 0, BottomLeft = 1, BottomRight = 2, TopLeft = 3, TopRight = 4 };

    public class QuadTree
    {
        public Quadrant Root { set; get; }
        public double MinSize { set; get; }
        public double MaxSize { set; get; }

        private readonly Dictionary<Quadrants, Dictionary<Directions, Pair<Quadrants, Directions>>> _directionTable;

        public QuadTree(Coords min, double size, double minSize=0)
        {
            _directionTable = BuildNeighborFSM();

            Root = AdequateQuadrantFactoryMethod();
            Root.Min = min;
            Root.Max = new Coords(min.X + size, min.Y + size);
            Root.Level = 0;
            Root.Tree = this;

            Root.GlobalPosition = new Quadrants[0];

            MinSize = minSize;
        }

        public QuadTree(Coords min, double size, double minSize, double maxSize) : this(min, size, minSize)
        {
            MaxSize = maxSize;
            SplitToMaxSize(Root);
        }

        public virtual Quadrant AdequateQuadrantFactoryMethod()
        {
            return new Quadrant();
        }

        private void SplitToMaxSize(Quadrant quadrant)
        {
            if (quadrant.Size <= MaxSize) return;
            quadrant.SplitQuadrant();
            SplitToMaxSize(quadrant.BottomLeft);
            SplitToMaxSize(quadrant.BottomRight);
            SplitToMaxSize(quadrant.TopLeft);
            SplitToMaxSize(quadrant.TopRight);
        }

        //TODO private
        public Quadrant FindQuadrantByCode(int code)
        {
            Quadrant current = Root;

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

        public Quadrant FindQuadrantByCode2(Quadrants[] code)
        {
            var current = Root;
            Quadrant temp = null;

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

        public Quadrant FindQuadrantByCode(Quadrants[] code)
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
            CalculateQuadrantNeighbors(Root);
        }

        public void CalculateQuadrantNeighbors(Quadrant quadrant)
        {
            if (quadrant == null)
                return;

            quadrant.Neighbors = quadrant.Tree.GetNeighbors(quadrant).ToArray();

            CalculateQuadrantNeighbors(quadrant.BottomLeft);
            CalculateQuadrantNeighbors(quadrant.BottomRight);
            CalculateQuadrantNeighbors(quadrant.TopLeft);
            CalculateQuadrantNeighbors(quadrant.TopRight);
        }


        public static int thr = 0;
        public static List<Task> tasks = new List<Task>();
        public void CalculateQuadrantNeighbors2(Quadrant quadrant)
        {
            if (quadrant == null)
                return;

            quadrant.Neighbors = quadrant.Tree.GetNeighbors(quadrant).ToArray();


            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.BottomLeft);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.BottomRight);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.TopLeft);
            CalculateQuadrantNeighborsParallelProxy(CalculateQuadrantNeighbors, quadrant.TopRight);
        }

        private void CalculateQuadrantNeighborsParallelProxy(Action<Quadrant> calculateQuadrantNeighbors, Quadrant quadrant)
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
        public IList<Quadrant> GetNeighbors(Quadrant quadrant)
        {
            //var code = IntToDigitArray(quadrant.GlobalPosition);
            var neighbors = new List<Quadrant>();

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
                var tolerance = 0.00001;
                var dir = codedNeighbor.First;
                var leftBound = Math.Abs(quadrant.Min.X - Root.Min.X) < tolerance;
                var rightBound = Math.Abs(quadrant.Max.X - Root.Max.X) < tolerance;
                var bottomBound = Math.Abs(quadrant.Min.Y - Root.Min.Y) < tolerance;
                var upBound = Math.Abs(quadrant.Max.Y - Root.Max.Y) < tolerance;

                var l_null = leftBound & (dir == Directions.L || dir == Directions.DL || dir == Directions.UL);
                var r_null = rightBound & (dir == Directions.R || dir == Directions.DR || dir == Directions.UR);
                var d_null = bottomBound & (dir == Directions.D || dir == Directions.DL || dir == Directions.DR);
                var u_null = upBound & (dir == Directions.U || dir == Directions.UL || dir == Directions.UR);

                Quadrant realQuadrant;
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
            //  quadrants:
            //  3 4
            //  1 2
            //
            //  directions
            //  0 - halt (neighbor quadrant in same Parent quadrant)
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

        public Quadrant GetNode(Coords coords)
        {
            if (coords.X >= Root.Min.X & coords.X < Root.Max.X & coords.Y >= Root.Min.Y & coords.Y < Root.Max.Y)
            {
                return GetNodeRec(coords, Root);
            }
            throw new ArgumentOutOfRangeException("The point is out of tree bounds");
        }

        public Quadrant GetNodeRec(Coords coords, Quadrant quadrant)
        {
            if (coords.X < quadrant.Min.X + quadrant.Size / 2)
                if (coords.Y < quadrant.Min.Y + quadrant.Size / 2)
                    return (quadrant.BottomLeft != null) ? GetNodeRec(coords, quadrant.BottomLeft) : quadrant;
                else
                    return (quadrant.TopLeft != null) ? GetNodeRec(coords, quadrant.TopLeft) : quadrant;    
            else
                if (coords.Y < quadrant.Min.Y + quadrant.Size / 2)
                    return (quadrant.BottomRight != null) ? GetNodeRec(coords, quadrant.BottomRight) : quadrant;
                else
                    return (quadrant.TopRight != null) ? GetNodeRec(coords, quadrant.TopRight) : quadrant;
        }

        public double GetDistanceRough(Quadrant quadrant1, Quadrant quadrant2)
        {
            var p1 = new Coords(quadrant1.Min.X + quadrant1.Size / 2, quadrant1.Min.Y + quadrant1.Size / 2);
            var p2 = new Coords(quadrant2.Min.X + quadrant2.Size / 2, quadrant2.Min.Y + quadrant2.Size / 2);
            return p1.DistanceTo(p2);
        }
    }

    public interface IQuadrantData
    {
        Quadrant Quadrant { set; get; }
    }
}
