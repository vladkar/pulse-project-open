using System;
using System.Collections.Generic;
using MultiagentEngine.Map;

namespace VirtualSociety.Model.Tree
{
    public class Quadrant
    {
        public QuadTree Tree { get; set; }
        public Quadrant Parent { set; get; }
        public Quadrant TopLeft { set; get; }
        public Quadrant TopRight { set; get; }
        public Quadrant BottomLeft { set; get; }
        public Quadrant BottomRight { set; get; }
        public Quadrant[] Neighbors { get; set; }

        public Quadrant[] Children
        {
            get { return new[] {BottomLeft, BottomRight, TopLeft, TopRight}; }
        }

        public Coords Min { set; get; }
        public Coords Max { set; get; }
        public Coords Center { get { return new Coords(Min.X + Size/2, Min.Y + Size / 2);} }
        public double Size { get { return this.Max.X - this.Min.X; } }
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

        private IQuadrantData _data;
        public IQuadrantData Data 
        {
            set { _data = value; _data.Quadrant = this; } 
            get { return _data; } 
        }

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
                throw new Exception("Quadrant can not be split: min size limit");

            if (this.BottomLeft == null)
            {
                this.BottomLeft = Tree.AdequateQuadrantFactoryMethod();
                this.BottomLeft.Parent = this;
                this.BottomLeft.Min = new Coords(this.Min.X, this.Min.Y);
                this.BottomLeft.Max = new Coords(this.Min.X + this.Size / 2, this.Min.Y + this.Size / 2);
                this.BottomLeft.Level = Level + 1;
                this.BottomLeft.Tree = this.Tree;

                //this.BottomLeft.LocalPosition = 1;
                //this.BottomLeft.GlobalPosition = this.GlobalPosition * 10 + 1;

                this.BottomLeft.LocalPosition = Quadrants.BottomLeft;
                this.BottomLeft.GlobalPosition = new Quadrants[this.GlobalPosition.Length + 1];
                Array.Copy(this.GlobalPosition, this.BottomLeft.GlobalPosition, this.GlobalPosition.Length);
                this.BottomLeft.GlobalPosition[this.BottomLeft.GlobalPosition.Length - 1] = this.BottomLeft.LocalPosition;
                this.BottomLeft.LocalPositionNum = (int)Quadrants.BottomLeft;
                this.BottomLeft.GlobalPositionNum = this.GlobalPositionNum * 10 + this.BottomLeft.LocalPositionNum;
            }

            if (this.BottomRight == null)
            {
                this.BottomRight = Tree.AdequateQuadrantFactoryMethod();
                this.BottomRight.Parent = this;
                this.BottomRight.Min = new Coords(this.Min.X + this.Size / 2, this.Min.Y);
                this.BottomRight.Max = new Coords(this.Max.X, this.Min.Y + this.Size / 2);
                this.BottomRight.Level = Level + 1;
                this.BottomRight.Tree = this.Tree;

                //this.BottomRight.LocalPosition = 2;
                //this.BottomRight.GlobalPosition = this.GlobalPosition * 10 + 2;

                this.BottomRight.LocalPosition = Quadrants.BottomRight;
                this.BottomRight.GlobalPosition = new Quadrants[this.GlobalPosition.Length + 1];
                Array.Copy(this.GlobalPosition, this.BottomRight.GlobalPosition, this.GlobalPosition.Length);
                this.BottomRight.GlobalPosition[this.BottomRight.GlobalPosition.Length - 1] = this.BottomRight.LocalPosition;
                this.BottomRight.LocalPositionNum = (int)Quadrants.BottomRight;
                this.BottomRight.GlobalPositionNum = this.GlobalPositionNum * 10 + this.BottomRight.LocalPositionNum;
            }

            if (this.TopLeft == null)
            {
                this.TopLeft = Tree.AdequateQuadrantFactoryMethod();
                this.TopLeft.Parent = this;
                this.TopLeft.Min = new Coords(this.Min.X, this.Min.Y + this.Size / 2);
                this.TopLeft.Max = new Coords(this.Min.X + this.Size / 2, this.Max.Y);
                this.TopLeft.Level = Level + 1;
                this.TopLeft.Tree = this.Tree;

                //this.TopLeft.LocalPosition = 3;
                //this.TopLeft.GlobalPosition = this.GlobalPosition * 10 + 3;

                this.TopLeft.LocalPosition = Quadrants.TopLeft;
                this.TopLeft.GlobalPosition = new Quadrants[this.GlobalPosition.Length + 1];
                Array.Copy(this.GlobalPosition, this.TopLeft.GlobalPosition, this.GlobalPosition.Length);
                this.TopLeft.GlobalPosition[this.TopLeft.GlobalPosition.Length - 1] = this.TopLeft.LocalPosition;
                this.TopLeft.LocalPositionNum = (int)Quadrants.TopLeft;
                this.TopLeft.GlobalPositionNum = this.GlobalPositionNum * 10 + this.TopLeft.LocalPositionNum;
            }

