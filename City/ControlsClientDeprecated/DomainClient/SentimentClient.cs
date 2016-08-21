using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;

namespace City.ControlsClient.DomainClient
{
    public class SentimentClient : AbstractPulseClient
    {
        public List<SentimentPost> Posts { get; set; }
        private PointsGisLayer globePosts;
        public Color positive = new Color(ColorConstant.ActiveElement.ToColor3(), 0.55f);
        public Color neutral = new Color(50, 50, 50, 150);
        public Color negative = ColorConstant.Crimson;

        protected override void InitializeControl()
        {
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            base.LoadLevel(controlInfo);
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;

            var posts = File.ReadAllLines(dir + @"SaintPetersburg\sentiment_dataset.csv").Skip(1).Select(a =>
            {
                var rawPost = a.Split(';');
                return new SentimentPost
                {
                    Id = rawPost[0],
                    Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
                    Lat = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
                    SentimentValue = Int32.Parse(rawPost[3])
                };
            }).ToList();

            Posts = posts;

            globePosts = new PointsGisLayer(Game, Posts.Count)
            {
                //ImageSizeInAtlas = new Vector2(256, 256),
                //TextureAtlas = Game.Content.Load<Texture2D>("gradient_point.tga"),
                ImageSizeInAtlas = new Vector2(512, 512),
                TextureAtlas = Game.Content.Load<Texture2D>("big_circle.tga"),
            };

            for (int i = 0; i < Posts.Count; i++)
            {
                var post = Posts[i];
                Color4 color = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
                switch (post.SentimentValue)
                {
                    case (0):
                        color = neutral;
                        break;
                    case (1):
                        color = positive;
                        break;
                    case (-1):
                        color = negative;
                        break;
                    default:
                        break;
                }
                globePosts.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(post.Lon),
                    Lat = DMathUtil.DegreesToRadians(post.Lat),
                    Color = color,
                    Tex0 = new Vector4(0, 0, 0.2f, 0.0f)
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
            return null;
        }

        public override string UserInfo()
        {
            return $"Twitter sentiment posts count: {Posts.Count}";
        }

        public override Frame AddControlsToUI()
        {
            if (!(Game.GameInterface is CustomGameInterface))
                return null;
            var ui = ((CustomGameInterface)Game.GameInterface).ui;
            var controlElements = Generator.getControlElement(new SentimentControlUI(globePosts), ui);
            return controlElements;
        }
    }
}