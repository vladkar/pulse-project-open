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
using City.UIFrames.FrameElement;
using City.Panel;
using City.UIFrames.Impl;
using Fusion.Engine.Graphics;

namespace City.ControlsClient.DomainClient
{
    public class VKSentView : AbstractPulseView<VKSentControl>
    {
		private PointsGisLayer globePostsVK;
		private PointsGisLayer globePostsInst;
		private PointsGisLayer globePostsTwitter;

        private List<InstagramPost> currentVKPosts;
        private List<InstagramPost> currentInstPosts;
        private List<InstagramPost> currentTwitterPosts;
        private WebPostFrame webFrame;
		private InstagramToolPanel rightPanel;
		private int maxImageSize = 400;
		private int offset = ConstantFrameUI.gridUnits;
		private Point previousMousePosition;
		private float maxAlpha = 0.4f;
		private Frame legendary_legend;
		private float time = 0;
		private float autoSaveTime = 300;
		private DVector3 targetCoords;
		private DVector3 currentCoords;
		private bool cameraMove = false;
		private bool cameraZoom = false;
		private double desiredDistance;
		private double currentDistance;
		private int timeToSleep = 300;
		private int secondsBeforeGoToBed;
		private bool machine = true;


		private TilesGisLayer tiles;

        public VKSentView(VKSentControl instagramControl)
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
           var ui = ((CustomGameInterface)Game.GameInterface).ui;
			secondsBeforeGoToBed = timeToSleep;

			//Frames
            var frame = (Panel as GisPanel).Frame;
			rightPanel = new InstagramToolPanel(ui);

			frame.Add(rightPanel);
			mapButtons = CreateMapControls(ui, frame.Width - offset * 10 - ConstantFrameUI.mapButtonSize - rightPanel.Width, frame.Height - offset * 8 - offset * 12 * 7);
			frame.Add(mapButtons);

			string disclaimer = "ВНИМАНИЕ! За содержимое постов из соцсетей создатели приложения ответственности не несут!";
			int length = ConstantFrameUI.sfUltraLight32.MeasureString(disclaimer).Width;
			var disclaimer_note = new Frame(ui, (int)(frame.Width - rightPanel.Width - length) / 2, frame.Height - ConstantFrameUI.sfUltraLight32.LineHeight - offset * 8, length, ConstantFrameUI.sfUltraLight32.LineHeight, disclaimer, Color.Zero)
			{
				Anchor = FrameAnchor.Bottom,
				Font = ConstantFrameUI.sfUltraLight32,
				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
			}; 
			
			frame.Add(disclaimer_note);
			CreateIntefaceElements(ui, 0, frame.Height - offset * 24);


			//Map
			ViewLayer.GlobeCamera.CameraDistance = GeoHelper.EarthRadius + 23;
			ViewLayer.GlobeCamera.GoToPlace(GlobeCamera.Places.Sochi);

			ViewLayer.GlobeCamera.Parameters.MinCameraDistance = GeoHelper.EarthRadius + 5;
			ViewLayer.GlobeCamera.Parameters.MaxCameraDistance = GeoHelper.EarthRadius + 110;
			currentCoords = new DVector3( ViewLayer.GlobeCamera.Yaw, ViewLayer.GlobeCamera.Pitch, ViewLayer.GlobeCamera.CameraDistance);
			targetCoords = currentCoords;
			currentDistance = ViewLayer.GlobeCamera.CameraDistance;
			desiredDistance = currentDistance;

			tiles = new TilesGisLayer(Game, ViewLayer.GlobeCamera) { IsVisible = true };
			tiles.SetMapSource(TilesGisLayer.MapSource.Dark);
			tiles.ZOrder = 2000;
			Layers.Add(new GisLayerWrapper(tiles));

			//Touch
			Game.Touch.Tap += p => {
				secondsBeforeGoToBed = timeToSleep;
			};
			Game.Touch.Tap += OnTap;

			Game.Touch.Manipulate += p => {
				secondsBeforeGoToBed = timeToSleep;
			};
			Game.Touch.Manipulate += OnManipulate;

			frame.Click += (sender, args) =>
			{
				if (cameraMove) cameraMove = false;
				if (cameraZoom) cameraZoom = false;
				secondsBeforeGoToBed = timeToSleep;
				if (webFrame != null)
				{
					if (webFrame.sendToHell)
					{
						webFrame.Parent.Remove(webFrame);
						webFrame = null;
					}
				}
				onMouseClick(args.X, args.Y);
			};


