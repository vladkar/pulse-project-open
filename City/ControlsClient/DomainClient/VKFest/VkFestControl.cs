using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using City.ControlsClient.DomainClient.VKFest.JsonReader;
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
using Newtonsoft.Json;

namespace City.ControlsClient.DomainClient
{
    public class VkFestControl : AbstractPulseControl {

        private string dir;

        public List<Post> PostsVK { get; set; }
        public List<Post> newPostsVK { get; set; }

        // time in seconds
        private float time;
        private int timeLastRequest;
        private int periodRequest=10;
        private int countPostForRequest = 20;

        protected override void InitializeControl()
        {
            Views[1] = new VkFestView(this);
		}

		protected override void LoadControl(ControlInfo controlInfo)
		{
            dir = (Game.GameClient as PulseMasterClient).Config.DataDir + @"\VkFest\";
            PostsVK = new List<Post>();

            //READ VK Post
            //		    var postsReaderVKPost = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText(dir + "posts.json"));
            //            PostsVK = new List<Post>();
            //            foreach (var line in File.ReadAllLines(dir + "posts.csv"))
            //		    {
            //		        var postLine = line.Split(',');
            //                PostsVK.Add(new Post()
            //                {
            //                    geo = new Geo()
            //                    {
            //                        coordinates = postLine[1] + " " + postLine[2],
            //                    }
            //                });
            //            }
            string[] filePaths = Directory.GetFiles(dir, "*.json");
		    foreach (var file in filePaths)
		    {
		        var rootObject = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText(file));
                if(rootObject?.posts!=null)
                    PostsVK.AddRange(rootObject.posts);
		    }
		    timeLastRequest = PostsVK.Count > 0 ? PostsVK[PostsVK.Count-1].date : 0;
//            var postsReaderVKPost = ControllerReader.GetPostsAndSaveFile(dir + timeLastRequest + ".json", timeLastRequest, 10);
//		    if (postsReaderVKPost?.posts != null && postsReaderVKPost.posts.Count > 0)
//		    {
//                PostsVK = postsReaderVKPost.posts;
//                timeLastRequest += periodRequest;
//            }
		}

        
		protected override void UpdateControl(GameTime gameTime)
		{
		    if (time > periodRequest)
		    {
		        var postsReaderVKPost = ControllerReader.GetPostsAndSaveFile(dir + timeLastRequest + ".json", timeLastRequest, countPostForRequest);
		        if (postsReaderVKPost?.posts != null && postsReaderVKPost.posts.Count > 0)
		        {					
					newPostsVK = postsReaderVKPost.posts;
                    timeLastRequest = newPostsVK.OrderByDescending(x => x.date).First().date + 1;
		        }
		        time -= periodRequest;
		    }
		    time += gameTime.ElapsedSec;
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