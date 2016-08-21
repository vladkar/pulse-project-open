using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.FrameElement;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace City.ControlsClient.DomainClient
{
    internal class SurveillanceCameraClient : AbstractPulseClient
    {
        public List<FakeSurveillanceCamera> Cameras { get; set; }
        private PointsGisLayer globePosts;
        public bool IsDownMouse { get; set; }
        private VideoFrame videoFrame;

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
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            var rnd = new Random();
            
            var cams = Directory.EnumerateFiles(dir + @"SaintPetersburg\surv-cameras").Select(f => new FakeSurveillanceCamera
            {
                FileName = f,
                Lat = 59.932689 + rnd.NextDouble() * 0.05,
                Lon = 30.347757 + rnd.NextDouble() * 0.05
            }).ToList<FakeSurveillanceCamera>();

            Cameras = cams;
            globePosts = new PointsGisLayer(Game, Cameras.Count)
            {
                ImageSizeInAtlas = new Vector2(512, 512),
                TextureAtlas = Game.Content.Load<Texture2D>("camera_icon.tga"),
            };

            for (int i = 0; i < Cameras.Count; i++)
            {
                var post = Cameras[i];

                globePosts.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(post.Lon),
                    Lat = DMathUtil.DegreesToRadians(post.Lat),
                    Color = ColorConstant.Orange,
                    Tex0 = new Vector4(0, 0, 1.5f, 0.0f)
                };
            }

            globePosts.UpdatePointsBuffer();

            GisLayers.Add(globePosts);
        }

        public override void UnloadLevel()
        {
            base.UnloadLevel();
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            if (Game.Keyboard.IsKeyDown(Keys.LeftButton) && !IsDownMouse)
            {
                IsDownMouse = true;
                onMouseClick();
            }
            if (Game.Keyboard.IsKeyUp(Keys.LeftButton))
            {
                IsDownMouse = false;
            }
            if (videoFrame?.geoPoint != null)
                updateCoorVideo();
            return null;
        }

        

        public override string UserInfo()
        {
            return $"Surveillance cameras count: {Cameras.Count}";
        }

        public void onMouseClick()
        {
            if (!globePosts.IsVisible) return;
            DVector2 mousePosition;
            ViewLayers.First().GlobeCamera.ScreenToSpherical(Game.Mouse.Position.X, Game.Mouse.Position.Y, out mousePosition);

            for (int i = 0; i < globePosts.PointsCount; i++)
            {
                var videoCamera = globePosts.PointsCpu[i];
                var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(videoCamera.Lon, videoCamera.Lat));

                if (distance < videoCamera.Tex0.Z)
                {
                    Log.Message("" + Cameras[i].FileName);
                    if (!(Game.GameInterface is CustomGameInterface))
                        return;
                    var ui = ((CustomGameInterface)Game.GameInterface).ui;
                    videoFrame = FrameHelper.createVideoFrame(ui, Cameras[i].FileName, videoCamera);
                    ui.RootFrame.Add(videoFrame);
                }
            }
        }

        private void updateCoorVideo()

        {
            var cartesianCoor = GeoHelper.SphericalToCartesian(new DVector2(videoFrame.geoPoint.Value.Lon, videoFrame.geoPoint.Value.Lat),

                    ViewLayers.First().GlobeCamera.EarthRadius);
            var screenPosition = ViewLayers.First().GlobeCamera.CartesianToScreen(cartesianCoor);
            videoFrame.Image = videoFrame.getTexture();
            videoFrame.X = (int) screenPosition.X;
            videoFrame.Y = (int) screenPosition.Y;
        }
    }
}