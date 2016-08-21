using System;
using System.Collections.Generic;
using City.ControlsClient;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace City.Panel
{
    public class GisPanel : PulsePanel
    {
        protected PolyGisLayer GisMap { get; set; }
        protected TilesGisLayer TileMap { get; set; }

	    public ViewFrame Frame { get; set; }

		protected LinesGisLayer LinesGrid4km { get; set; }
        protected LinesGisLayer LinesGrid2km { get; set; }
        protected LinesGisLayer LinesGrid1km { get; set; }
        protected LinesGisLayer LinesGrid500m { get; set; }
        protected LinesGisLayer LinesGrid250m { get; set; }

        public GisPanel(Game e, RenderLayer vl, ViewFrame frame) : base(e, vl)
        {
	        Frame = frame;
        }

        public override void Initialize()
        {
            //GisMap = PolyGisLayer.CreateFromUtmFbxModel(GameEngine, "mapspb_342631_6664090_36N");
            //GisMap.Flags = (int)(PolyGisLayer.PolyFlags.VERTEX_SHADER | PolyGisLayer.PolyFlags.PIXEL_SHADER | PolyGisLayer.PolyFlags.XRAY);
            //GisMap.IsVisible = true;
            //GisMap.ZOrder = 900;

            //TileMap = new TilesGisLayer(GameEngine, PanelLayer.GlobeCamera);
            //TileMap.SetMapSource(TilesGisLayer.MapSource.GoogleSatteliteMap);
            //TileMap.IsVisible = false;

            //DVector2 lbPoint = new DVector2(29.63126, 59.59093);
            //LinesGrid4km = LinesGisLayer.GenerateDistanceGrid(GameEngine, lbPoint, 4, 20, 20, new Color(128, 128, 128, 128));
            //LinesGrid4km.ZOrder = 100;
			//
            //LinesGrid2km = LinesGisLayer.GenerateDistanceGrid(GameEngine, lbPoint, 2, 39, 39, new Color(128, 128, 128, 128));
            //LinesGrid2km.ZOrder = 101;
			//
            //LinesGrid1km = LinesGisLayer.GenerateDistanceGrid(GameEngine, lbPoint, 1, 77, 77, new Color(128, 128, 128, 128));
            //LinesGrid1km.ZOrder = 102;
			//
            //LinesGrid500m = LinesGisLayer.GenerateDistanceGrid(GameEngine, lbPoint, 0.5, 153, 153, new Color(128, 128, 128, 128));
            //LinesGrid500m.ZOrder = 103;
			//
            //LinesGrid250m = LinesGisLayer.GenerateDistanceGrid(GameEngine, lbPoint, 0.250, 305, 305, new Color(128, 128, 128, 128));
            //LinesGrid250m.ZOrder = 104;
			//
            //LinesGrid4km.Flags = LinesGrid2km.Flags = LinesGrid1km.Flags = LinesGrid500m.Flags = LinesGrid250m.Flags = (int)(LinesGisLayer.LineFlags.THIN_LINE | LinesGisLayer.LineFlags.OVERALL_COLOR);
            //LinesGrid4km.OverallColor = LinesGrid2km.OverallColor = LinesGrid1km.OverallColor = LinesGrid500m.OverallColor = LinesGrid250m.OverallColor = new Color4(0.5f, 0.5f, 0.5f, 0.5f);
            
//            PanelLayer.GisLayers.Add(Linesm

            //PanelLayer.GisLayers.Add(TileMap);
            //PanelLayer.GisLayers.Add(GisMap);
        }

        public override void Load()
        {
            foreach (var view in Views)
            {
                foreach (var layer in view.GetLayers())
                {
                    if (layer is SpriteLayerWrapper)
                        PanelLayer.SpriteLayers.Add(layer.GetLayer() as SpriteLayer);
                    else if (layer is GisLayerWrapper)
                        PanelLayer.GisLayers.Add(layer.GetLayer() as Gis.GisLayer);
                }
            }
        }

        public override void Unload()
        {
            //TODO
        }

        public override void Update(GameTime gameTime)
        {
            //TileMap.Update(gameTime);
            //UpdateGrids();

            //if (GameEngine.Keyboard.IsKeyDown(Keys.P))
            //{
            //    Console.WriteLine($"{PanelLayer.GlobeCamera.Yaw}, {PanelLayer.GlobeCamera.Pitch}, {PanelLayer.GlobeCamera.CameraDistance}");
            //}
        }

        protected void UpdateGrids()
        {
            var camera = PanelLayer.GlobeCamera;
            var camDist = camera.CameraDistance;

            float grid4kHigh = 7000, grid4kLow = 6800;
            float grid2kHigh = 6600, grid2kLow = 6550;
            float grid1kHigh = 6450, grid1kLow = 6430;
            float grid500mHigh = 6400, grid500mLow = 6395;
            float grid250mHigh = 6390, grid250mLow = 6385;

            float g4kT = MathUtil.Clamp(((float)camDist - grid4kHigh) / (grid4kLow - grid4kHigh), 0.0f, 1.0f);
            float g2kT = MathUtil.Clamp(((float)camDist - grid2kHigh) / (grid2kLow - grid2kHigh), 0.0f, 1.0f);
            float g1kT = MathUtil.Clamp(((float)camDist - grid1kHigh) / (grid1kLow - grid1kHigh), 0.0f, 1.0f);
            float g500T = MathUtil.Clamp(((float)camDist - grid500mHigh) / (grid500mLow - grid500mHigh), 0.0f, 1.0f);
            float g250T = MathUtil.Clamp(((float)camDist - grid250mHigh) / (grid250mLow - grid250mHigh), 0.0f, 1.0f);

            g4kT -= g2kT;
            g2kT -= g1kT;
            g1kT -= g500T;
            g500T -= g250T;

            LinesGrid4km.TransparencyMultiplayer = g4kT;
            LinesGrid2km.TransparencyMultiplayer = g2kT;
            LinesGrid1km.TransparencyMultiplayer = g1kT;
            LinesGrid500m.TransparencyMultiplayer = g500T;
            LinesGrid250m.TransparencyMultiplayer = g250T;
        }
    }
}
