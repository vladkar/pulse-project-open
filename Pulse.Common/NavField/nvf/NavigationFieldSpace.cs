using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;

using Pulse.Common.Scenery.Objects;
using Pulse.MultiagentEngine.Map;

namespace NavigField
{
    public class NavigationFieldSpace : FieldSpace
    {
        public new NavigationGridCell[,] NavigFieldArray { get; set; }
        public int countOfActiveCells;
        public int countOfUnevaluatedCells;
        public int countOfObstclCells;
        public int countOfUnevaltdObstCells;
        private bool _isEightConnectedGrid;
        public bool isEightConnectedGrid
        {
            get { return _isEightConnectedGrid; }
            set
            {
                _isEightConnectedGrid = value;
                _isFourConnected = !value;
                _isFourDiagConnected = !value;
            }
        }
        public bool _isFourConnected;
        public bool isFourConnected
        {
            get { return _isFourConnected; }
            set
            {
                _isFourConnected = value;
                _isEightConnectedGrid = !value;
                _isFourDiagConnected = !value;
            }
        }
        public bool _isFourDiagConnected;
        public bool isFourDiagConnected
        {
            get { return _isFourDiagConnected; }
            set
            {
                _isFourDiagConnected = value;
                _isEightConnectedGrid = !value;
                _isFourConnected = !value;
            }
        }
        public bool isAgressiveAlg;
        public double preferredVelocity { get; set; }
        int[] kxky = new int[] { 0, -1, 0, 1, 0, -1 };
        public bool initialGridCalculated;

        public int calcIterationsPassed { get; set; }
        public int sleepTimeOnSimple;
        public int sleepTimeOnExtended;
        public int sleepTimeOnSimpleCell;
        public int sleepTimeOnExtendedCell;


        public PulseVector2 _offset;
        public double _toPulseVector2Multiplier;
        public ICollection<PulseVector2[]> _obstacles;
        public PulseVector2[] _initPoints;
        public PulseVector2[] _customBorder = null;
        public int safetyDistanceToBorder;



        private delegate int Del(int xCurrent, int yCurrent, int xIsCalculated, int yIsCalculated);


        public NavigationFieldSpace(int xSize_tmp = 100, int ySize_tmp = 100) : base(xSize_tmp, ySize_tmp)
        {
            Clear();
            //            Initialization();        
        }

        public void LoadInitData(double toCoordsMultiplier, ICollection<PulseVector2[]> obstacles, PulseVector2[] initPoints, PulseVector2[] customBorder = null)
        {
            _toPulseVector2Multiplier = toCoordsMultiplier;
            _obstacles = obstacles;
            _initPoints = initPoints;
            _customBorder = customBorder;


            foreach (PulseVector2[] _obstacle in obstacles)
            {

                PulseVector2 prev = new PulseVector2();
                PulseVector2 first = new PulseVector2();
                PulseVector2 last = new PulseVector2();

                first = _obstacle.First<PulseVector2>();
                last = _obstacle.Last<PulseVector2>();

                int i = 0;

                foreach (PulseVector2 e in _obstacle)
                {

                    i++;
                    if (prev == default(PulseVector2))
                    {
                        prev = e;

                        continue;
                    }

                    this.SetLineObstacles((prev.X - this._offset.X) / toCoordsMultiplier, (prev.Y - this._offset.Y) / toCoordsMultiplier,
                        (e.X - this._offset.X) / toCoordsMultiplier, (e.Y - this._offset.Y) / toCoordsMultiplier, 0.5);
                    prev = e;

                }
                this.SetLineObstacles((last.X - this._offset.X) / toCoordsMultiplier, (last.Y - this._offset.Y) / toCoordsMultiplier,
                        (first.X - this._offset.X) / toCoordsMultiplier, (first.Y - this._offset.Y) / toCoordsMultiplier, 0.5);

            }

            this.MarkActiveArea(initPoints[0], this._offset, toCoordsMultiplier);

        }

        public int MarkActiveArea(PulseVector2 initPoint, PulseVector2 offset, double toCoordsMultiplier)
        {/*
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    if (this.NavigFieldArray[i, j].space == -1
                        &&
                        !this.NavigFieldArray[i, j].isObstacle
                        &&
                        !this.NavigFieldArray[i, j].isAim)
                    {
                        this.NavigFieldArray[i, j].space = 0;
                        this.Indexer(this.MarkingObstclSpace, i, j, Math.Max(this.xSize, this.ySize), Math.Max(this.xSize, this.ySize));

                        i = xSize - 1;

                        break;
                        
                    }
                }
            
            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    if (this.NavigFieldArray[i, j].space == -1
                        &&
                        !this.NavigFieldArray[i, j].isObstacle
                        &&
                        !this.NavigFieldArray[i, j].isAim)
                    {*/

            int initXIndex = (int)(Math.Round((initPoint.X - offset.X) / toCoordsMultiplier));
            int initYIndex = (int)(Math.Round((initPoint.Y - offset.Y) / toCoordsMultiplier));

            this.NavigFieldArray[initXIndex, initYIndex].space = 1;

            this.NavigFieldArray[initXIndex, initYIndex].isActive = true;
            int _countOfActiveCells = 0;

            do
            {
                _countOfActiveCells = this.countOfActiveCells;
                this.Indexer(this.MarkingActiveSpace, initXIndex, initYIndex, Math.Max(this.xSize, this.ySize));
            } while (_countOfActiveCells != this.countOfActiveCells);

            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                    if (this.NavigFieldArray[i, j].space == -1)
                        this.NavigFieldArray[i, j].isObstacle = true;




            return 0;
        }

