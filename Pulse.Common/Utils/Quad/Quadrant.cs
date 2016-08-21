using System;
using System.Collections.Generic;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Utils.Quad
{
    public class Quadrant<T>
    {
        public AbstractQuadTree<T> Tree { get; set; }
        public Quadrant<T> Parent { set; get; }
        public Quadrant<T> TopLeft { set; get; }
        public Quadrant<T> TopRight { set; get; }
        public Quadrant<T> BottomLeft { set; get; }
        public Quadrant<T> BottomRight { set; get; }
        public Quadrant<T>[] Neighbors { get; set; }

        public Quadrant<T>[] Children
        {
            get { return new[] {BottomLeft, BottomRight, TopLeft, TopRight}; }
        }

        public PulseVector2 BottomLeftCorner { set; get; }
        public PulseVector2 TopRightCorner { set; get; }
        public PulseVector2 Center { get { return new PulseVector2(BottomLeftCorner.X + Size/2, BottomLeftCorner.Y + Size / 2);} }
        public double Size { get { return TopRightCorner.X - BottomLeftCorner.X; } }
        public int Level { set; get; }

        public Quadrants[] GlobalPosition { set; get; }
        public Quadrants LocalPosition { set; get; }
        public long GlobalPositionNum { get; set; }
        public int LocalPositionNum { get; set; }

        public long GlobalPositionNumCalculated
        {
            get
            {
                var res = 0L;
                for (int i = 0; i < GlobalPosition.Length; i++)
                {
                    res = res * 10 + (int)GlobalPosition[i];
                }
                return res;
            }
        }

        public int LocalPositionNumCalculated
        {
            get
            {
                return (int)LocalPosition;
            }
        }

        public T Data { set; get; }

        public Quadrant() { }

        public void SplitQuadrant()
        {
            //
            //  3 4
            //  1 2
            //
            //  level 0 - Root
            //  
            //  position example: 13 (level 1: bottom left -> level 2: top left)
            //  Root -> nodes (1->3)
            //

            if (Size /2 < Tree.MinSize)
                throw new Exception("Quadrant<T> can not be split: min size limit");

            if (BottomLeft == null)
            {
                BottomLeft = new Quadrant<T>();
                BottomLeft.Parent = this;
                BottomLeft.BottomLeftCorner = new PulseVector2(BottomLeftCorner.X, BottomLeftCorner.Y);
                BottomLeft.TopRightCorner = new PulseVector2(BottomLeftCorner.X + Size / 2, BottomLeftCorner.Y + Size / 2);
                BottomLeft.Level = Level + 1;
                BottomLeft.Tree = Tree;

                //this.BottomLeft.LocalPosition = 1;
                //this.BottomLeft.GlobalPosition = this.GlobalPosition * 10 + 1;

                BottomLeft.LocalPosition = Quadrants.BottomLeft;
                BottomLeft.GlobalPosition = new Quadrants[GlobalPosition.Length + 1];
                Array.Copy(GlobalPosition, BottomLeft.GlobalPosition, GlobalPosition.Length);
                BottomLeft.GlobalPosition[BottomLeft.GlobalPosition.Length - 1] = BottomLeft.LocalPosition;
                BottomLeft.LocalPositionNum = (int)Quadrants.BottomLeft;
                BottomLeft.GlobalPositionNum = GlobalPositionNum * 10 + BottomLeft.LocalPositionNum;
            }

            if (BottomRight == null)
            {
                BottomRight = new Quadrant<T>();
                BottomRight.Parent = this;
                BottomRight.BottomLeftCorner = new PulseVector2(BottomLeftCorner.X + Size / 2, BottomLeftCorner.Y);
                BottomRight.TopRightCorner = new PulseVector2(TopRightCorner.X, BottomLeftCorner.Y + Size / 2);
                BottomRight.Level = Level + 1;
                BottomRight.Tree = Tree;

                //this.BottomRight.LocalPosition = 2;
                //this.BottomRight.GlobalPosition = this.GlobalPosition * 10 + 2;

                BottomRight.LocalPosition = Quadrants.BottomRight;
                BottomRight.GlobalPosition = new Quadrants[GlobalPosition.Length + 1];
                Array.Copy(GlobalPosition, BottomRight.GlobalPosition, GlobalPosition.Length);
                BottomRight.GlobalPosition[BottomRight.GlobalPosition.Length - 1] = BottomRight.LocalPosition;
                BottomRight.LocalPositionNum = (int)Quadrants.BottomRight;
                BottomRight.GlobalPositionNum = GlobalPositionNum * 10 + BottomRight.LocalPositionNum;
            }

            if (TopLeft == null)
            {
                TopLeft = new Quadrant<T>();
                TopLeft.Parent = this;
                TopLeft.BottomLeftCorner = new PulseVector2(BottomLeftCorner.X, BottomLeftCorner.Y + Size / 2);
                TopLeft.TopRightCorner = new PulseVector2(BottomLeftCorner.X + Size / 2, TopRightCorner.Y);
                TopLeft.Level = Level + 1;
                TopLeft.Tree = Tree;

                //this.TopLeft.LocalPosition = 3;
                //this.TopLeft.GlobalPosition = this.GlobalPosition * 10 + 3;

                TopLeft.LocalPosition = Quadrants.TopLeft;
                TopLeft.GlobalPosition = new Quadrants[GlobalPosition.Length + 1];
                Array.Copy(GlobalPosition, TopLeft.GlobalPosition, GlobalPosition.Length);
                TopLeft.GlobalPosition[TopLeft.GlobalPosition.Length - 1] = TopLeft.LocalPosition;
                TopLeft.LocalPositionNum = (int)Quadrants.TopLeft;
                TopLeft.GlobalPositionNum = GlobalPositionNum * 10 + TopLeft.LocalPositionNum;
            }

            if (TopRight == null) 
            {
                TopRight = new Quadrant<T>();
                TopRight.Parent = this;
                TopRight.BottomLeftCorner = new PulseVector2(BottomLeftCorner.X + Size / 2, BottomLeftCorner.Y + Size / 2);
                TopRight.TopRightCorner = new PulseVector2(TopRightCorner.X, TopRightCorner.Y);
                TopRight.Level = Level + 1;
                TopRight.Tree = Tree;

                //this.TopRight.LocalPosition = 4;
                //this.TopRight.GlobalPosition = this.GlobalPosition * 10 + 4;

                TopRight.LocalPosition = Quadrants.TopRight;
                TopRight.GlobalPosition = new Quadrants[GlobalPosition.Length + 1];
                Array.Copy(GlobalPosition, TopRight.GlobalPosition, GlobalPosition.Length);
                TopRight.GlobalPosition[TopRight.GlobalPosition.Length - 1] = TopRight.LocalPosition;
                TopRight.LocalPositionNum = (int)Quadrants.TopRight;
                TopRight.GlobalPositionNum = GlobalPositionNum * 10 + TopRight.LocalPositionNum;
            }
        }

        public void GlueQuadrant()
        {
            TopLeft = null;
            TopRight = null;
            BottomLeft = null;
            BottomRight = null;
        }

        public PulseVector2[] GetPolygon()
        {
            var polygon = new PulseVector2[5];

            polygon[0] = BottomLeftCorner;
            polygon[1] = new PulseVector2(BottomLeftCorner.X, TopRightCorner.Y);
            polygon[2] = TopRightCorner;
            polygon[3] = new PulseVector2(TopRightCorner.X, BottomLeftCorner.Y);
            polygon[4] = BottomLeftCorner;

            return polygon;
        }

        public IList<Quadrant<T>> GetBorders(Directions direction)
        {
            IList<Quadrant<T>> borders;

            if (BottomLeft == null || BottomRight == null || TopLeft == null || TopRight == null)
                return new List<Quadrant<T>>() { this };

            switch (direction)
            {
                case Directions.L:
                    borders = BottomLeft.GetBorders(direction);
                    ((List<Quadrant<T>>)borders).AddRange(TopLeft.GetBorders(direction));
                    return borders;

                case Directions.R:
                    borders = BottomRight.GetBorders(direction);
                    ((List<Quadrant<T>>)borders).AddRange(TopRight.GetBorders(direction));
                    return borders;

                case Directions.U:
                    borders = TopLeft.GetBorders(direction);
                    ((List<Quadrant<T>>)borders).AddRange(TopRight.GetBorders(direction));
                    return borders;

                case Directions.D:
                    borders = BottomLeft.GetBorders(direction);
                    ((List<Quadrant<T>>)borders).AddRange(BottomRight.GetBorders(direction));
                    return borders;



                case Directions.DL:
                    borders = BottomLeft.GetBorders(direction);
                    return borders;

                case Directions.DR:
                    borders = BottomRight.GetBorders(direction);
                    return borders;

                case Directions.UL:
                    borders = TopLeft.GetBorders(direction);
                    return borders;

                case Directions.UR:
                    borders = TopRight.GetBorders(direction);
                    return borders;

                default:
                    throw new Exception("GetBorders support only R L U D");
            }
        }

        public Quadrant<T> GetDirectChildBySubCode2(Quadrants subcode)
        {
            switch (subcode)
            {
                case Quadrants.BottomLeft:
                    if (BottomLeft == null)
                        return BottomLeft;
                    return BottomLeft;

                case Quadrants.BottomRight:
                    return BottomRight;

                case Quadrants.TopLeft:
                    return TopLeft;

                case Quadrants.TopRight:
                    return TopRight;

                default:
                    throw new Exception("Incorrect subcode");
            }
        }

        public Quadrant<T> GetDirectChildBySubCode(Quadrants subcode)
        {
            switch (subcode){
                case Quadrants.BottomLeft:
                    if (BottomLeft == null)
                        throw new Exception(GlobalPosition + ": BL subQuadrant<T> is null");
                    else
                        return BottomLeft;

                case Quadrants.BottomRight:
                    if (BottomRight == null)
                        throw new Exception(GlobalPosition + ": BR subQuadrant<T> is null");
                    else
                        return BottomRight;

                case Quadrants.TopLeft:
                    if (TopLeft == null)
                        throw new Exception(GlobalPosition + ": TL subQuadrant<T> is null");
                    else
                        return TopLeft;

                case Quadrants.TopRight:
                    if (TopRight == null)
                        throw new Exception(GlobalPosition + ": BL subQuadrant<T> is null");
                    else
                        return TopRight;

                default:
                    throw new Exception("Incorrect subcode");
            }            
        }

        public override string ToString()
        {
//            var t = GlobalPositionNum == 1411423313 ? "OLOLO" : GlobalPositionNum.ToString();
//            return t + (Neighbors != null ? ": " + Neighbors.Length.ToString() : ": 0");
            return GlobalPositionNum.ToString();
        }
    }
}