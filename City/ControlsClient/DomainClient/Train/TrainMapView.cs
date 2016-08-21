using System;
using System.Collections.Generic;
using System.Linq;
using City.Models;
using City.Panel;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;
using City.ModelServer;

namespace City.ControlsClient.DomainClient.Train
{


	public class TrainMapView : AbstractPulseView<TrainControl>
	{
		TilesGisLayer	tiles;
		ControlInfo		config;

		//public	PointsGisLayer	TrainPoint;
		private TextGisLayer	textLayer;
		private DVector2		trainCoords;
		private Dictionary<string, GeoBanner>	CityBanners;
		private SpriteLayer						CityBannersLayer;
		private SpriteLayer		TrainPointLayer;
		private PointsGisLayer	StopPlaces;

		private DiscTexture trainTex;

		Random r = new Random();

		private bool isUIInitialized;

		public bool LockCameraOnTrain { set; get; } = true;

		private Point previousMousePosition;


		public TrainMapView(TrainControl trainControl)
		{
			Control = trainControl;
		}


		public override Frame AddControlsToUI()
		{
			return null;
		}


		protected override void InitializeView()
		{
			StopPlaces = new PointsGisLayer(Game, 100, true) {
				ImageSizeInAtlas = new Vector2(128, 128),
				TextureAtlas	= Game.Content.Load<Texture2D>("Train/station_circle"),
				SizeMultiplier	= 1.0f,
				ZOrder			= 1600,
				IsVisible		= true
			};
			StopPlaces.Flags = (int)(PointsGisLayer.PointFlags.DOTS_WORLDSPACE);
			Layers.Add(new GisLayerWrapper(StopPlaces));
			

			///////////////////// City banners //////////////////////////////////////
			CityBannersLayer	= new SpriteLayer(Game.RenderSystem, 500) {
				Order = 100
			};

			TrainPointLayer = new SpriteLayer(Game.RenderSystem, 500) {
				Order = 200
			};

			trainTex = Game.Content.Load<DiscTexture>("Train/trainMarker");

			Control.OnTrainDepart += Control_OnTrainDepart;
			Control.OnTrainArrive +=
				place => { if (place == Control.CurrentRoute.Schedule.StopsPlacesList.Last()) Control_OnTrainDepart(place); }; 

			isUIInitialized = false;
		}

		private void Control_OnTrainDepart(StopPlace city)
		{
			if (CityBanners == null) return;
			if (!CityBanners.ContainsKey(city.Title)) return;
			var banner		= CityBanners[city.Title];

//		    var stopStation = Control.CurrentRoute.Schedule.StopsPlacesList.First(s => s.ArrivalTime >= Control.CurrentTime && s.DepartureTime - new TimeSpan(0, 2, 0) <= Control.CurrentTime);
//		    var stopTime = stopStation.ArrivalTime - stopStation.DepartureTime;
//		    var infection = Int32.Parse(Control.ControlConfig.Scenario.TakeLast(1).ToString()); //TODO hack: take the last schar of scenario name, which depicts the infection type is sirius demo
//		    var count = r.Next(2, 5) + (stopTime.TotalMinutes / 2 + 10) * (double)infection /2;
		    var count = r.Next(5, 15);

            var response	= DiseaseMacroClient.GetServerRespond(city.CityName, "smallpox", count, 30);

			banner.InitTime		= Control.CurrentTime;
			banner.DiseaseData	= response;
        }

