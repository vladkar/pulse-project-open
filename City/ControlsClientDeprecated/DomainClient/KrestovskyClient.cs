using System;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace City.ControlsClient.DomainClient
{
    public class KrestovskyClient : AbstractPulseAgentClient
    {
        private PolyGisLayer _krestMap;
       // private PolyGisLayer _itmoBilds;
        private HeatMapLayer _heatMap;
        private PointsGisLayer _agentsLayer;


        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            base.LoadLevel(controlInfo);

            _krestMap = PolyGisLayer.CreateFromUtmFbxModel(Game, "krest_342631_6664090_36N");
            _krestMap.Flags = (int)(PolyGisLayer.PolyFlags.VERTEX_SHADER | PolyGisLayer.PolyFlags.PIXEL_SHADER | PolyGisLayer.PolyFlags.XRAY);
            _krestMap.IsVisible = true;

            //_itmoBilds = PolyGisLayer.CreateFromUtmFbxModel(Game, "itmoscaled_342631_6664090_36N");
            //_itmoBilds.Flags = (int)(PolyGisLayer.PolyFlags.VERTEX_SHADER | PolyGisLayer.PolyFlags.PIXEL_SHADER | PolyGisLayer.PolyFlags.XRAY);
            //_itmoBilds.IsVisible = true;

            _heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 30.210857, 30.229397, 59.975717, 59.957073, 10, 256, 256, new MercatorProjection());
            _heatMap.MaxHeatMapLevel = 25;
            _heatMap.InterpFactor = 1.0f;
	        _heatMap.IsVisible = true;

			GisLayers.Add(_krestMap);
            //MasterLayer.GisLayers.Add(_itmoBilds);
            GisLayers.Add(_heatMap);

            _agentsLayer = new PointsGisLayer(Game, 50000, false)
            {
                ImageSizeInAtlas	= new Vector2(36, 36),
                TextureAtlas		= Game.Content.Load<Texture2D>("circles.tga")
            };

            GisLayers.Add(_agentsLayer);
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            var s = CurrentSnapShot as PulseSnapshot;
            if (s != null)
            {
				var count = Math.Min(s.Agents.Count, _agentsLayer.PointsCpu.Length);

                for (int i = 0; i < count; i++)
                {
                    _agentsLayer.PointsCpu[i] = new Gis.GeoPoint
                    {
                        Lon = DMathUtil.DegreesToRadians(s.Agents[i].Y),
                        Lat = DMathUtil.DegreesToRadians(s.Agents[i].X),
                        Color = Color.White,
                        Tex0 = new Vector4(0, 0, 0.002f, 3.14f)
                    };

                    _heatMap.AddValue(s.Agents[i].Y, s.Agents[i].X, 10.0f);
                }
                _agentsLayer.UpdatePointsBuffer();
                _agentsLayer.PointsCountToDraw = s.Agents.Count;
                _heatMap.UpdateHeatMap();
            }


//            if (Game.Keyboard.IsKeyDown(Keys.NumPad0))
//            {
//                TileMap.IsVisible = true;
//                GisMap.IsVisible = false;
//                _krestMap.IsVisible = false;
//            //    _itmoBilds.IsVisible = false;
//            }
//            if (Game.Keyboard.IsKeyDown(Keys.NumPad1))
//            {
//                TileMap.IsVisible = false;
//                GisMap.IsVisible = true;
//              //  _krestMap.IsVisible = true;
//             //   _itmoBilds.IsVisible = true;
//            }

            if (Game.Keyboard.IsKeyDown(Keys.NumPad2))
            {
                _krestMap.IsVisible = true;
	            _agentsLayer.IsVisible = true;
	            _heatMap.IsVisible = true;
            }
            if (Game.Keyboard.IsKeyDown(Keys.NumPad3))
            {
                _krestMap.IsVisible = false;
				_agentsLayer.IsVisible = false;
				_heatMap.IsVisible = false;
			}




//			if (GameEngine.Keyboard.IsKeyUp(Keys.I)) {
//				isPressed = false;
//			}
//
//			if (GameEngine.Keyboard.IsKeyDown(Keys.I) && !isPressed) {
//				MasterLayer.GlobeCamera.SaveCurrentStateToFile();
//				isPressed = true;
//			}
//
//			if (GameEngine.Keyboard.IsKeyDown(Keys.L)) {
//				MasterLayer.GlobeCamera.LoadAnimation();
//				MasterLayer.GlobeCamera.ResetAnimation();
//			}
//			if (GameEngine.Keyboard.IsKeyDown(Keys.K)) {
//				MasterLayer.GlobeCamera.StopAnimation();
//			}
//
//			MasterLayer.GlobeCamera.PlayAnimation(gameTime);
//
//
//			if (TileMap.IsVisible)
//				TileMap.Update(gameTime);

			return null;
        }

		bool isPressed = false;


		public override string UserInfo()
        {
            return "";
        }
    }
}