        public int Clear()
        {
            NavigFieldArray = new NavigationGridCell[xSize, ySize];
            FieldArray = new GridCell[xSize, ySize];

            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    NavigFieldArray[i, j] = new NavigationGridCell();
                    FieldArray[i, j] = new GridCell();

                    this.NavigFieldArray[i, j].space = -1;
                    this.NavigFieldArray[i, j].isActive = false;
                    this.NavigFieldArray[i, j].wasCalculated =false;
                    this.NavigFieldArray[i, j].unCalculatable = false;

                    this.NavigFieldArray[i, j].xPredecessor = i;
                    this.NavigFieldArray[i, j].yPredecessor = j;
                }

            initialGridCalculated = false;
            this.calcIterationsPassed = 0;
            this.preferredVelocity = 0.1;
            this.sleepTimeOnSimple = 0;

            this.sleepTimeOnSimple = 0;
            this.sleepTimeOnExtended = 0;
            this.sleepTimeOnSimpleCell = 0;
            this.sleepTimeOnExtendedCell = 0;
            this.isFourConnected = true;

            return 0;
        }

        public int ClearCalculations()
        {
            for (int i = 0; i < this.xSize; i++)
                for (int j = 0; j < this.ySize; j++)
                    this.NavigFieldArray[i, j].wasCalculated = false;
            this.calcIterationsPassed = 0;

            return 0;
        }



