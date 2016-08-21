using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NavigField
{
    public class FieldSpace
    {             
        public GridCell[,] FieldArray { get; set; }
        public int xSize { get; protected set; }
        public int ySize { get; protected set; }

        public FieldSpace(int xSize_tmp = 100, int ySize_tmp = 100)
        {
            FieldArray = new GridCell[xSize_tmp, ySize_tmp];

            xSize = xSize_tmp;
            ySize = ySize_tmp;

            for (int i = 0; i < xSize; i++)
                for (int j = 0; j < ySize; j++)
                    FieldArray[i,j] = new GridCell();            
        }

        public virtual int UpdateCell(bool isObstacle, int x, int y, double angle, double cost)
        {
            if (x < xSize && y < ySize && x > -1 && y> -1 && FieldArray.Length > 0 )
            {
                FieldArray[x,y] = new GridCell(isObstacle, x, y, angle, cost);
                return 0;
            }
            else
                return -1;
        }

        public int GetCountOfFields()
        {
            return xSize * ySize;
        }

        public virtual int GetCountOfActiveCells()
        {
            int countOfActive = 0;

            foreach(GridCell e in FieldArray)
            {
                
                if (!e.isObstacle)
                    countOfActive++;
            }
            return countOfActive;
        }

    }

}