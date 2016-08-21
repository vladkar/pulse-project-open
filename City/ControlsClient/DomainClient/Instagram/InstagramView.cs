using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using City.Panel;
using City.Snapshot.Snapshot;
using City.UIFrames;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;

namespace City.ControlsClient.DomainClient
{
    public class InstagramView : AbstractPulseView<InstagramControl>
    {
        private PointsGisLayer globePosts;
        private HeatMapLayer heatMap;
        private int pointsToDraw = 0;
        private List<InstagramPost> currentPosts;
        private List<Frame> frames;
        bool Pause  {get; set; }
//
        public TimeSpan currentTime { get; set; }
        private float tt = 0;
        float speed = 1.00f;
//
        double  timeStepInSeconds   = 60;
        double  sizeOfMovingWindow  = 20;   // (sizeOfMovingWindow + 2 * timeOfFadeOut) should be less than 60
        int     timeOfFadeOut       = 15;    // (sizeOfMovingWindow + 2 * timeOfFadeOut) should be less than 60

        private TilesGisLayer tiles;

        public InstagramView(InstagramControl instagramControl)
        {
            Control = instagramControl;
        }

        public override Frame AddControlsToUI()
        {
            if (!(Game.GameInterface is CustomGameInterface))
                return null;
            var ui = ((CustomGameInterface)Game.GameInterface).ui;
            //TODO new InstagramUI() -> extrat values
            var controlElements = Generator.getControlElement(new InstagramUI(), ui);
            return controlElements;
        }

        protected override void InitializeView()
        {
        }

        protected override void LoadView(ControlInfo controlInfo)
        {
            ViewLayer.GlobeCamera.Yaw = 0.52893754304863982;
            ViewLayer.GlobeCamera.Pitch = -1.0460873698101123;
            ViewLayer.GlobeCamera.CameraDistance = 6502.8797220490105;

            //            ViewLayer.GlobeCamera.Yaw = 0.527224026069971;
            //            ViewLayer.GlobeCamera.Pitch = -1.04654411416673;
            //            ViewLayer.GlobeCamera.CameraDistance = 6378.3575872918454;

            currentTime = new TimeSpan(22,20,0);
            int maxDots = Control.PostsGroupByHour.Max((x) => x.Count) * 2;
            currentPosts = new List<InstagramPost>();

            globePosts = new PointsGisLayer(Game, maxDots)
            {
                ImageSizeInAtlas    = new Vector2(512, 512),
                TextureAtlas        = Game.Content.Load<Texture2D>("big_circle.tga")
            };
            globePosts.ZOrder = 2000;

            Layers.Add(new GisLayerWrapper(globePosts));

            heatMap = HeatMapLayer.GenerateHeatMapWithRegularGrid(Game, 29.425507, 30.701294, 60.182843, 59.759508, 30, 512, 512, new MercatorProjection());
            heatMap.MaxHeatMapLevel = 3000.01f;
            heatMap.InterpFactor	= 1.0f;
	        heatMap.HeatMapTransparency = 1.0f;
            heatMap.MinHeatMapLevel = -300.0f;
            heatMap.ZOrder = 1900;

            Layers.Add(new GisLayerWrapper(heatMap));
            heatMap.IsVisible = true;

			//Game.Keyboard.KeyUp += (x, y) => { if (y.Key == Keys.LeftButton) onMouseClick(Game.Mouse.Position.X, Game.Mouse.Position.Y); };

			Pause = false;
			frames = new List<Frame>();
		}

        protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {
            if (!Pause)
            {
                tt += gameTime.ElapsedSec;
                if (tt > speed)
                {
                    tt = tt - speed;
                }
                currentTime = currentTime.Add(TimeSpan.FromSeconds(timeStepInSeconds));
                drawPostedPoints(currentTime);
            }
            //Pause = true;
            //if (Game.Keyboard.IsKeyDown(Keys.LeftButton))
            //  onMouseClick();
            if (Game.Keyboard.IsKeyDown(Keys.J))
            {
                frames.ForEach((x) => x.Parent.Remove(x));
            }

            return null;
        }

        protected override void UnloadView()
        {
        }
        
