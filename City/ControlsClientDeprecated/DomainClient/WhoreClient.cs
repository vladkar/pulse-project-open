using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using City.Snapshot;
using City.Snapshot.Snapshot;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Frames;
using Fusion.Engine.Input;

namespace City.ControlsClient.DomainClient
{
    public class WhoreClient : AbstractPulseClient
    {
        public List<WhorePlace> WhorePlaces { get; set; }
        private PointsGisLayer globePosts;

        //gradient
        public Color startColor = new Color(220, 55, 255); //new Color(134, 6, 65); 
        public Color endColor = new Color(255, 240, 0); 
        private float quantil      = 0.97f; // percentage of points involved to gradient

        private Dictionary<string, Vector2> computedValues = new  Dictionary<string, Vector2>();
        private string currentStatus;


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
           var whores = File.ReadAllLines(dir + @"SaintPetersburg\whore_dataset.csv").Skip(0).Select(a =>
            {
                var rawPost = a.Split(' ');
                int value = 0;
                return new WhorePlace
                {
                    Id = rawPost[0],
                    Lat = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
                    Lon = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
                    Age = int.TryParse(rawPost[3], out value) ? value : -1,
                    Height = int.TryParse(rawPost[4], out value) ? value : -1,
                    Weight = int.TryParse(rawPost[5], out value) ? value : -1,
                    TittySize = int.TryParse(rawPost[6], out value) ? value : -1,
                    Price1 = int.TryParse(rawPost[7], out value) ? value : -1,
                    Price2 = int.TryParse(rawPost[8], out value) ? value : -1,
                    Price3 = int.TryParse(rawPost[9], out value) ? value : -1
                };
            }).ToList();

            WhorePlaces = whores;
            
            globePosts = new PointsGisLayer(Game, WhorePlaces.Count)
            {
                ImageSizeInAtlas = new Vector2(512, 512),
                TextureAtlas = Game.Content.Load<Texture2D>("big_circle.tga")
            };

            for (int i = 0; i < WhorePlaces.Count; i++)
            {
                var post = WhorePlaces[i];                
                globePosts.PointsCpu[i] = new Gis.GeoPoint
                {
                    Lon = DMathUtil.DegreesToRadians(post.Lon),
                    Lat = DMathUtil.DegreesToRadians(post.Lat),
                    Color = Color.White,
                    Tex0 = new Vector4(0, 0, 0.11f, 0.0f)
                };
            }
            updateAllDotsColors("Price1");
            GisLayers.Add(globePosts);           
        }

        public override void UnloadLevel()
        {
            base.UnloadLevel();
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
            if (Game.Keyboard.IsKeyDown(Keys.A))                        
                updateAllDotsColors("Age");
            if (Game.Keyboard.IsKeyDown(Keys.W))
                updateAllDotsColors("Weight");
            if (Game.Keyboard.IsKeyDown(Keys.T))
                updateAllDotsColors("TittySize");
            if (Game.Keyboard.IsKeyDown(Keys.H))
                updateAllDotsColors("Height");
            if (Game.Keyboard.IsKeyDown(Keys.P))
                updateAllDotsColors("Price1");
            if (Game.Keyboard.IsKeyDown(Keys.K))
                globePosts.IsVisible = false;
            if (Game.Keyboard.IsKeyDown(Keys.J))
                globePosts.IsVisible = true;
            if (Game.Keyboard.IsKeyDown(Keys.LeftButton))
                onMouseClick();
            return null;
        }
        
        public override string UserInfo()
        {
            return $"You can pay for love at {WhorePlaces.Count} places";
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
        }

        // colored dots by specific field
        private void updateAllDotsColors (string str)
        {
            if (str.Equals(currentStatus)) return;
        
            var property = typeof(WhorePlace).GetProperty(str);            
            int supremum = 0;
            int minimum = 0;

            if (!computedValues.ContainsKey(str))
            {
                 List<int> values = new List<int>();
                 Dictionary<int, int> probability = new Dictionary<int, int>();

                 var maximum = WhorePlaces.Max((x) => (int)property.GetValue(x));
                 minimum = WhorePlaces.Min((x) => ((int)property.GetValue(x) > 0) ? (int)property.GetValue(x) : maximum);

                 WhorePlaces.ForEach((x) => { if (!values.Contains((int)property.GetValue(x))) values.Add((int)property.GetValue(x)); });
                 values.Sort();
                 values.ForEach((x) => probability.Add(x, WhorePlaces.Count((y) => (int)property.GetValue(y) == x)));

                 supremum = 0;
                 float epsilon = 0;
                 foreach (var p in probability)
                 {
                     if (epsilon >= quantil) break;
                     epsilon += (float)p.Value / WhorePlaces.Count;
                     supremum = p.Key;
                 }
                 Log.Message("" + supremum);
                 computedValues.Add(str, new Vector2(minimum, supremum));
            }
            else
            {
                 Vector2 values;
                 computedValues.TryGetValue(str, out values);
                 minimum     = (int) values.X;
                 supremum    = (int) values.Y;
            }                      

            for (int i = 0; i < globePosts.PointsCount; i++)
            {
                 var post = WhorePlaces[i];
                 var value = ((int)property.GetValue(post) > 0) ? (int)property.GetValue(post) : minimum;
                 float alpha = (float)(value - minimum) / (supremum - minimum);
                 alpha = (alpha > 1) ? 1 : alpha;
                 globePosts.PointsCpu[i].Color = endColor * alpha + (1 - alpha) * startColor;
            }
            currentStatus = str;
            Log.Message("status changed: " + currentStatus);
            globePosts.UpdatePointsBuffer();
        }

        //action on mouse
        public void onMouseClick()
        {
            if (!globePosts.IsVisible) return;
            DVector2 mousePosition;
            ViewLayers.First().GlobeCamera.ScreenToSpherical(Game.Mouse.Position.X, Game.Mouse.Position.Y, out mousePosition);

            for (int i = 0; i < globePosts.PointsCount; i++)
            {
                var girl = globePosts.PointsCpu[i];
                var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(girl.Lon, girl.Lat));

                if(distance < girl.Tex0.Z)
                {
                    Log.Message("" + WhorePlaces[i].Id);
                    Log.Message("" + WhorePlaces[i].Age);
                    Log.Message("" + WhorePlaces[i].Price1);
                }
            }
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

        //update alpha, sets one transparency value to all points
        public void updateAlpha(float alpha)
        {
            for (int i = 0; i < globePosts.PointsCount; i++)
            {
                globePosts.PointsCpu[i].Color.Alpha = alpha;
            }
            globePosts.UpdatePointsBuffer();
        }
    }
}