            if (this.TopRight == null) 
            {
                this.TopRight = Tree.AdequateQuadrantFactoryMethod();
                this.TopRight.Parent = this;
                this.TopRight.Min = new Coords(this.Min.X + this.Size / 2, this.Min.Y + this.Size / 2);
                this.TopRight.Max = new Coords(this.Max.X, this.Max.Y);
                this.TopRight.Level = Level + 1;
                this.TopRight.Tree = this.Tree;

                //this.TopRight.LocalPosition = 4;
                //this.TopRight.GlobalPosition = this.GlobalPosition * 10 + 4;

                this.TopRight.LocalPosition = Quadrants.TopRight;
                this.TopRight.GlobalPosition = new Quadrants[this.GlobalPosition.Length + 1];
                Array.Copy(this.GlobalPosition, this.TopRight.GlobalPosition, this.GlobalPosition.Length);
                this.TopRight.GlobalPosition[this.TopRight.GlobalPosition.Length - 1] = this.TopRight.LocalPosition;
                this.TopRight.LocalPositionNum = (int)Quadrants.TopRight;
                this.TopRight.GlobalPositionNum = this.GlobalPositionNum * 10 + this.TopRight.LocalPositionNum;
            }
        }

        public void GlueQuadrant()
        {
            this.TopLeft = null;
            this.TopRight = null;
            this.BottomLeft = null;
            this.BottomRight = null;
        }

        public Coords[] GetPolygon()
        {
            var polygon = new Coords[5];

            polygon[0] = this.Min;
            polygon[1] = new Coords(this.Min.X, this.Max.Y);
            polygon[2] = this.Max;
            polygon[3] = new Coords(this.Max.X, this.Min.Y);
            polygon[4] = this.Min;

            return polygon;
        }

        public IList<Quadrant> GetBorders(Directions direction)
        {
            IList<Quadrant> borders;

            if (BottomLeft == null || BottomRight == null || TopLeft == null || TopRight == null)
                return new List<Quadrant>() { this };

            switch (direction)
            {
                case Directions.L:
                    borders = this.BottomLeft.GetBorders(direction);
                    ((List<Quadrant>)borders).AddRange(this.TopLeft.GetBorders(direction));
                    return borders;

                case Directions.R:
                    borders = this.BottomRight.GetBorders(direction);
                    ((List<Quadrant>)borders).AddRange(this.TopRight.GetBorders(direction));
                    return borders;

                case Directions.U:
                    borders = this.TopLeft.GetBorders(direction);
                    ((List<Quadrant>)borders).AddRange(this.TopRight.GetBorders(direction));
                    return borders;

                case Directions.D:
                    borders = this.BottomLeft.GetBorders(direction);
                    ((List<Quadrant>)borders).AddRange(this.BottomRight.GetBorders(direction));
                    return borders;



                case Directions.DL:
                    borders = this.BottomLeft.GetBorders(direction);
                    return borders;

                case Directions.DR:
                    borders = this.BottomRight.GetBorders(direction);
                    return borders;

                case Directions.UL:
                    borders = this.TopLeft.GetBorders(direction);
                    return borders;

                case Directions.UR:
                    borders = this.TopRight.GetBorders(direction);
                    return borders;

                default:
                    throw new Exception("GetBorders support only R L U D");
            }
        }

        public Quadrant GetDirectChildBySubCode2(Quadrants subcode)
        {
            switch (subcode)
            {
                case Quadrants.BottomLeft:
                    if (this.BottomLeft == null)
                        return BottomLeft;
                    return this.BottomLeft;

                case Quadrants.BottomRight:
                    return this.BottomRight;

                case Quadrants.TopLeft:
                    return this.TopLeft;

                case Quadrants.TopRight:
                    return this.TopRight;

                default:
                    throw new Exception("Incorrect subcode");
            }
        }

        public Quadrant GetDirectChildBySubCode(Quadrants subcode)
        {
            switch (subcode){
                case Quadrants.BottomLeft:
                    if (this.BottomLeft == null)
                        throw new Exception(this.GlobalPosition + ": BL subquadrant is null");
                    else
                        return this.BottomLeft;

                case Quadrants.BottomRight:
                    if (this.BottomRight == null)
                        throw new Exception(this.GlobalPosition + ": BR subquadrant is null");
                    else
                        return this.BottomRight;

                case Quadrants.TopLeft:
                    if (this.TopLeft == null)
                        throw new Exception(this.GlobalPosition + ": TL subquadrant is null");
                    else
                        return this.TopLeft;

                case Quadrants.TopRight:
                    if (this.TopRight == null)
                        throw new Exception(this.GlobalPosition + ": BL subquadrant is null");
                    else
                        return this.TopRight;

                default:
                    throw new Exception("Incorrect subcode");
            }            
        }

        public override string ToString()
        {
            var t = GlobalPositionNum == 1411423313 ? "OLOLO" : GlobalPositionNum.ToString();
            return t + (Neighbors != null ? ": " + Neighbors.Length.ToString() : ": 0");
//            return GlobalPositionNum.ToString();
        }
    }
}