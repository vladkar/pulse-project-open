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
    public class VKSentControl : AbstractPulseControl
    {
        public List<InstagramPost> PostsInst { get; set; }
        public List<InstagramPost> PostsVK { get; set; }
        public List<InstagramPost> PostsTwitter { get; set; }


        //protected InstagramView view;

        protected override void InitializeControl()
        {
//            view = new ;
            Views[1] = new VKSentView(this);
		}

		private static DateTime ConvertFromUnixTimestamp(double timestamp)
		{
			DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return origin.AddMilliseconds(timestamp);
		}

		protected override void LoadControl(ControlInfo controlInfo)
        {
            var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
            PostsVK = new List<InstagramPost>();
			PostsInst = new List<InstagramPost>();
			PostsTwitter = new List<InstagramPost>();
			int id = 0;


			//READ VK POST INFO
			var postsReaderVKPost = File.ReadAllLines(dir + @"\Sochi\vk_posts_sochi_users_filtered_with_polarity.csv").Skip(1);
			Console.WriteLine(dir);
			foreach (var p in postsReaderVKPost)
			{
				var rawPost = p.Split(',');
				var post = new InstagramPost
				{
					Id = rawPost[0],
					Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
					Lat = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
					TimeStamp = ConvertFromUnixTimestamp(Double.Parse(rawPost[3]) * 1000),
					Url = rawPost[4],
					Likes = rawPost[5],
					Polarity = float.Parse(rawPost[6]),
					Text = "",
					SocialNetworkId = 0,
					User_Positive = 0,
					User_Neutral = 0,
					User_Negative = 0,
				};
				int arrayId = post.TimeStamp.Hour;
				PostsVK.Add(post);
			}
			id = 0;
			var postsReaderVKPostText= File.ReadAllLines(dir + @"\Sochi\vk_posts_sochi_users_filtered_with_polarity.TEXT.csv").Skip(1);
			foreach (var p in postsReaderVKPostText)
			{
				PostsVK[id].Text = p;
				id++;
			}

			//READ VK PHOTO INFO
			var postsReaderVKPhoto = File.ReadAllLines(dir + @"\Sochi\vk_photos_with_emotions.csv").Skip(1);
			foreach (var p in postsReaderVKPhoto)
			{
				var rawPost = p.Split(',');
				var post = new InstagramPost
				{
					Id = rawPost[0],
					Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
					Lat = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
					TimeStamp = ConvertFromUnixTimestamp(Double.Parse(rawPost[3]) * 1000),
					Url = rawPost[4],
					Likes = "нет данных",
					Polarity = 0,
					Text = "",
					SocialNetworkId = 0,
					Happiness = float.Parse(rawPost[11], NumberStyles.Float),
					Sadness = float.Parse(rawPost[7], NumberStyles.Float) + float.Parse(rawPost[8], NumberStyles.Float)
								+ float.Parse(rawPost[9], NumberStyles.Float) + float.Parse(rawPost[10], NumberStyles.Float)
								+ float.Parse(rawPost[13], NumberStyles.Float),
					Neutral = float.Parse(rawPost[12], NumberStyles.Float) + float.Parse(rawPost[14], NumberStyles.Float),
					User_Positive = 0,
					User_Neutral = 0,
					User_Negative = 0,
				};
				int arrayId = post.TimeStamp.Hour;
				PostsVK.Add(post);
			}
			var postsReaderVKPhotoText = File.ReadAllLines(dir + @"\Sochi\vk_photos.TEXT.csv").Skip(1);
			foreach (var p in postsReaderVKPhotoText)
			{
				PostsVK[id].Text = p;
				id++;
			}

			id = 0;
			var postsReaderVKUserValue = File.ReadAllLines(dir + @"\Sochi\vk_posts_sochi_users.csv").Skip(1);
			foreach (var p in postsReaderVKUserValue)
			{
				var rawPost = p.Split(',');
				PostsVK[id].User_Positive = int.Parse(rawPost[0]);
				PostsVK[id].User_Neutral = int.Parse(rawPost[1]);
				PostsVK[id].User_Negative = int.Parse(rawPost[2]);
				id++;
			}


			//READ Instagram INFO
			var postsReaderInst = File.ReadAllLines(dir + @"\Sochi\instagram_posts_with_polarity_and_emotions.csv").Skip(1);
			foreach (var p in postsReaderInst)
			{
				var rawPost = p.Split(',');
				var post = new InstagramPost
				{
					Id = rawPost[0],
					Lon = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
					Lat = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
					TimeStamp = ConvertFromUnixTimestamp(Double.Parse(rawPost[3]) * 1000),
					Url = rawPost[4],
					Likes = rawPost[5],
					Polarity = float.Parse(rawPost[6]),
					Text = "",
					
					SocialNetworkId = 1,
					Happiness = float.Parse(rawPost[11], NumberStyles.Float),
					Sadness = float.Parse(rawPost[7], NumberStyles.Float) + float.Parse(rawPost[8], NumberStyles.Float) 
								+ float.Parse(rawPost[9], NumberStyles.Float) + float.Parse(rawPost[10], NumberStyles.Float) 
								+ float.Parse(rawPost[13], NumberStyles.Float),
					Neutral = float.Parse(rawPost[12], NumberStyles.Float) + float.Parse(rawPost[14], NumberStyles.Float),
					User_Positive = 0,
					User_Neutral = 0,
					User_Negative = 0,
				};
				PostsInst.Add(post);
			}

			id = 0;
			var postsReaderInstText = File.ReadAllLines(dir + @"\Sochi\instagram_posts_with_polarity.TEXT.csv").Skip(1);
			foreach (var p in postsReaderInstText)
			{				
				PostsInst[id].Text = p;
				id++;          
			}

			id = 0;
			var postsReaderInstagramUserValue = File.ReadAllLines(dir + @"\Sochi\instagram_posts_users.csv").Skip(1);
			foreach (var p in postsReaderInstagramUserValue)
			{
				var rawPost = p.Split(',');
				PostsInst[id].User_Positive = int.Parse(rawPost[0]);
				PostsInst[id].User_Neutral = int.Parse(rawPost[1]);
				PostsInst[id].User_Negative = int.Parse(rawPost[2]);
				id++;
			}

			//READ Twitter INFO
			var postsReaderTwitter = File.ReadAllLines(dir + @"\Sochi\twitter_sochi_tweets_with_polarity.csv").Skip(1);
			foreach (var p in postsReaderTwitter)
			{
				var rawPost = p.Split(',');
				string url = rawPost[4];
				var post = new InstagramPost
				{
					Id = rawPost[0],
					Lon = Double.Parse(rawPost[2], CultureInfo.InvariantCulture),
					Lat = Double.Parse(rawPost[1], CultureInfo.InvariantCulture),
					TimeStamp = ConvertFromUnixTimestamp(Double.Parse(rawPost[3]) * 1000),
					Url = url.Equals("no") ? null : url,
					Likes = rawPost[5],
					Polarity = float.Parse(rawPost[6]),
					Text = "",

					//Place = rawPost[8],
					SocialNetworkId = 2,
					User_Positive = 0,
					User_Neutral = 0,
					User_Negative = 0,
				};
				PostsTwitter.Add(post);
			}

			id = 0;
			var postsReaderTwitterText = File.ReadAllLines(dir + @"\Sochi\twitter_sochi_tweets_with_polarity.TEXT.csv").Skip(1);
			foreach (var p in postsReaderTwitterText)
			{
				PostsTwitter[id].Text = p;
				id++;
			}

			id = 0;
			var postsReaderTwitterUserValue = File.ReadAllLines(dir + @"\Sochi\twitter_sochi_tweets_users.csv").Skip(1);
			foreach (var p in postsReaderTwitterUserValue)
			{
				var rawPost = p.Split(',');
				PostsTwitter[id].User_Positive = int.Parse(rawPost[0]);
				PostsTwitter[id].User_Neutral = int.Parse(rawPost[1]);
				PostsTwitter[id].User_Negative = int.Parse(rawPost[2]);
				id++;
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