using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NavigField
{
    public class GridCell
    {
        public int xPos { get; protected set; }
        public int yPos { get; protected set; }

        public int space { get; set; }

        public double angle { get; set; }
        public double velocity { get; set; }

        public bool isActive { get; set; }
        public bool isObstacle { get; set; }
               

        public GridCell(bool isObstcl = false, int x = 0, int y = 0, double angle_tmp = 0, double velocity_tmp = 0)
        {
            isObstacle = isObstcl;
            xPos = x;
            yPos = y;
            angle = angle_tmp;
            velocity = velocity_tmp;
        }
    }
}