        public int SetLineObstacles(double X0, double Y0,
                               double X1, double Y1,
                               double guidVelocity = 0.7, double dx = 1, double directCorrection = 0)
        {


            if (X1 - X0 != 0)
            {
                double k = Math.Abs((Y1 - Y0) / (X1 - X0));
                double kx = (X1 - X0) / Math.Abs(X1 - X0);
                double ky = 0;

                double cellCountX = Math.Abs(X1 - X0);
                double cellCountY = Math.Abs(Y1 - Y0);
                //double cellCount = Math.Max(Math.Abs(X1 - X0), Math.Abs(Y1 - Y0));

                if (Y1 - Y0 != 0)
                    ky = (Y1 - Y0) / Math.Abs(Y1 - Y0);

                if (k < 1)
                    for (double x = 0; x < cellCountX; x += dx)
                    {
                        double y;
                        if (Y1 - Y0 != 0)
                            y = k * x;
                        else
                            y = 0;

                        int xCoord = (int)Math.Truncate(kx * x + X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                            (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                            this.NavigFieldArray[xCoord, yCoord].isObstacle = true;

                    }
                else
                    for (double y = 0; y < cellCountY; y += dx)
                    {
                        double x;
                        x = y / k;

                        int xCoord = (int)Math.Truncate(kx * x + X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                            (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                            this.NavigFieldArray[xCoord, yCoord].isObstacle = true;

                    }

            }

            else
            {
                double cellCountY = Math.Abs(Y1 - Y0);
                double y;
                double ky;

                if (Y1 - Y0 != 0)
                {
                    ky = (Y1 - Y0) / Math.Abs(Y1 - Y0);
                    for (y = 0; y < cellCountY; y += dx)
                    {
                        int xCoord = (int)Math.Truncate(X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                            this.NavigFieldArray[xCoord, yCoord].isObstacle = true;
                    }
                }

                else
                {
                    int xCoord = (int)Math.Truncate(X0);
                    int yCoord = (int)Math.Truncate(Y0);

                    if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                        this.NavigFieldArray[xCoord, yCoord]
                        .isObstacle = true;

                    return 0;
                };

            }



            return 0;
        }

        public int SetLineGuid(double X0, double Y0,
                               double X1, double Y1,
                               double guidVelocity = 0.7, double dx = 1, double directCorrection = 0)
        {

            double direction = this.Direction(X0, Y0, X1, Y1);

            if (X1 - X0 != 0)
            {
                double k = Math.Abs((Y1 - Y0) / (X1 - X0));
                double kx = (X1 - X0) / Math.Abs(X1 - X0);
                double ky = 0;

                double cellCountX = Math.Abs(X1 - X0);
                double cellCountY = Math.Abs(Y1 - Y0);
                //double cellCount = Math.Max(Math.Abs(X1 - X0), Math.Abs(Y1 - Y0));

                if (Y1 - Y0 != 0)
                    ky = (Y1 - Y0) / Math.Abs(Y1 - Y0);

                if (k < 1)
                    for (double x = 0; x < cellCountX; x += dx)
                    {
                        double y;
                        if (Y1 - Y0 != 0)
                            y = k * x;
                        else
                            y = 0;

                        int xCoord = (int)Math.Truncate(kx * x + X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                            (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                        {
                            this.FieldArray[xCoord, yCoord].angle
                                = direction + directCorrection;

                            this.FieldArray[xCoord, yCoord].velocity
                                = guidVelocity;
                        }

                    }
                else
                    for (double y = 0; y < cellCountY; y += dx)
                    {
                        double x;
                        x = y / k;

                        int xCoord = (int)Math.Truncate(kx * x + X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                            (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                        {
                            this.FieldArray[xCoord, yCoord].angle
                                = direction + directCorrection;

                            this.FieldArray[xCoord, yCoord].velocity
                                = guidVelocity;
                        }

                    }

            }

            else
            {
                double cellCountY = Math.Abs(Y1 - Y0);
                double y;
                double ky;

                if (Y1 - Y0 != 0)
                {
                    ky = (Y1 - Y0) / Math.Abs(Y1 - Y0);
                    for (y = 0; y < cellCountY; y += dx)
                    {
                        int xCoord = (int)Math.Truncate(X0);
                        int yCoord = (int)Math.Truncate(ky * y + Y0);

                        if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                        {
                            this.FieldArray[xCoord, yCoord].angle
                                = direction + directCorrection;

                            this.FieldArray[xCoord, yCoord].velocity
                                = guidVelocity;
                        }
                    }
                }

                else
                {
                    int xCoord = (int)Math.Truncate(X0);
                    int yCoord = (int)Math.Truncate(Y0);

                    if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)))
                    {
                        this.FieldArray[xCoord, yCoord].angle
                            = direction + directCorrection;

                        this.FieldArray[xCoord, yCoord].velocity
                            = guidVelocity;
                    }

                    return 0;
                };

            }

            return 0;
        }

        public int SetSineWaveGuid(int X0, int Xn,
                                    int Y0, double guidVelocity = 0.99,
                                    double A = 5, double directCorrection = 0)
        {

            for (int x = X0; x < Xn; x++)
            {
                double y = Y0 + A * Math.Sin((2 * Math.PI * ((double)(x - X0) / (double)(Xn - X0))));
                double y2 = Y0 + A * Math.Sin(2 * Math.PI * (double)(x - X0 + 1) / (double)(Xn - X0));

                int yCoord = (int)Math.Truncate(y);
                int y2Coord = (int)Math.Truncate(y2);

                if ((0 <= x) && (x < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)) &&
                    (0 <= x + 1) && (x + 1 < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= y2Coord) && (y2Coord < this.NavigFieldArray.GetLength(1)))
                {
                    if ((!this.NavigFieldArray[x, yCoord].isObstacle &&
                        !this.NavigFieldArray[x, yCoord].isAim)
                        &&
                        (!this.NavigFieldArray[x + 1, y2Coord].isObstacle &&
                        !this.NavigFieldArray[x + 1, y2Coord].isAim))
                    {
                        this.FieldArray[x, (int)Math.Truncate(y)].angle
                            = this.Direction(x, y, x + 1, y2) + directCorrection;
                        this.FieldArray[x, (int)Math.Truncate(y)].velocity
                            = guidVelocity;
                    }
                }


            }

            return 0;
        }

        public int SetCircleGuid(int X0, int Y0,
                                 int radius, double dx = 0.1,
                                 double guidVelocity = 0.7,
                                 double directCorrection = 0)
        {


            for (int k = -1; k < 2; k += 2)
                for (double x = X0 - radius; x < X0 + radius; x += dx)
                {
                    double y = Y0 + k * Math.Sqrt(
                        Math.Abs(Math.Pow(radius, 2) - Math.Pow(x - X0, 2)));
                    double y2 = Y0 + k * Math.Sqrt(
                        Math.Abs(Math.Pow(radius, 2) - Math.Pow(x - X0 + dx, 2)));

                    int xCoord = (int)Math.Round(x);
                    int x2Coord = (int)Math.Round(x + dx);

                    int yCoord = (int)Math.Round(y);
                    int y2Coord = (int)Math.Round(y2);

                    if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)) &&
                        (0 <= x2Coord) && (x2Coord < this.NavigFieldArray.GetLength(0)) &&
                        (0 <= y2Coord) && (y2Coord < this.NavigFieldArray.GetLength(1)))
                    {
                        if ((!this.NavigFieldArray[xCoord, yCoord].isObstacle &&
                            !this.NavigFieldArray[xCoord, yCoord].isAim)
                            &&
                            (!this.NavigFieldArray[x2Coord, y2Coord].isObstacle &&
                            !this.NavigFieldArray[x2Coord, y2Coord].isAim))
                        {
                            this.FieldArray[xCoord, (int)Math.Truncate(y)].angle
                                = this.Direction(x, y, x + dx, y2)
                                + directCorrection
                                + (k + 1) * Math.PI / 2;
                            this.FieldArray[xCoord, (int)Math.Truncate(y)].velocity
                                = guidVelocity;
                        }
                    }
                }



            return 0;
        }

        public int SetSpiralGuid(int X0, int Y0,
                                 int radius, double da = 0.1,
                                 double dr = 0.01,
                                 double guidVelocity = 0.7,
                                 double directCorrection = 0, double a0 = 0)
        {
            Random random = new Random();


            for (double a = a0, r = 1; a < 1000; a += da, r += dr)
            {
                double x = X0 + r * Math.Cos(a);
                double y = Y0 + r * Math.Sin(a);

                double x1 = X0 + (r + dr) * Math.Cos(a + da);
                double y1 = Y0 + (r + dr) * Math.Sin(a + da);



                int xCoord = (int)Math.Round(x1);
                int x2Coord = (int)Math.Round(x1);

                int yCoord = (int)Math.Round(y);
                int y2Coord = (int)Math.Round(y1);

                if ((0 <= xCoord) && (xCoord < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= yCoord) && (yCoord < this.NavigFieldArray.GetLength(1)) &&
                    (0 <= x2Coord) && (x2Coord < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= y2Coord) && (y2Coord < this.NavigFieldArray.GetLength(1)))
                {
                    if ((!this.NavigFieldArray[xCoord, yCoord].isObstacle &&
                        !this.NavigFieldArray[xCoord, yCoord].isAim)
                        &&
                        (!this.NavigFieldArray[x2Coord, y2Coord].isObstacle &&
                        !this.NavigFieldArray[x2Coord, y2Coord].isAim))
                    {/*
                        this.FieldArray[xCoord, (int)Math.Truncate(y)].angle
                            = this.Direction(x, y, x1, y1)
                            + directCorrection;
                        */

                        this.FieldArray[xCoord, (int)Math.Truncate(y)].angle
                            = random.Next();
                        this.FieldArray[xCoord, (int)Math.Truncate(y)].velocity
                            = guidVelocity;
                    }
                }
            }



            return 0;
        }



        public int CalculateFieldForAim(int xAimIndex, int yAimIndex,
                                        int extendedIterations = 5,
                                        double preferredVelocity = 1,
                                        double initCost = 0)
        {
            this.NavigFieldArray[xAimIndex, yAimIndex].isAim = true;
            this.NavigFieldArray[xAimIndex, yAimIndex].pathCost = initCost;

            this.preferredVelocity = preferredVelocity;

            int maxDistToBorder = Math.Max(Math.Max((this.xSize - 1) - xAimIndex, xAimIndex),
                Math.Max((this.ySize - 1) - yAimIndex, yAimIndex));

            this.isFourConnected = true;
// Calculating Fields for obstacle regions

            this.countOfObstclCells = this.xSize * this.ySize - this.countOfActiveCells;
            this.countOfUnevaluatedCells = this.countOfObstclCells;


            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                    if (this.NavigFieldArray[i, j].isActive)
                    {
                        this.NavigFieldArray[i, j].wasCalculated = true;
                        this.NavigFieldArray[i, j].unCalculatable = true;
                    }
                        
                    
            while (this.countOfUnevaluatedCells > 1)
                this.Indexer(this.PathFinderObstArea, xAimIndex, yAimIndex, Math.Max(this.xSize, this.ySize));

            this.Indexer(ExtendedPredecessorFinder, xAimIndex, yAimIndex, maxDistToBorder, extendedIterations);

            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                {
                    if (!this.NavigFieldArray[i, j].isActive)
                        this.NavigFieldArray[i, j].isObstacle = true;

                    this.NavigFieldArray[i, j].wasCalculated = false;
                    this.NavigFieldArray[i, j].unCalculatable = !this.NavigFieldArray[i, j].unCalculatable;
                }

// Calculating main Navigation Field

            if (!initialGridCalculated)
            {
                this.countOfUnevaluatedCells = this.countOfActiveCells;
                this.NavigFieldArray[xAimIndex, yAimIndex].wasCalculated = true;

                while (this.countOfUnevaluatedCells > 0)
                    this.Indexer(PathFinderActArea, xAimIndex, yAimIndex, maxDistToBorder);

            }

            this.Indexer(ExtendedPredecessorFinder, xAimIndex, yAimIndex, maxDistToBorder, extendedIterations);

            
            return 0;
        }

        public double Direction(double Ax, double Ay,
                                               double Bx, double By)
        {

            double distance = Math.Sqrt(
                    Math.Pow(Math.Abs(Ax - Bx), 2) +
                    Math.Pow(Math.Abs(Ay - By), 2));


            double a = 0;

            if (Bx - Ax != 0)
            {
                a = Math.Atan(
                (By - Ay) /
                (Bx - Ax));

                if (Bx - Ax < 0)
                    a += Math.PI;
            }
            else
                a = Math.Asin((By - Ay) / distance);

            if (By - Ay == 0)
                a = Math.Acos((Bx - Ax) / distance);

            return a;
        }


        public override int GetCountOfActiveCells()
        {
            int countOfActive = 0;

            foreach (NavigationGridCell e in NavigFieldArray)
            {

                if (!e.isObstacle || e.isAim)
                    countOfActive++;
            }


            return countOfActive;
        }


        private int Indexer(Del handler, int xAimIndex, int yAimIndex, int calculatingRadius = 1, int calcIterations = 1)
        {

            for (int iteration = 1; iteration <= calcIterations; iteration++)
            {
                if (handler == PathFinderActArea)
                {
                    this.calcIterationsPassed++;
                    System.Threading.Thread.Sleep(this.sleepTimeOnSimple);
                }



                if (handler == ExtendedPredecessorFinder)
                {
                    this.calcIterationsPassed++;
                    System.Threading.Thread.Sleep(this.sleepTimeOnExtended);
                }




                for (int radius = 1; radius <= calculatingRadius; radius++)
                {
                    int onSideCellsToDraw = 2 * radius;

                    int xBeginIndex = xAimIndex + radius;
                    int yBeginIndex = yAimIndex + radius;

                    int x = xBeginIndex, y = yBeginIndex;


                    for (int rectSide = 0; rectSide < 4; rectSide++)
                    {
                        int kx = kxky[rectSide + 1], ky = kxky[rectSide];

                        for (int dl = 0; dl < onSideCellsToDraw; dl++)
                        {
                            x += kx; y += ky;

                            if ((0 <= x) && (x < this.NavigFieldArray.GetLength(0)) &&
                                (0 <= y) && (y < this.NavigFieldArray.GetLength(1)))
                                handler(x, y, xAimIndex, yAimIndex);


                        }
                    }
                }
            }

            if (handler == PathFinderActArea)
                initialGridCalculated = true;


            return 0;
        }


        private int MarkingActiveSpace(int xIsCalculated, int yIsCalculated, int xPrev, int yPrev)
        {
            if (!this.NavigFieldArray[xIsCalculated, yIsCalculated].isObstacle
                ||
                this.NavigFieldArray[xIsCalculated, yIsCalculated].isAim)
                this.Indexer(this.MarkingActiveCell, xIsCalculated, yIsCalculated, 1, 1);

            return 0;
        }

        private int MarkingActiveCell(int xCurrent, int yCurrent, int xIsCalculated, int yIsCalculated)
        {

            if (this.isFourConnected
                &&
                (xIsCalculated == xCurrent || yIsCalculated == yCurrent)
                ||
                this.isEightConnectedGrid)

                if (this.NavigFieldArray[xIsCalculated, yIsCalculated].space == -1
                    &&
                    !this.NavigFieldArray[xIsCalculated, yIsCalculated].isObstacle
                    &&

                    this.NavigFieldArray[xCurrent, yCurrent].isActive)
                {
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].space = 1;
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].isActive = true;
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost = 0;
                    this.countOfActiveCells++;

                }

            return 0;
        }