			//Data
			//VK
			currentVKPosts = Control.PostsVK;
			globePostsVK = new PointsGisLayer(Game, currentVKPosts.Count)
			{
				ImageSizeInAtlas = new Vector2(128, 128),
				TextureAtlas = Game.Content.Load<Texture2D>("Train/station_circle.tga")
			};
			globePostsVK.ZOrder = 1000;
			int id = 0;
			foreach (var post in currentVKPosts)
			{
				globePostsVK.PointsCpu[id] = new Gis.GeoPoint
				{
					Lon = DMathUtil.DegreesToRadians(post.Lon),
					Lat = DMathUtil.DegreesToRadians(post.Lat),
					Color = new Color(255, 217, 0, maxAlpha),
					Tex0 = new Vector4(0, 0, 0.05f, 0.0f)
				};
				id++;
			}
			globePostsVK.UpdatePointsBuffer();

			//INSTAGRAM
			currentInstPosts = Control.PostsInst;
			globePostsInst = new PointsGisLayer(Game, currentInstPosts.Count)
			{
				ImageSizeInAtlas = new Vector2(128, 128),
				TextureAtlas = Game.Content.Load<Texture2D>("Train/station_circle.tga")
			};
			globePostsInst.ZOrder = 1000;
			id = 0;
			foreach (var post in currentInstPosts)
			{
				globePostsInst.PointsCpu[id] = new Gis.GeoPoint
				{
					Lon = DMathUtil.DegreesToRadians(post.Lon),
					Lat = DMathUtil.DegreesToRadians(post.Lat),
					Color = new Color(255, 217, 0, maxAlpha),
					Tex0 = new Vector4(0, 0, 0.05f, 0.0f)
				};
				id++;
			}
			globePostsInst.UpdatePointsBuffer();

			//TWITTER
			currentTwitterPosts = Control.PostsTwitter;
			globePostsTwitter = new PointsGisLayer(Game, currentTwitterPosts.Count)
			{
				ImageSizeInAtlas = new Vector2(128, 128),
				TextureAtlas = Game.Content.Load<Texture2D>("Train/station_circle.tga")
			};
			globePostsTwitter.ZOrder = 1000;
			id = 0;
			foreach (var post in currentTwitterPosts)
			{
				globePostsTwitter.PointsCpu[id] = new Gis.GeoPoint
				{
					Lon = DMathUtil.DegreesToRadians(post.Lon),
					Lat = DMathUtil.DegreesToRadians(post.Lat),
					Color = new Color(255, 217, 0, maxAlpha),
					Tex0 = new Vector4(0, 0, 0.05f, 0.0f)
				};
				id++;
			}
			globePostsTwitter.UpdatePointsBuffer();
			Layers.Add(new GisLayerWrapper(globePostsVK));
			Layers.Add(new GisLayerWrapper(globePostsInst));
			Layers.Add(new GisLayerWrapper(globePostsTwitter));

			Game.RenderSystem.Fullscreen = true;

			int evaluationNumber = 0;
			int positive_number = 0;
			int neutral_number = 0;
			int negative_number = 0;

			foreach (var post in currentInstPosts)
			{
				positive_number += post.User_Positive;
				neutral_number += post.User_Neutral;
				negative_number += post.User_Negative;
			}
			foreach (var post in currentTwitterPosts)
			{
				positive_number += post.User_Positive;
				neutral_number += post.User_Neutral;
				negative_number += post.User_Negative;
			}
			foreach (var post in currentVKPosts)
			{
				positive_number += post.User_Positive;
				neutral_number += post.User_Neutral;
				negative_number += post.User_Negative;
			}


			evaluationNumber = positive_number + negative_number + neutral_number;
			activity_count.Text = "" + evaluationNumber;
			activity_count.Width = activity_count.Font.MeasureString(activity_count.Text).Width;
		}

		float tt = 0;
		bool help = true;
		protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {       
			var ui = ((CustomGameInterface)Game.GameInterface).ui;
			tiles.Update(gameTime);
			if (rightPanel.needsUpdate)
			{
				changeSocialNetworksVisibility();
				changeSocialNetworksColor();
				colorBySentiment();
				rightPanel.needsUpdate = false;
			}		

			time += gameTime.ElapsedSec;
			if (time > autoSaveTime)
			{
				time = time - autoSaveTime;
				updateData();
			}

			tt += gameTime.ElapsedSec;
			if (tt > 1)
			{
				tt = tt - 1;
				secondsBeforeGoToBed--;
			}

			if(secondsBeforeGoToBed == 0 && help)
			{
				CreateHelpWindow(ui, (Panel as GisPanel).Frame);
				help = false;
			}

			if (cameraMove)
			{
				currentCoords = DVector3.Lerp(currentCoords, targetCoords, gameTime.ElapsedSec * 2.0f);
				ViewLayer.GlobeCamera.Yaw = currentCoords.X;
				ViewLayer.GlobeCamera.Pitch = currentCoords.Y;
				ViewLayer.GlobeCamera.CameraDistance = currentCoords.Z;
				if (currentCoords == targetCoords) cameraMove = false;
			}

			if (cameraZoom)
			{
				currentDistance = DMathUtil.Lerp(currentDistance, desiredDistance, gameTime.ElapsedSec * 5.0f);
				ViewLayer.GlobeCamera.CameraDistance = currentDistance;
				if (currentDistance == desiredDistance) cameraZoom = false;
			}

			return null;
        }