        private void drawPostedPoints(TimeSpan time)
        {
            heatMap.ClearData();
            currentPosts.Clear();
            for (int i = 0; i < globePosts.PointsCount; i++)
            {
                globePosts.PointsCpu[i].Color.Alpha = 0;
            }

            int id = time.Hours;
            double currentMinutes = time.Minutes;
            pointsToDraw = 0;            

            double leftLimit = (currentMinutes - sizeOfMovingWindow / 2 - timeOfFadeOut);
            double rightLimit = (currentMinutes + sizeOfMovingWindow / 2 + timeOfFadeOut);
            int fadeLeft = timeOfFadeOut;
            int fadeRight = timeOfFadeOut;

            if ( (int) rightLimit > 60)
            {
                int hourId = (id + 1) % 24;
                int right = (int) rightLimit - 60;
                fadeRight = 0;
                if (right < timeOfFadeOut) {
                    fadeRight = timeOfFadeOut - right;
                }
                makePointsVisible(hourId, 0, right, 0, timeOfFadeOut);
                rightLimit = 60;
            }

            if ((int) leftLimit < 0 )
            {
                int hourId = (id != 0) ? id - 1 : 23;
                int left = 60 + (int) leftLimit;
                fadeLeft = 0;
                if (Math.Abs(leftLimit) < timeOfFadeOut)
                {
                    fadeLeft = timeOfFadeOut - (int) Math.Abs(leftLimit);
                }
                makePointsVisible(hourId, left, 60, timeOfFadeOut, 0);
                leftLimit = 0;
            }           

            makePointsVisible(id, (int) leftLimit, (int) rightLimit, fadeLeft, fadeRight);  
                                 
            globePosts.UpdatePointsBuffer();                                   
            heatMap.UpdateHeatMap();

           // Log.Message("" + pointsToDraw);
           // Log.Message("" + currentTime);
        }

        private int makePointsVisible(int hour, int leftLimMinutes, int rightLimMinutes, int fadeLeftTime, int fadeRightTime)
        {            
            int drawID = 0;
            int finalPoint = Control.PostsGroupByHour[hour].Count;
            for (int i = 0; i < finalPoint; i++)
            {
                var post = Control.PostsGroupByHour[hour][i];
                var postMinutes = post.TimeStamp.Minute;

                if (postMinutes > rightLimMinutes) break;
                if (postMinutes < leftLimMinutes)
                {
                    if (fadeLeftTime != 0) drawID++;
                    continue;
                };

                float alpha = 1.0f;
                if (fadeLeftTime != 0 && postMinutes  <= fadeLeftTime + leftLimMinutes)
                {
                    alpha = (float)  (postMinutes - leftLimMinutes + (timeOfFadeOut - fadeLeftTime)) / timeOfFadeOut;
                }

                if (fadeRightTime != 0 && postMinutes >= rightLimMinutes - fadeRightTime)
                {
                    alpha = (float) (rightLimMinutes - postMinutes + (timeOfFadeOut - fadeRightTime)) / timeOfFadeOut;
                }

                globePosts.PointsCpu[pointsToDraw] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(post.Lon),
                    Lat = DMathUtil.DegreesToRadians(post.Lat),
                    Color = new Color(ColorConstant.LiteBlue.ToColor3(), alpha),
                    Tex0 = new Vector4(0, 0, 0.11f, 0.0f)
                };
                heatMap.AddValue(post.Lon, post.Lat, 300.0f * alpha);
                currentPosts.Insert(pointsToDraw, post);
                pointsToDraw++;
            }
            //pointsToDraw--;
            return drawID;
        }

        //action on mouse
        public void onMouseClick(int mouseX, int mouseY)
        {
            if (!globePosts.IsVisible) return;
            DVector2 mousePosition;

            ViewLayer.GlobeCamera.ScreenToSpherical(Game.Mouse.Position.X, Game.Mouse.Position.Y, out mousePosition);
            
            for (int i = 0; i < currentPosts.Count; i++)
            {                
                var post = globePosts.PointsCpu[i];
                if (post.Color.Alpha == 0) continue;
                var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));

                if (distance < post.Tex0.Z)
                {                    
                    Log.Message("" + currentPosts.ElementAt(i).TimeStamp);
					Log.Message("" + currentTime);
					//Pause = true;

					if (!(Game.GameInterface is CustomGameInterface))
                        return;
                    var ui = ((CustomGameInterface)Game.GameInterface).ui;

					var url = currentPosts.ElementAt(i).Url;

					var frame = FrameHelper.createWebPhotoFrame(ui, mouseX, mouseY, 150, 150, "inst_load", url, 0, new InstagramPost[1], null);
					frames.Add(frame);
					ui.RootFrame.Add(frame);
					break;
                }
            }
        }

		string GetFileNameFromUrl(string url)
		{
			url = url.Replace(":", "");
			url = url.Replace("/", "");
			url = url.Replace(".", "");
			url = url.Remove(url.Length - 3);

			return url;
		}

		public void changePointsVisibility()
        {
            globePosts.IsVisible = !globePosts.IsVisible;
        }

        public void changeHeatmapVisibility()
        {
            heatMap.IsVisible = !heatMap.IsVisible;
        }

		//update size
		public void updateSize(float size)
		{
			for (int i = 0; i < globePosts.PointsCount; i++)
			{
				globePosts.PointsCpu[i].Tex0.Z = size;
			}
			globePosts.UpdatePointsBuffer();
		}
	}
}