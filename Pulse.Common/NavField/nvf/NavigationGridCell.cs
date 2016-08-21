using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NavigField
{
    public class NavigationGridCell : GridCell
    {        
        public bool extended_alg;
        public double ak { get; set; }
        public bool isTraversable;
        public bool isAim;
        public bool wasCalculated;
        public bool unCalculatable;
        public int calcIterationsPassed { get; set; }
        public int simpleCalcIterationsPassed { get; set; }
        public int extendedCalcIterationsPassed { get; set; }
        private double pathCst;
        public double pathCost
        {
            get
            {
                return pathCst;
            }
            set
            {
                pathCst = value;
                if (pathCst == Double.PositiveInfinity)
                    isObstcl = true;
                else
                    isObstcl = false;
            }   
        }

        public int xPredecessor { get; set; }
        public int yPredecessor { get; set; }        

        public int xPredecessor1 { get; set; }
        public int yPredecessor1 { get; set; }
        public double extendedXPredecessor { get; set; }
        public double extendedYPredecessor { get; set; }


        private bool isObstcl;
        public new bool isObstacle
        {
            get { return isObstcl; }
            set
            {
                isObstcl = value;
                if (isObstcl)
                    pathCst = Double.PositiveInfinity;
                else
                    pathCst = 0;
            }
        }

        public bool extendedCalculated;

        public NavigationGridCell()
        {            
            simpleCalcIterationsPassed = 0;
            extendedCalcIterationsPassed = 0;
            pathCost = 0;
            isTraversable = true;
            ak = 2;
        }

        public int Update()
        {
            return 0;
        }
    }
}