        protected override void UnloadView()
        {
			updateData();
		}

		private void updateData()
		{
			var dir = (Game.GameClient as PulseMasterClient).Config.DataDir;
			FileStream fcreateTwitter = File.Open(dir + @"Sochi/twitter_sochi_tweets_users.csv", FileMode.Create);
			StreamWriter twitter = new StreamWriter(fcreateTwitter);
			twitter.WriteLine("user_positive,user,neutral,user_negative");
			foreach (var post in currentTwitterPosts)
			{
				twitter.WriteLine(post.User_Positive + "," + post.User_Neutral + "," + post.User_Negative);
			}
			twitter.Close();

			FileStream fcreateVK = File.Open(dir + @"Sochi/vk_posts_sochi_users.csv", FileMode.Create);
			StreamWriter vk = new StreamWriter(fcreateVK);
			vk.WriteLine("user_positive,user_negative");
			foreach (var post in currentVKPosts)
			{
				vk.WriteLine(post.User_Positive + "," + post.User_Neutral + "," + post.User_Negative);
			}
			vk.Close();

			FileStream fcreateInst = File.Open(dir + @"Sochi/instagram_posts_users.csv", FileMode.Create);
			StreamWriter inst = new StreamWriter(fcreateInst);
			inst.WriteLine("user_positive,user_negative");
			foreach (var post in currentInstPosts)
			{
				inst.WriteLine(post.User_Positive + "," + post.User_Neutral + "," + post.User_Negative);
			}
			inst.Close();
			return;
		} 
      

