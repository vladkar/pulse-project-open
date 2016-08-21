using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;


namespace City
{
	public class GeoGrid4KmTo250m
	{
		public LinesGisLayer LinesGrid4km	{ get; protected set; }
		public LinesGisLayer LinesGrid2km	{ get; protected set; }
		public LinesGisLayer LinesGrid1km	{ get; protected set; }
		public LinesGisLayer LinesGrid500m	{ get; protected set; }
		public LinesGisLayer LinesGrid250m	{ get; protected set; }

		public DVector2		LeftBottomCornerDegrees	{ get; protected set; }


		public GeoGrid4KmTo250m(Game game, DVector2 lbPointDegrees, int dim = 10)
		{
			DVector2 lbPoint = lbPointDegrees;
			LinesGrid4km = LinesGisLayer.GenerateDistanceGrid(game, lbPoint, 4, dim, dim, new Color(128, 128, 128, 128));
			LinesGrid4km.ZOrder = 100;

			var dim2Km = dim + dim - 1;
			LinesGrid2km = LinesGisLayer.GenerateDistanceGrid(game, lbPoint, 2, dim2Km, dim2Km, new Color(128, 128, 128, 128));
			LinesGrid2km.ZOrder = 101;

			var dim1Km = dim2Km + dim2Km - 1;
            LinesGrid1km = LinesGisLayer.GenerateDistanceGrid(game, lbPoint, 1, dim1Km, dim1Km, new Color(128, 128, 128, 128));
			LinesGrid1km.ZOrder = 102;

			var dim500m = dim1Km + dim1Km - 1;
            LinesGrid500m = LinesGisLayer.GenerateDistanceGrid(game, lbPoint, 0.5, dim500m, dim500m, new Color(128, 128, 128, 128));
			LinesGrid500m.ZOrder = 103;

			var dim250m = dim500m + dim500m - 1;
            LinesGrid250m = LinesGisLayer.GenerateDistanceGrid(game, lbPoint, 0.250, dim250m, dim250m, new Color(128, 128, 128, 128));
			LinesGrid250m.ZOrder = 104;
			
			LinesGrid4km.Flags = LinesGrid2km.Flags = LinesGrid1km.Flags = LinesGrid500m.Flags = LinesGrid250m.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE | LinesGisLayer.LineFlags.OVERALL_COLOR);
			LinesGrid4km.OverallColor = LinesGrid2km.OverallColor = LinesGrid1km.OverallColor = LinesGrid500m.OverallColor = LinesGrid250m.OverallColor = new Color4(0.3f, 0.3f, 0.3f, 0.3f);
		}

		public void UpdateGrids(double cameraDistacne)
		{
			var camDist = cameraDistacne;

			float grid4kHigh	= 7000, grid4kLow	= 6800;
			float grid2kHigh	= 6600, grid2kLow	= 6550;
			float grid1kHigh	= 6450, grid1kLow	= 6430;
			float grid500mHigh	= 6400, grid500mLow = 6395;
			float grid250mHigh	= 6390, grid250mLow = 6385;

			float g4kT	= MathUtil.Clamp(((float)camDist - grid4kHigh)		/ (grid4kLow - grid4kHigh),		0.0f, 1.0f);
			float g2kT	= MathUtil.Clamp(((float)camDist - grid2kHigh)		/ (grid2kLow - grid2kHigh),		0.0f, 1.0f);
			float g1kT	= MathUtil.Clamp(((float)camDist - grid1kHigh)		/ (grid1kLow - grid1kHigh),		0.0f, 1.0f);
			float g500T = MathUtil.Clamp(((float)camDist - grid500mHigh)	/ (grid500mLow - grid500mHigh), 0.0f, 1.0f);
			float g250T = MathUtil.Clamp(((float)camDist - grid250mHigh)	/ (grid250mLow - grid250mHigh), 0.0f, 1.0f);

			g4kT	-= g2kT;
			g2kT	-= g1kT;
			g1kT	-= g500T;
			g500T	-= g250T;

			LinesGrid4km.TransparencyMultiplayer	= g4kT;
			LinesGrid2km.TransparencyMultiplayer	= g2kT;
			LinesGrid1km.TransparencyMultiplayer	= g1kT;
			LinesGrid500m.TransparencyMultiplayer	= g500T;
			LinesGrid250m.TransparencyMultiplayer	= g250T;
		}
	}
}