		protected override void LoadView(ControlInfo controlInfo)
		{
			if (!isUIInitialized) {
				var ui = ((CustomGameInterface)Game.GameInterface).ui;
				var frame = (Panel as GisPanel).Frame;
				frame.Add(CreateMapControls(ui, frame.Width - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize, frame.Height - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize * 5));

				///////////////////////// Initialize Tiles /////////////////////////////////////
				tiles = new TilesGisLayer(Game, ViewLayer.GlobeCamera) { IsVisible = true };
				tiles.SetMapSource(TilesGisLayer.MapSource.Dark);
				tiles.ZOrder = 5000;
				Layers.Add(new GisLayerWrapper(tiles));

				ViewLayer.SpriteLayers.Add(CityBannersLayer);
				ViewLayer.SpriteLayers.Add(TrainPointLayer);

				isUIInitialized = true;
			}

			//Game.Touch.Tap			+= OnTap;
			Game.Touch.Manipulate	+= OnManipulate;

            config = controlInfo;

			///////////////////////// Initialize camera /////////////////////////////////////
			ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 23;
			ViewLayer.GlobeCamera.Yaw	= DMathUtil.DegreesToRadians(Control.CurrentRoute.Schedule.StopsPlacesList.First().Longtitude);
			ViewLayer.GlobeCamera.Pitch = -DMathUtil.DegreesToRadians(Control.CurrentRoute.Schedule.StopsPlacesList.First().Latitude);

			ViewLayer.GlobeCamera.CameraState = GlobeCamera.CameraStates.ViewToPoint;

			ViewLayer.GlobeCamera.Parameters.MinCameraDistance	= GeoHelper.EarthRadius + 23;
			ViewLayer.GlobeCamera.Parameters.MaxCameraDistance	= GeoHelper.EarthRadius + Control.CurrentRoute.MaxCameraDistance;

			ViewLayer.GlobeCamera.Parameters.MinViewToPointPitch = -Math.PI/20.0;


			CityBanners	= new Dictionary<string, GeoBanner>();
			var places	= Control.CurrentRoute.Schedule.StopsPlacesList;
			for (int i = 0; i < places.Length; i++) {
				StopPlace t = places[i];

				var stationInd	= Control.CurrentRoute.TrainStationsIndeces[i];
				var point		= DMathUtil.DegreesToRadians(Control.CurrentRoute.RouteGeoPoints[stationInd]);

				CityBanners[t.Title] = new GeoBanner(Game, point, t.Title) {InitTime = t.ArrivalTime};

				StopPlaces.PointsCpu[i] = new Gis.GeoPoint {
					Lon = point.X,
					Lat = point.Y,
					Color	= new Color(255, 242, 0, 255),
					Tex0	= new Vector4(0.0f, 0.0f, 1.0f, 0.0f)
				};
            }

			StopPlaces.PointsCountToDraw = places.Length;
			StopPlaces.UpdatePointsBuffer();

			Layers.Add(new GisLayerWrapper(Control.CurrentRoute.LineLayer));
            Layers.Add(new GisLayerWrapper(Control.CurrentRoute.PolyRouteLayer));
		}


		//public void OnTap(TouchEventArgs p)
		//{
		//	var f = ConstantFrameUI.GetHoveredFrame((Panel as GisPanel).Frame, p.Position);
		//	if (f != null) {
		//		f.OnClick(Keys.LeftButton, false);
		//	}
		//}


		public void OnManipulate(TouchEventArgs p)
		{
			var f = ConstantFrameUI.GetHoveredFrame((Panel as GisPanel).Frame, p.Position);
			if (f != null) {
				ViewLayer.GlobeCamera.CameraZoom(MathUtil.Clamp(1.0f - p.ScaleDelta, -0.3f, 0.3f));

				if (p.IsEventEnd) return;

				if (p.IsEventBegin)
					previousMousePosition = p.Position;

				if (LockCameraOnTrain)
					ViewLayer.GlobeCamera.RotateViewToPointCamera((Vector2)p.Position - (Vector2)previousMousePosition);
				else ViewLayer.GlobeCamera.MoveCamera(previousMousePosition, p.Position);

				previousMousePosition = p.Position;
			}
		}


		protected override ICommandSnapshot UpdateView(GameTime gameTime)
		{
			trainCoords = DMathUtil.DegreesToRadians(Control.CurrentRoute.GetPointByTime(Control.CurrentTime));

			//LockCameraOnTrain = Game.Keyboard.IsKeyDown(Keys.Space);

			var min		= ViewLayer.GlobeCamera.Parameters.MinCameraDistance;
			var max		= ViewLayer.GlobeCamera.Parameters.MaxCameraDistance;
			var dist	= ViewLayer.GlobeCamera.CameraDistance;

			StopPlaces.SizeMultiplier = MathUtil.Lerp(0.4f, 12.0f, (float) ((dist - min)/(max - min)));


			if (LockCameraOnTrain) {
				ViewLayer.GlobeCamera.Yaw = trainCoords.X;
				ViewLayer.GlobeCamera.Pitch = -trainCoords.Y;

				lockButton.BackColor = lockButton.HoverColor;

				ViewLayer.GlobeCamera.Update(gameTime);
            }
			else lockButton.BackColor = lockButton.DefaultBackColor;

			tiles.Update(gameTime);

			foreach(var ban in CityBanners) {
				ban.Value.Update(Control.CurrentTime);
			}

			CityBannersLayer.Clear();
			if(!LockCameraOnTrain) {
				var visibleList = new List<Rectangle>();
				var first	= CityBanners.Values.First();
				var last	= CityBanners.Values.Last();

				visibleList.Add(first.GetRectangle(ViewLayer.GlobeCamera));
				visibleList.Add(last.GetRectangle(ViewLayer.GlobeCamera));

				first.Draw(ViewLayer.GlobeCamera, CityBannersLayer);
				last.Draw(ViewLayer.GlobeCamera, CityBannersLayer);

				foreach (var ban in CityBanners.Values) {
					var		banRec	= ban.GetRectangle(ViewLayer.GlobeCamera);
					bool	draw	= true;
                    foreach (var rec in visibleList) {
						if(rec.Intersects(banRec)) {
							draw = false;
						}
					}
					if (draw) {
						visibleList.Add(banRec);
                        ban.Draw(ViewLayer.GlobeCamera, CityBannersLayer);
					}
				}
			} else {
				var cities = Control.CurrentRoute.GetNearestCities(Control.CurrentTime);
				foreach (var city in cities)
					CityBanners[city.Title].Draw(ViewLayer.GlobeCamera, CityBannersLayer);
            }

			//if (Game.Keyboard.IsKeyDown(Keys.D0)) {
			//	Console.WriteLine("Yaw:		" + ViewLayer.GlobeCamera.Yaw);
			//	Console.WriteLine("Pitch:	" + ViewLayer.GlobeCamera.Pitch);
			//	Console.WriteLine("Distance: " + ViewLayer.GlobeCamera.CameraDistance);
			//}

			TrainPointLayer.Clear();
			var cart	= GeoHelper.SphericalToCartesian(trainCoords);
			var screen	= ViewLayer.GlobeCamera.CartesianToScreen(cart);

			TrainPointLayer.Draw(trainTex, new Rectangle((int)screen.X - (trainTex.Width/4), (int)screen.Y - (trainTex.Height/4), trainTex.Width/2, trainTex.Height/2), new Color(255,255,255, 200));

			return null;
		}