        //action on mouse
        public void onMouseClick(int mouseX, int mouseY)
        {
			if (rightPanel.GlobalRectangle.Contains(mouseX, mouseY)) return;
			if (mapButtons.GlobalRectangle.Contains(mouseX, mouseY)) return;
			if (infoButton.GlobalRectangle.Contains(mouseX, mouseY)) return;

			DVector2 mousePosition;
			var ui = ((CustomGameInterface)Game.GameInterface).ui;
			ViewLayer.GlobeCamera.ScreenToSpherical( mouseX , mouseY, out mousePosition);
			var posts = new List<InstagramPost>();

			if (globePostsTwitter.IsVisible)
			{
				for (int i = 0; i < currentTwitterPosts.Count; i++)
				{
					var post = globePostsTwitter.PointsCpu[i];
					if (post.Color.Alpha == 0) continue;
					var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));

					if (distance < post.Tex0.Z * (ViewLayer.GlobeCamera.CameraDistance - ViewLayer.GlobeCamera.EarthRadius) * 0.1f)
					{
						if (!(Game.GameInterface is CustomGameInterface))
							return;
						posts.Add(currentTwitterPosts[i]);
					}
				}
			}

			if (globePostsInst.IsVisible)
			{
				for (int i = 0; i < currentInstPosts.Count; i++)
				{
					var post = globePostsInst.PointsCpu[i];
					if (post.Color.Alpha == 0) continue;
					var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));

					if (distance < post.Tex0.Z * (ViewLayer.GlobeCamera.CameraDistance - ViewLayer.GlobeCamera.EarthRadius) * 0.1f)
					{
						if (!(Game.GameInterface is CustomGameInterface))
							return;
						posts.Add(currentInstPosts[i]);
					}
				}
			}

			if (globePostsVK.IsVisible)
			{
				for (int i = 0; i < currentVKPosts.Count; i++)
				{
					var post = globePostsVK.PointsCpu[i];
					if (post.Color.Alpha == 0) continue;
					var distance = GeoHelper.DistanceBetweenTwoPoints(mousePosition, new DVector2(post.Lon, post.Lat));
					if (distance < post.Tex0.Z * (ViewLayer.GlobeCamera.CameraDistance - ViewLayer.GlobeCamera.EarthRadius) * 0.1f)
					{
						if (!(Game.GameInterface is CustomGameInterface))
							return;

						posts.Add(currentVKPosts[i]);
					}
				}
			}			

			Log.Message(DMathUtil.RadiansToDegrees(mousePosition.X) + "");
			Log.Message(DMathUtil.RadiansToDegrees(mousePosition.Y) + "");
			if (posts.Count > 0)
			{
				Game.Touch.Tap -= OnTap;
				Game.Touch.Manipulate -= OnManipulate;
				var p = (Panel as GisPanel);
				darkPanel = new Button(ui, 0, 0, p.Frame.Width - rightPanel.Width, p.Frame.Height, "", new Color(0, 0, 0, 180))
				{
					HoverColor = new Color(0, 0, 0, 180),
				};
				darkPanel.Click += (c, b) =>
				{
					darkPanel.Parent.Remove(darkPanel);
					if (webFrame != null)
					{
						webFrame.Parent.Remove(webFrame);
						webFrame = null;
						changeSocialNetworksVisibility();
						changeSocialNetworksColor();
						colorBySentiment();
					}
					darkPanel = null;
					Game.Touch.Tap += OnTap;
					Game.Touch.Manipulate += OnManipulate;
				};
				p.Frame.Insert(1, darkPanel);

				webFrame = FrameHelper.createWebPhotoFrame(ui, (int) (p.Frame.Width - rightPanel.Width) / 2, (int) p.Frame.Height / 2, maxImageSize, maxImageSize, "inst_load", posts[0].Url, rightPanel.Width, posts.ToArray(), activity_count);
				p.Frame.Add(webFrame);


			} else
			{
				//if (darkPanel != null)
				//{
				//	darkPanel.Parent.Remove(darkPanel);
				//	darkPanel = null;
				//}
				//if (webFrame != null)
				//{
				//	webFrame.Parent.Remove(webFrame);
				//	webFrame = null;
				//	changeSocialNetworksVisibility();
				//	changeSocialNetworksColor();
				//	colorBySentiment();
				//}
				//Game.Touch.Tap += OnTap;
				//Game.Touch.Manipulate += OnManipulate;
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

		public void changeSocialNetworksVisibility()
        {
			foreach (var c in rightPanel.socialFrameList.Children)
			{
				var checkbox = c as Checkbox;

				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
				{
					case 0:
						globePostsVK.IsVisible = checkbox.IsChecked;
						break;
					case 1:
						globePostsTwitter.IsVisible = checkbox.IsChecked;
						break;
					case 2:
						globePostsInst.IsVisible = checkbox.IsChecked;
						break;
				};

			}
		}

		public void changeSocialNetworksColor()
		{
			foreach (var c in rightPanel.filterColorFrameList.Children)
			{
				var checkbox = c as Checkbox;
				if (!checkbox.IsChecked) continue;
				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
				{
					case 0:
						{			
							for (int i = 0; i < globePostsVK.PointsCount; i++)
							{
								globePostsVK.PointsCpu[i].Color = new Color(255, 217, 0);
								globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							for (int i = 0; i < globePostsInst.PointsCount; i++)
							{
								globePostsInst.PointsCpu[i].Color = new Color(255, 217, 0);
								globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
							{
								globePostsTwitter.PointsCpu[i].Color = new Color(255, 217, 0);
								globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							globePostsVK.UpdatePointsBuffer();
							globePostsInst.UpdatePointsBuffer();
							globePostsTwitter.UpdatePointsBuffer();
							legendary_legend.Visible = false;
							legendImage.Visible = false;			
							machine = true;
							break;
						}
					case 1:
						{
							for (int i = 0; i < globePostsVK.PointsCount; i++)
							{
								globePostsVK.PointsCpu[i].Color = new Color(0, 105, 255);
								globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							for (int i = 0; i < globePostsInst.PointsCount; i++)
							{
								globePostsInst.PointsCpu[i].Color = new Color(250, 128, 40);
								globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
							{
								globePostsTwitter.PointsCpu[i].Color = new Color(0, 255, 255);
								globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
							};
							globePostsVK.UpdatePointsBuffer();
							globePostsInst.UpdatePointsBuffer();							
							globePostsTwitter.UpdatePointsBuffer();
							legendary_legend.Visible = true;
							legendImage.Visible = true;
							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-socnet");
							legendImage.Width = legendImage.Image.Width;
							legendImage.Height = legendImage.Image.Height;
							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
							machine = true;
							break;
						}
					case 2:
						{
							for (int i = 0; i < globePostsVK.PointsCount; i++)
							{
								var post = currentVKPosts[i];
								globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
								if (post.Happiness > 0.5 || post.Polarity > 0)
								{
									globePostsVK.PointsCpu[i].Color = new Color(113, 185, 29);
								}
								else
								{
									if (post.Sadness > 0.5 || post.Polarity < 0)
									{
										globePostsVK.PointsCpu[i].Color = new Color(185, 38, 46);
									} else
									{
										globePostsVK.PointsCpu[i].Color = new Color(165, 176, 184);
										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
									}
								}
							};
							for (int i = 0; i < globePostsInst.PointsCount; i++)
							{
								var post = currentInstPosts[i];
								globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
								if (post.Happiness > 0.5 || post.Polarity > 0)
								{
									globePostsInst.PointsCpu[i].Color = new Color(113, 185, 29);
								}
								else
								{
									if (post.Sadness > 0.5 || post.Polarity < 0)
									{
										globePostsInst.PointsCpu[i].Color = new Color(185, 38, 46);
									}
								}
								

								if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
								{
									globePostsInst.PointsCpu[i].Color = new Color(165, 176, 184);
								}
								
							};
							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
							{
								var post = currentTwitterPosts[i];
								globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
								if (post.Happiness > 0.5 || post.Polarity > 0)
								{
									globePostsTwitter.PointsCpu[i].Color = new Color(113, 185, 29);
								}
								else
								{
									if (post.Sadness > 0.5 || post.Polarity < 0)
									{
										globePostsTwitter.PointsCpu[i].Color = new Color(185, 38, 46);
									}
									else
									{
										globePostsTwitter.PointsCpu[i].Color = new Color(165, 176, 184);
									}
								}
							};
							globePostsVK.UpdatePointsBuffer();
							globePostsInst.UpdatePointsBuffer();
							globePostsTwitter.UpdatePointsBuffer();
							legendary_legend.Visible = true;
							legendImage.Visible = true;
							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-emot-mach");
							legendImage.Width = legendImage.Image.Width;
							legendImage.Height = legendImage.Image.Height;
							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
							machine = true;
							break;
						}
					case 3:
						{
							for (int i = 0; i < globePostsVK.PointsCount; i++)
							{
								float values = currentVKPosts[i].User_Positive + currentVKPosts[i].User_Neutral + currentVKPosts[i].User_Negative;
								if (values == 0)
								{
									globePostsVK.PointsCpu[i].Color = new Color(230, 190, 110);
								}
								else
								{
									globePostsVK.PointsCpu[i].Color = new Color(165, 176, 184);
									if (currentVKPosts[i].User_Positive / values > 0.5)
									{
										globePostsVK.PointsCpu[i].Color = new Color(113, 185, 29);
										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
									}

										if (currentVKPosts[i].User_Negative / values > 0.5)
										{
											globePostsVK.PointsCpu[i].Color = new Color(185, 38, 46);
											globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									
								}								
							};
							for (int i = 0; i < globePostsInst.PointsCount; i++)
							{
								float values = currentInstPosts[i].User_Positive + currentInstPosts[i].User_Neutral + currentInstPosts[i].User_Negative;
								if (values == 0)
								{
									globePostsInst.PointsCpu[i].Color = new Color(230, 190, 110);
								}
								else
								{
									globePostsInst.PointsCpu[i].Color =  new Color(165, 176, 184);
									if (currentInstPosts[i].User_Positive / values > 0.5)
									{
										globePostsInst.PointsCpu[i].Color = new Color(113, 185, 29);
										globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if (currentInstPosts[i].User_Negative / values > 0.5)
										{
											globePostsInst.PointsCpu[i].Color = new Color(185, 38, 46);
											globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
										}
									}
								}								
							};
							for (int i = 0; i < globePostsTwitter.PointsCount; i++)
							{
								float values = currentTwitterPosts[i].User_Positive + currentTwitterPosts[i].User_Neutral + currentTwitterPosts[i].User_Negative;
								if (values == 0)
								{
									globePostsTwitter.PointsCpu[i].Color = new Color(230, 190, 110);
								} else
								{
									globePostsTwitter.PointsCpu[i].Color = new Color(165, 176, 184);
									if ((float) (currentTwitterPosts[i].User_Positive / values) > 0.5)
									{
										globePostsTwitter.PointsCpu[i].Color = new Color(113, 185, 29);
										globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
									}
										if ((float) (currentTwitterPosts[i].User_Negative / values) > 0.5)
										{
											globePostsTwitter.PointsCpu[i].Color = new Color(185, 38, 46);
											globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									
								}								
							};
							globePostsVK.UpdatePointsBuffer();
							globePostsInst.UpdatePointsBuffer();
							globePostsTwitter.UpdatePointsBuffer();
							legendary_legend.Visible = true;
							legendImage.Visible = true;
							legendImage.Image = ((CustomGameInterface)Game.GameInterface).ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-emot-user");
							legendImage.Width = legendImage.Image.Width;
							legendImage.Height = legendImage.Image.Height;
							legendImage.X = (Panel as GisPanel).Frame.Width - rightPanel.Width - legendImage.Image.Width - 10 * ConstantFrameUI.gridUnits;
							machine = false;
							break;
						}
				};
			}
		}

		public void colorBySentiment()
		{
			for (int i = 0; i < currentVKPosts.Count; i++)
			{
				globePostsVK.PointsCpu[i].Color.Alpha = 0;
			}

			for (int i = 0; i < currentInstPosts.Count; i++)
			{
				globePostsInst.PointsCpu[i].Color.Alpha = 0;
			}
			for (int i = 0; i < currentTwitterPosts.Count; i++)
			{
				globePostsTwitter.PointsCpu[i].Color.Alpha = 0;
			}
			foreach (var c in rightPanel.filterFrameList.Children)
			{		
				var checkbox = c as Checkbox;
				switch (checkbox.Parent.Children.ToList().IndexOf(checkbox))
				{
					case 0:
						{							
							for (int i = 0; i < currentVKPosts.Count; i++)
							{
								var post = currentVKPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;

								if (machine)
								{
									if (post.Happiness > 0.5 || post.Polarity > 0)
									{
										globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if ((float)(post.User_Positive / values) > 0.5)
										{
											globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
										}
									}
									
								}
								
							}							
							globePostsVK.UpdatePointsBuffer();
							
							for (int i = 0; i < currentInstPosts.Count; i++)
							{
								var post = currentInstPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;

								if (machine)
								{
									if (post.Happiness > 0.5 || post.Polarity > 0)
									{
										globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if ((float)(post.User_Positive / values) > 0.5)
										{
											globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
										}
									}
								}
								
							}							
							globePostsInst.UpdatePointsBuffer();

							for (int i = 0; i < currentTwitterPosts.Count; i++)
							{
								var post = currentTwitterPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;

								if (machine)
								{
									if (post.Happiness > 0.5 || post.Polarity > 0)
									{
										globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if ((float)(post.User_Positive / values) > 0.5)
										{
											globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
										}
									}
								}
								
							}
							globePostsTwitter.UpdatePointsBuffer();
							break;
						}
					case 1:
						{							
							for (int i = 0; i < currentInstPosts.Count; i++)
							{
								var post = currentInstPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;
								if (machine)
								{
									if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
									{
										globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if (!((float)(post.User_Positive / values) > 0.5) && !(post.User_Negative / values > 0.5))
										{
											globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
										}
									}
								}								
							}
							globePostsInst.UpdatePointsBuffer();
							
							
							
							for (int i = 0; i < currentVKPosts.Count; i++)
							{
								var post = currentVKPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;

								if (machine)
								{
									if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
									{
										globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if (!((float)(post.User_Positive / values) > 0.5) && !(post.User_Negative / values > 0.5))
										{
											globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
										}
									}
								}
							}
							globePostsVK.UpdatePointsBuffer();

							for (int i = 0; i < currentTwitterPosts.Count; i++)
							{
								var post = currentTwitterPosts[i];
								float values = post.User_Positive + post.User_Neutral + post.User_Negative;

								if (machine)
								{
									if (post.Neutral > 0.5 || (!(post.Happiness > 0.5 || post.Polarity > 0) && !(post.Sadness > 0.5 || post.Polarity < 0)))
									{
										globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
									}
									else
									{
										if (!((float)(post.User_Positive / values) > 0.5) && !(post.User_Negative / values > 0.5))
										{
											globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
										}
									}
								}
							}
							globePostsTwitter.UpdatePointsBuffer();
							break;
						}
					case 2:
						
							for (int i = 0; i < currentVKPosts.Count; i++)
							{
								var post = currentVKPosts[i];
							float values = post.User_Positive + post.User_Neutral + post.User_Negative;

							if (machine)
								{
									if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0))
									{
										globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
									}
								}
								else
								{
									if (values == 0)
									{
										globePostsVK.PointsCpu[i].Color.Alpha = maxAlpha;
								}
									else
									{
										if (post.User_Negative / values > 0.5)
										{
											globePostsVK.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsVK.PointsCpu[i].Color.Alpha;
										}
									}
								}
								
							}
							globePostsVK.UpdatePointsBuffer();
						
						
							for (int i = 0; i < currentInstPosts.Count; i++)
							{
								var post = currentInstPosts[i];
							float values = post.User_Positive + post.User_Neutral + post.User_Negative;

							if (machine)
							{
								if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0))
								{
									globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
								}
							}
							else
							{
								if (values == 0)
								{
									globePostsInst.PointsCpu[i].Color.Alpha = maxAlpha;
								}
								else
								{
									if (post.User_Negative / values > 0.5)
									{
										globePostsInst.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsInst.PointsCpu[i].Color.Alpha;
									}
								}
							}

						}

						globePostsInst.UpdatePointsBuffer();

						for (int i = 0; i < currentTwitterPosts.Count; i++)
						{
							var post = currentTwitterPosts[i];
							float values = post.User_Positive + post.User_Neutral + post.User_Negative;

							if (machine)
							{
								if (post.Sadness > 0.5 || post.Polarity < 0 && !(post.Happiness > 0.5 || post.Polarity > 0))
								{
									globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
								}
							}
							else
							{
								if (values == 0)
								{
									globePostsTwitter.PointsCpu[i].Color.Alpha = maxAlpha;
								}
								else
								{
									if (post.User_Negative / values > 0.5)
									{
										globePostsTwitter.PointsCpu[i].Color.Alpha = (checkbox.IsChecked) ? maxAlpha : globePostsTwitter.PointsCpu[i].Color.Alpha;
									}
								}
							}

						}
						globePostsTwitter.UpdatePointsBuffer();

						break;
				};
			}
		}

		private Frame CreateMapControls(FrameProcessor ui, int x, int y)
		{
			var listButton = new Frame(ui, x, y, 0, 0, "", Color.Zero)
			{
				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
			};
			var textureAll = Game.Content.Load<DiscTexture>(@"Sochi\btns_show-all");
			var textureSochi = Game.Content.Load<DiscTexture>(@"Sochi\cities-Sochi");
			var textureAdler = Game.Content.Load<DiscTexture>(@"Sochi\cities-Adler");
			var textureKrPol = Game.Content.Load<DiscTexture>(@"Sochi\cities-Krpol");
			var texturePlus = Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomin");
			var textureMinus = Game.Content.Load<DiscTexture>(@"ui\map-btns-zoomout");

			var listTexture = new[] { textureSochi, textureAdler, textureKrPol, texturePlus, textureMinus };

			var listCityButton = new ListBox(ui, 0, 0, 0, 0, "", new Color(25, 25, 25, 255))
			{
				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
			};			
 
			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureAll, textureAll.Width, textureAll.Height, Color.White,
				() => {
					cameraMove = true;
					currentCoords = new DVector3(ViewLayer.GlobeCamera.Yaw, ViewLayer.GlobeCamera.Pitch, ViewLayer.GlobeCamera.CameraDistance);
					targetCoords = new DVector3(DMathUtil.DegreesToRadians(40.0676951171882), -DMathUtil.DegreesToRadians(43.555521838764), ViewLayer.GlobeCamera.Parameters.MaxCameraDistance);
				}));

			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureSochi, textureSochi.Width, textureSochi.Height, Color.White, 
				() => {
					cameraMove = true;
					currentCoords = new DVector3(ViewLayer.GlobeCamera.Yaw, ViewLayer.GlobeCamera.Pitch, ViewLayer.GlobeCamera.CameraDistance);
					targetCoords = new DVector3(DMathUtil.DegreesToRadians(39.749685), -DMathUtil.DegreesToRadians(43.58191), GeoHelper.EarthRadius + 27);
				}));

			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureAdler, textureAdler.Width, textureAdler.Height, Color.White, 
				() => {
					cameraMove = true;
					currentCoords = new DVector3(ViewLayer.GlobeCamera.Yaw, ViewLayer.GlobeCamera.Pitch, ViewLayer.GlobeCamera.CameraDistance);
					targetCoords = new DVector3(DMathUtil.DegreesToRadians(39.946433), -DMathUtil.DegreesToRadians(43.417497), GeoHelper.EarthRadius + 23);					
				}));

			listCityButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureKrPol, textureKrPol.Width, textureKrPol.Height, Color.White, 
				() => {
					cameraMove = true;
					currentCoords = new DVector3(ViewLayer.GlobeCamera.Yaw, ViewLayer.GlobeCamera.Pitch, ViewLayer.GlobeCamera.CameraDistance);
					targetCoords = new DVector3(DMathUtil.DegreesToRadians(40.258084), -DMathUtil.DegreesToRadians(43.668331), GeoHelper.EarthRadius + 25);
				}));

			var listMapButton = new ListBox(ui, 0, listCityButton.Y + listCityButton.Height +  offset * 12, 0, 0, "", new Color(25, 25, 25, 255))
			{
				Anchor = FrameAnchor.Right | FrameAnchor.Bottom
			};
			listMapButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", texturePlus, texturePlus.Width, texturePlus.Height, Color.White, 
				() => {
					currentDistance = ViewLayer.GlobeCamera.CameraDistance;
					desiredDistance = ViewLayer.GlobeCamera.CameraDistance + (ViewLayer.GlobeCamera.CameraDistance - ViewLayer.GlobeCamera.EarthRadius) * (-0.3f);
					cameraZoom = true;
				}));
			listMapButton.addElement(FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.mapButtonSize, ConstantFrameUI.mapButtonSize, "", textureMinus, textureMinus.Width, textureMinus.Height, Color.White, 
				() => {
					currentDistance = ViewLayer.GlobeCamera.CameraDistance;
					desiredDistance = ViewLayer.GlobeCamera.CameraDistance + (ViewLayer.GlobeCamera.CameraDistance - ViewLayer.GlobeCamera.EarthRadius) * (0.3f);
					cameraZoom = true;
				}));

			listButton.Add(listCityButton);
			listButton.Add(listMapButton);
			listButton.Width = listCityButton.Width;
			listButton.Height = listMapButton.Y + listMapButton.Height;
			return listButton;
		}

		private Frame activity_count;
		private Frame legendImage;
		private Frame darkPanel;
		private Frame mapButtons;
		private Frame infoButton;

		private void CreateHelpWindow(FrameProcessor ui, Frame frame)
		{
			var help_texture = ui.Game.Content.Load<DiscTexture>("Sochi/final_help-screen");
			var infoPanel = new Button(ui, 0, 0, help_texture.Width, help_texture.Height, "", Color.Zero)
			{
				Image = help_texture,
			};
			infoPanel.Click += (c, b) =>
			{
				infoPanel.Parent.Remove(infoPanel);
				infoPanel = null;
				secondsBeforeGoToBed = timeToSleep;
				help = true;
			};
			frame.Add(infoPanel);

			if (webFrame != null) return;
			Game.Touch.Tap -= OnTap;
			Game.Touch.Manipulate -= OnManipulate;

			infoPanel.Click += (c, b) =>
			{
				Game.Touch.Tap += OnTap;
				Game.Touch.Manipulate += OnManipulate;
			};
		}

		private void CreateIntefaceElements(FrameProcessor ui, int x, int y)
		{
			var frame = (Panel as GisPanel).Frame;
			var texture = ui.Game.Content.Load<DiscTexture>("ui/help-btn");
			infoButton = new Button(ui, x, y, offset * 24, offset * 24, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				Image = texture,
				Anchor = FrameAnchor.Bottom | FrameAnchor.Left,
				HoverColor  =ColorConstant.HoverColor,
			};
			infoButton.Click += (s, a) =>
			{
				help = false;
				CreateHelpWindow(ui, frame);
			};
			frame.Add(infoButton);

			string activityString = "Пользовательских";
			Console.WriteLine(ConstantFrameUI.sfUltraLight32.MeasureString(activityString).Width);
			var activity_note = new Frame(ui, 9 * offset, 9 * offset, ConstantFrameUI.sfUltraLight32.MeasureString(activityString).Width, offset * 8, activityString, Color.Zero)
			{
				Font = ConstantFrameUI.sfUltraLight32,
				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
				TextAlignment = Alignment.TopLeft,
			};
			string activityString2 = "оценок:";
			var activity_note2 = new Frame(ui, 9 * offset, activity_note.Y + activity_note.Height, ConstantFrameUI.sfUltraLight32.MeasureString(activityString2).Width, offset * 8, activityString2, Color.Zero)
			{
				Font = ConstantFrameUI.sfUltraLight32,
				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
				TextAlignment = Alignment.MiddleLeft,

			};
			frame.Add(activity_note);
			frame.Add(activity_note2);

			activity_count = new Frame(ui, 9 * offset, activity_note2.Y + activity_note2.Height, 0, ConstantFrameUI.sfLight75.LineHeight, activityString, Color.Zero)
			{
				Anchor = FrameAnchor.Top | FrameAnchor.Left,
				Font = ConstantFrameUI.sfLight75,
			};
			frame.Add(activity_count);

			string legend = "Легенда:";
			legendary_legend = new Frame(ui, (int)(frame.Width - rightPanel.Width - ConstantFrameUI.sfUltraLight32.MeasureString(legend).Width) - 10 * offset, offset * 8, ConstantFrameUI.sfUltraLight32.MeasureString(legend).Width, ConstantFrameUI.sfUltraLight32.MeasureString(legend).Height, legend, Color.Zero)
			{
				Anchor = FrameAnchor.Top | FrameAnchor.Right,
				Font = ConstantFrameUI.sfUltraLight32,
				ForeColor = new Color(Color.White.ToVector3(), 0.5f),
			};

			var image = ui.Game.Content.Load<DiscTexture>(@"ui\Legend\lgnd_legend-socnet");
			legendImage = new Frame(ui, frame.Width - rightPanel.Width - image.Width - 10 * offset, legendary_legend.Y + legendary_legend.Height + offset * 8, image.Width, image.Height, "", Color.Zero)
			{
				Anchor = FrameAnchor.Top ,
				Image = image,
				ImageMode = FrameImageMode.Stretched,
			};
			frame.Add(legendary_legend);
			frame.Add(legendImage);
		}

		public void OnTap(TouchEventArgs p)
		{
			

			onMouseClick(p.Position.X, p.Position.Y);
			if (p.IsEventEnd) return;
			if (p.IsEventBegin)
			{
				if (cameraMove) cameraMove = false;
				if (cameraZoom) cameraZoom = false;
			}
		}

		public void OnManipulate(TouchEventArgs p)
		{
			//if (webFrame != null) return;


			var f = ConstantFrameUI.GetHoveredFrame((Panel as GisPanel).Frame, p.Position);
			if (f != null)
			{
				ViewLayer.GlobeCamera.CameraZoom(MathUtil.Clamp(1.0f - p.ScaleDelta, -0.3f, 0.3f));

				if (p.IsEventEnd) return;

				if (p.IsEventBegin)
					previousMousePosition = p.Position;

				if (cameraMove) cameraMove = false;
				if (cameraZoom) cameraZoom = false;
				ViewLayer.GlobeCamera.MoveCamera(previousMousePosition, p.Position);
				previousMousePosition = p.Position;
			}
		}
		private void updateCoorFrame()
		{
			if (webFrame != null)
			{
				//var cartesianCoor = GeoHelper.SphericalToCartesian(new DVector2(webFrame.geoPoint.Value.Lon, webFrame.geoPoint.Value.Lat),
				//														ViewLayer.GlobeCamera.EarthRadius);

				//var screenPosition = ViewLayer.GlobeCamera.CartesianToScreen(cartesianCoor);
				//webFrame.X = (int)screenPosition.X;
				//webFrame.Y = (int)screenPosition.Y;
			}
		}


	}
}