        private int PathFinderActArea(int xIsCalculated, int yIsCalculated, int param3, int param4)
        {
            if (this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated)
                return 0;

            if (this.NavigFieldArray[xIsCalculated, yIsCalculated].isActive)
            {

                System.Threading.Thread.Sleep(this.sleepTimeOnSimpleCell);

                this.Indexer(PredecessorFinderActArea, xIsCalculated, yIsCalculated);

                if (!this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated)
                    return 0;

                int xPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].xPredecessor;
                int yPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].yPredecessor;
                

                if (this.isEightConnectedGrid || this.isFourConnected)
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost
                        = this.EvaluatingCellActArea(
                            xIsCalculated,
                            yIsCalculated,
                        xPredecessor,
                        yPredecessor, false);

                if (this.isFourDiagConnected
                    &&
                    (xIsCalculated != xPredecessor && yIsCalculated != yPredecessor))

                    this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost
                    = this.EvaluatingCellActArea(
                        xIsCalculated,
                        yIsCalculated,
                    xPredecessor,
                    yPredecessor, false);                
            }

            return 0;
        }

        private int PredecessorFinderActArea(int xCurrent, int yCurrent, int xIsCalculated, int yIsCalculated)
        {
            if (this.NavigFieldArray[xIsCalculated, yIsCalculated].isActive)
                PredecessorFinder(xCurrent, yCurrent, xIsCalculated, yIsCalculated);


            return 0;
        }


        private double EvaluatingCellActArea(int xIsCalculating, int yIsCalculating,
                                      int currentXPredecessor, int currentYPredecessor, bool costOnly = true)
        {

            double aimCellDistance = Math.Sqrt(
                    Math.Pow(Math.Abs(xIsCalculating - currentXPredecessor), 2) +
                    Math.Pow(Math.Abs(yIsCalculating - currentYPredecessor), 2));

            double guidVector = this.FieldArray[currentXPredecessor, currentYPredecessor].velocity;
            double a, navDirect;

            if (guidVector == 0)
            {
                navDirect = Direction(xIsCalculating, yIsCalculating, currentXPredecessor, currentYPredecessor);
                a = navDirect;
                a -= this.FieldArray[currentXPredecessor, currentYPredecessor].angle;
            }

            else
            {
                a = this.FieldArray[currentXPredecessor, currentYPredecessor].angle;
                navDirect = a;
            }




            double predCellTraversingVelocity = ((guidVector * Math.Cos(a)) +
                Math.Sqrt(
                    Math.Pow(guidVector * Math.Cos(a), 2) -
                    Math.Pow(guidVector, 2) +
                    this.preferredVelocity));

            //predCellTraversingVelocity = (predCellTraversingVelocity > 1) ? 1 : predCellTraversingVelocity;
            if (this.isAgressiveAlg)
                predCellTraversingVelocity = (Math.Cos(a) > 0 && guidVector > 0)
                ?
                Math.Min(predCellTraversingVelocity, guidVector)
                    / ((double)(this.preferredVelocity) + guidVector) : predCellTraversingVelocity;

            double predCellTraversingCost = (aimCellDistance / 2) / predCellTraversingVelocity;

            a = navDirect;
            a -= this.FieldArray[xIsCalculating, yIsCalculating].angle;
            guidVector = this.FieldArray[xIsCalculating, yIsCalculating].velocity;

            double cellTraversingVelocity = ((guidVector * Math.Cos(a)) +
                Math.Sqrt(
                    Math.Pow(guidVector * Math.Cos(a), 2) -
                    Math.Pow(guidVector, 2) +
                    this.preferredVelocity));

            //cellTraversingVelocity = (cellTraversingVelocity > 1) ? 1 : cellTraversingVelocity;
            if (this.isAgressiveAlg)
                cellTraversingVelocity = (Math.Cos(a) > 0 && guidVector > 0)
                ?
                Math.Min(cellTraversingVelocity, guidVector)
                    / ((double)(this.preferredVelocity) + guidVector) : cellTraversingVelocity;

            double cellTraversingCost = (aimCellDistance / 2) / cellTraversingVelocity;




            double resultCost;

            if (!this.NavigFieldArray[currentXPredecessor, currentYPredecessor].isObstacle)
                resultCost = Math.Round((this.NavigFieldArray[currentXPredecessor, currentYPredecessor]
                    .pathCost + cellTraversingCost + predCellTraversingCost), 2);
            else
                resultCost = cellTraversingCost;

            if (costOnly)
                return resultCost;

            this.NavigFieldArray[xIsCalculating, yIsCalculating].xPredecessor1 = currentXPredecessor;
            this.NavigFieldArray[xIsCalculating, yIsCalculating].yPredecessor1 = currentYPredecessor;
            this.NavigFieldArray[xIsCalculating, yIsCalculating].angle = navDirect;
            this.NavigFieldArray[xIsCalculating, yIsCalculating].velocity = cellTraversingVelocity;
            this.NavigFieldArray[xIsCalculating, yIsCalculating].pathCost = resultCost;


            return resultCost;
        }

        private int ExtendedPredecessorFinder(int xIsCalculating, int yIsCalculating,
                                         int param3, int param4)
        {
            
            if (this.NavigFieldArray[xIsCalculating, yIsCalculating].isObstacle
                ||
                this.NavigFieldArray[xIsCalculating, yIsCalculating].isAim
                ||
                this.NavigFieldArray[xIsCalculating, yIsCalculating].unCalculatable
                )
                return 0;
            
            System.Threading.Thread.Sleep(this.sleepTimeOnExtendedCell);

            double ak = this.NavigFieldArray[xIsCalculating, yIsCalculating].ak;

            for (int rectSide = 0; rectSide < 4; rectSide++)
            {
                int ax = this.kxky[rectSide + 1];
                int ay = this.kxky[rectSide];
                int bx = this.kxky[rectSide + 2];
                int by = this.kxky[rectSide + 1];


                int Ax = xIsCalculating + ax;
                int Ay = yIsCalculating + ay;

                int Bx = xIsCalculating + bx;
                int By = yIsCalculating + by;

                int Cx = xIsCalculating + bx + by;
                int Cy = yIsCalculating + ax + ay;

                if ((0 <= Ax) && (Ax < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= Ay) && (Ay < this.NavigFieldArray.GetLength(1)) &&
                    (0 <= Bx) && (Bx < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= By) && (By < this.NavigFieldArray.GetLength(1)) &&
                    (0 <= Cx) && (Cx < this.NavigFieldArray.GetLength(0)) &&
                    (0 <= Cy) && (Cy < this.NavigFieldArray.GetLength(1)))
                {
                    if ((!this.NavigFieldArray[Ax, Ay].isObstacle ||
                          this.NavigFieldArray[Ax, Ay].isAim)
                          &&
                        (!this.NavigFieldArray[Bx, By].isObstacle ||
                          this.NavigFieldArray[Bx, By].isAim)
                          &&
                        (!this.NavigFieldArray[Cx, Cy].isObstacle ||
                          this.NavigFieldArray[Cx, Cy].isAim))
                    {
                        double costA = this
                            .NavigFieldArray[Ax, Ay]
                            .pathCost;

                        double costB = this
                            .NavigFieldArray[Bx, By]
                            .pathCost;

                        int kx = 0;
                        int vx = 0;

                        int ky = 0;
                        int vy = 0;

                        switch (rectSide)
                        {
                            case 0:
                                {
                                    kx = 1;
                                    vx = -1;

                                    ky = -1;
                                    vy = 0;
                                }
                                break;
                            case 1:
                                {
                                    kx = 1;
                                    vx = 0;

                                    ky = 1;
                                    vy = -1;
                                }
                                break;
                            case 2:
                                {
                                    kx = -1;
                                    vx = 1;

                                    ky = 1;
                                    vy = 0;
                                }
                                break;
                            case 3:
                                {
                                    kx = -1;
                                    vx = 0;

                                    ky = -1;
                                    vy = 1;
                                }
                                break;
                        }

                        double currentCost = 0;

                        for (double a = 0; a < 1; a += 1 / ak)
                        {
                            double currentXPoint = xIsCalculating + a * kx + vx;
                            double currentYPoint = yIsCalculating + a * ky + vy;



                            currentCost = ExtendedEvaluatingCell(
                                xIsCalculating, yIsCalculating,
                                currentXPoint,
                                currentYPoint,
                                Ax, Ay,
                                Bx, By,
                                a);


                        }

                    }
                }
            }


            return 0;
        }


        private double ExtendedEvaluatingCell(int xIsCalculating, int yIsCalculating,
                                               double currentXPoint, double currentYPoint,
                                               int Ax, int Ay,
                                               int Bx, int By,
                                               double coordParam)
        {
            double resultCost;
            double cellDistance = 1;

            double toPointDistance = Math.Sqrt(
                    Math.Pow(Math.Abs(xIsCalculating - currentXPoint), 2) +
                    Math.Pow(Math.Abs(yIsCalculating - currentYPoint), 2));

            double a, navDirect
                = Direction(xIsCalculating, yIsCalculating, currentXPoint, currentYPoint);

            a = navDirect;

            double guidVector;

            if (0 <= navDirect && navDirect < Math.PI / 4
                 ||
                 Math.PI / 2 <= navDirect && navDirect < Math.PI * 3 / 4
                 ||
                 Math.PI <= navDirect && navDirect < Math.PI * 5 / 4
                 ||
                 -Math.PI / 2 <= navDirect && navDirect < -Math.PI / 4)
            {
                a -= this.FieldArray[Ax, Ay].angle;
                guidVector = this.FieldArray[Ax, Ay].velocity;
            }
            else
            {
                a -= this.FieldArray[Bx, By].angle;
                guidVector = this.FieldArray[Bx, By].velocity;
            }

            if (guidVector > 0)            
            {
                a = -1 * (a - navDirect);
                navDirect = a;
            }

            double predCellTraversingVelocity = ((guidVector * Math.Cos(a)) +
                Math.Sqrt(
                    Math.Pow(guidVector * Math.Cos(a), 2) -
                    Math.Pow(guidVector, 2) +
                    this.preferredVelocity));

            //predCellTraversingVelocity = (predCellTraversingVelocity > 1) ? 1 : predCellTraversingVelocity;

            if (this.isAgressiveAlg)
                predCellTraversingVelocity = (Math.Cos(a) > 0 && guidVector > 0)
                ?
                Math.Min(predCellTraversingVelocity, guidVector)
                    / ((double)(this.preferredVelocity) + guidVector) : predCellTraversingVelocity;

            double isCalculatingCellDistance = 0;
            double predCellDistance = 0;

            if (-Math.PI / 2 <= navDirect && navDirect < 0)
            {
                isCalculatingCellDistance = (cellDistance / 2) /
                    Math.Cos(
                        Math.Abs(Math.Abs(-Math.PI / 4 + navDirect + Math.PI / 2) - Math.PI / 4));
                predCellDistance = toPointDistance - isCalculatingCellDistance;
            }
            if (0 <= navDirect && navDirect < Math.PI / 2)
            {
                isCalculatingCellDistance = (cellDistance / 2) /
                    Math.Cos(
                        Math.Abs(Math.Abs(-Math.PI / 4 + navDirect) - Math.PI / 4));
                predCellDistance = toPointDistance - isCalculatingCellDistance;
            }
            if (Math.PI / 2 <= navDirect && navDirect < Math.PI)
            {
                isCalculatingCellDistance = (cellDistance / 2) /
                    Math.Cos(
                        Math.Abs(Math.Abs(-Math.PI / 4 + navDirect - Math.PI / 2) - Math.PI / 4));
                predCellDistance = toPointDistance - isCalculatingCellDistance;
            }
            if (Math.PI <= navDirect && navDirect < 1.5 * Math.PI)
            {
                isCalculatingCellDistance = (cellDistance / 2) /
                    Math.Cos(
                        Math.Abs(Math.Abs(-Math.PI / 4 + navDirect - Math.PI) - Math.PI / 4));
                predCellDistance = toPointDistance - isCalculatingCellDistance;
            }

            double predCellTraversingCost = predCellDistance / predCellTraversingVelocity;

            a = navDirect;
            a -= this.FieldArray[xIsCalculating, yIsCalculating].angle;
            guidVector = this.FieldArray[xIsCalculating, yIsCalculating].velocity;

            double cellTraversingVelocity = ((guidVector * Math.Cos(a)) +
                Math.Sqrt(
                    Math.Pow(guidVector * Math.Cos(a), 2) -
                    Math.Pow(guidVector, 2) +
                    this.preferredVelocity));

            //cellTraversingVelocity = (cellTraversingVelocity > 1) ? 1 : cellTraversingVelocity;

            if (this.isAgressiveAlg)
                cellTraversingVelocity = (Math.Cos(a) > 0 && guidVector > 0)
                ?
                Math.Min(cellTraversingVelocity, guidVector)
                    / ((double)(this.preferredVelocity) + guidVector) : cellTraversingVelocity;

            double cellTraversingCost = isCalculatingCellDistance / cellTraversingVelocity;

            double Acost = (Double.IsInfinity(this.NavigFieldArray[Ax, Ay].pathCost))
                            ? 0
                            : this.NavigFieldArray[Ax, Ay].pathCost;
            double Bcost = (Double.IsInfinity(this.NavigFieldArray[Bx, By].pathCost))
                            ? 0
                            : this.NavigFieldArray[Bx, By].pathCost;
            resultCost = (1 - coordParam) * Acost + coordParam * Bcost
                            + (cellTraversingCost + predCellTraversingCost);





            if (resultCost < this.NavigFieldArray[xIsCalculating, yIsCalculating].pathCost)
            {
                this.NavigFieldArray[xIsCalculating, yIsCalculating].extendedCalculated = true;
                this.NavigFieldArray[xIsCalculating, yIsCalculating].pathCost = resultCost;
                this.NavigFieldArray[xIsCalculating, yIsCalculating].angle = navDirect;
                this.NavigFieldArray[xIsCalculating, yIsCalculating].velocity = cellTraversingVelocity;
                this.NavigFieldArray[xIsCalculating, yIsCalculating]
                    .extendedXPredecessor = currentXPoint;
                this.NavigFieldArray[xIsCalculating, yIsCalculating]
                    .extendedYPredecessor = currentYPoint;
            }


            return Math.Round(resultCost, 2);
        }

        private int PathFinderObstArea(int xIsCalculated, int yIsCalculated, int param3, int param4)
        {/*
            if (this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated)
                return 0;
*/
            if (!this.NavigFieldArray[xIsCalculated, yIsCalculated].isActive)
            {
                System.Threading.Thread.Sleep(this.sleepTimeOnSimpleCell);

                this.Indexer(PredecessorFinderObstArea, xIsCalculated, yIsCalculated);

                int xPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].xPredecessor;
                int yPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].yPredecessor;
                          

                if (this.isEightConnectedGrid || this.isFourConnected)
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost
                        = this.EvaluatingCellActArea(
                            xIsCalculated,
                            yIsCalculated,
                        xPredecessor,
                        yPredecessor, false);

                if (this.isFourDiagConnected
                    &&
                    (xIsCalculated != xPredecessor && yIsCalculated != yPredecessor))

                    this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost
                    = this.EvaluatingCellActArea(
                        xIsCalculated,
                        yIsCalculated,
                    xPredecessor,
                    yPredecessor, false);
            }

            return 0;
        }

        private int PredecessorFinderObstArea(int xCurrent, int yCurrent, int xIsCalculated, int yIsCalculated)
        {
            if (!this.NavigFieldArray[xIsCalculated, yIsCalculated].isActive)
                PredecessorFinder(xCurrent, yCurrent, xIsCalculated, yIsCalculated);


            return 0;
        }


        private int PredecessorFinder(int xCurrent, int yCurrent, int xIsCalculated, int yIsCalculated)
        {

            int xPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].xPredecessor;
            int yPredecessor = this.NavigFieldArray[xIsCalculated, yIsCalculated].yPredecessor;

            if (this.isFourConnected
                &&
                (xIsCalculated == xCurrent || yIsCalculated == yCurrent)
                ||
                this.isEightConnectedGrid)


                if (this.NavigFieldArray[xCurrent, yCurrent].wasCalculated
                    &&
                    (!(this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated)
                    ||
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated
                    &&
                    this.EvaluatingCellActArea(xIsCalculated, yIsCalculated, xCurrent, yCurrent)
                    <
                    this.NavigFieldArray[xCurrent, yCurrent].pathCost))
                {
                    

                    this.NavigFieldArray[xIsCalculated, yIsCalculated].xPredecessor = xCurrent;
                    this.NavigFieldArray[xIsCalculated, yIsCalculated].yPredecessor = yCurrent;

                    this.countOfUnevaluatedCells = (this.countOfUnevaluatedCells > 0 && !this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated)
                    ?
                    --this.countOfUnevaluatedCells
                    :
                    this.countOfUnevaluatedCells;

                    this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated = true;

                    
                }
                else
                if (this.isFourDiagConnected
                    &&
                    (xIsCalculated != xCurrent && yIsCalculated != yCurrent))

                    if (this.NavigFieldArray[xCurrent, yCurrent].wasCalculated
                        &&
                        this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated
                        &&
                        (this.EvaluatingCellActArea(xIsCalculated, yIsCalculated, xCurrent, yCurrent)
                        <
                        this.NavigFieldArray[xIsCalculated, yIsCalculated].pathCost))
                    {
                        

                        this.NavigFieldArray[xIsCalculated, yIsCalculated].xPredecessor = xCurrent;
                        this.NavigFieldArray[xIsCalculated, yIsCalculated].yPredecessor = yCurrent;
                        
                        this.countOfUnevaluatedCells = (this.countOfUnevaluatedCells > 0) 
                            ?
                            --this.countOfUnevaluatedCells 
                            :
                            this.countOfUnevaluatedCells;

                        this.NavigFieldArray[xIsCalculated, yIsCalculated].wasCalculated = true;
                    }


            return 0;
        }

    }
}
