using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public class InstagramControl : AbstractPulseControl
    {
        public List<InstagramPost>[] PostsGroupByHour { get; set; }


        //protected InstagramView view;

        protected override void InitializeControl()
        {
//            view = new ;
            Views[1] = new InstagramView(this);
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            PostsGroupByHour = new List<InstagramPost>[24];
            for (int i = 0; i < PostsGroupByHour.Length; i++)
            {
                PostsGroupByHour[i] = new List<InstagramPost>();
            }

            var postsReader = File.ReadAllLines(dir + @"SaintPetersburg\instagram_dataset.csv").Skip(1);
            foreach (var p in postsReader)
            {
                var rawPost = p.Split(';');
                var post = new InstagramPost
                {
                    Id = rawPost[0],
                    TimeStamp = DateTime.Parse(rawPost[1]),
                    Url = rawPost[2],
                    Lat = Double.Parse(rawPost[3], CultureInfo.InvariantCulture),
                    Lon = Double.Parse(rawPost[4], CultureInfo.InvariantCulture)
                };
                int arrayId = post.TimeStamp.Hour;
                PostsGroupByHour[arrayId].Add(post);
            }

            for (int i = 0; i < PostsGroupByHour.Length; i++)
            {
                PostsGroupByHour[i] = PostsGroupByHour[i].OrderBy((x) => x.TimeStamp.TimeOfDay).ToList();
            }
        }

        protected override void UpdateControl(GameTime gameTime)
        {
        }

        protected override bool ValidateSnapshot()
        {
            return true;
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
        }

        public override void UnloadControl()
        {
        }

        public override string UserInfo()
        {
            return "Instagram control info string";
        }
    }
}