		protected override void UnloadView()
		{
			CityBannersLayer.Clear();
			TrainPointLayer.Clear();
			StopPlaces.PointsCountToDraw = 0;

			//Game.Touch.Tap			-= OnTap;
			Game.Touch.Manipulate	-= OnManipulate;
        }

		private Button lockButton;
        private Frame CreateMapControls(FrameProcessor ui, int x, int y)
        {
            var listButton = new ListBox(ui, x, y, 0, 0, "", ColorConstant.trainBackColor) {
                Anchor		= FrameAnchor.Right | FrameAnchor.Bottom
            };
            var textureMaxScale = Game.Content.Load<DiscTexture>(@"ui\map-btns-maxscale");
            var textureLock		= Game.Content.Load<DiscTexture>(@"ui\map-btns-lockview");
			var texturePointMap = Game.Content.Load<DiscTexture>(@"ui\map-btns-zoom2train");
			var texturePlus		= Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomin");
			var textureMinus	= Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomout");

			var backColor = new Color(30, 37, 43, 128);


			var routeButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureMaxScale, texturePointMap.Width, texturePointMap.Height, Color.White,
			() => {
				ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + Control.CurrentRoute.MaxCameraDistance;
                ViewLayer.GlobeCamera.Yaw	= Control.CurrentRoute.FullRouteViewYaw;
				ViewLayer.GlobeCamera.Pitch = Control.CurrentRoute.FullRouteViewPitch;
			});
			(routeButton as Button).DefaultBackColor = backColor;
	        listButton.addElement(routeButton);


			listButton.addElement(lockButton = (Button)FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureLock, textureLock.Width, textureLock.Height, Color.White, () => {
	            if (LockCameraOnTrain) {
		            LockCameraOnTrain = false;
		            ViewLayer.GlobeCamera.CameraState = GlobeCamera.CameraStates.TopDown;
	            }
	            else {
		            LockCameraOnTrain = true;
					ViewLayer.GlobeCamera.CameraState = GlobeCamera.CameraStates.ViewToPoint;
				} }));

			(lockButton as Button).DefaultBackColor = backColor;


			var pointButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", texturePointMap, texturePointMap.Width, texturePointMap.Height, Color.White,
				() => {
					ViewLayer.GlobeCamera.Yaw = trainCoords.X;
					ViewLayer.GlobeCamera.Pitch = -trainCoords.Y;
			});
			(pointButton as Button).DefaultBackColor = backColor;
			listButton.addElement(pointButton);


			var plusButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", texturePlus, texturePlus.Width, texturePlus.Height, Color.White, () => { ViewLayer.GlobeCamera.CameraZoom(-0.3f); });
			(plusButton as Button).DefaultBackColor = backColor;
			listButton.addElement(plusButton);

			var minusButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureMinus, textureMinus.Width, textureMinus.Height, Color.White, () => { ViewLayer.GlobeCamera.CameraZoom(0.3f); });
			(minusButton as Button).DefaultBackColor = backColor;
			listButton.addElement(minusButton);

			return listButton;
        }

    }
}
