using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Engine.Media;
using Fusion.Engine.Frames;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics;
using Fusion.Engine.Common;
using System.IO;
using System.Net;
using City.ControlsClient;
using System.Globalization;

namespace City.UIFrames.FrameElement
{
	public class WebPostFrame: Frame
	{
		private int sizeButton = 40;
		private int currentPhoto = 0;
		private int currentPost = 0;
		private int maxImageSize;
		private int sizeRightPanel = 0;

		private Frame photo;
		private Frame message;
		private Frame info;
		private Frame buttonLeft;
		private Frame buttonRight;
		private Frame betweenPosts;
		private Frame evaluationPanel;

		private Frame activity_number;
		private int activity_count;

		public Frame negativeButton;
		public Frame positiveButton;
		public Frame neutralButton;

		private InstagramPost[] posts;

		public bool increasePositiveValue = true;
		public bool increaseNegativeValue = true;
		public bool increaseNeutralValue = true;
		public bool sendToHell = false;
		public bool recalcSum = true;

		public Gis.GeoPoint? geoPoint;

		private string[] soc_icons = new string[]
		{
			"social/btns_vk_bw", "social/btns_instagram_bw", "social/btns_twitter_bw"
		};
	
		private static Frame frame;

		public WebPostFrame(FrameProcessor ui, string img, string filename, int rightPanel) : base(ui)
        {
			sizeRightPanel = rightPanel;
			init(ui, img, filename);
		}

		public WebPostFrame(FrameProcessor ui, int x, int y, int w, int h, string img, string url, Color backColor, string text, int rightPanel, InstagramPost[] inst, Frame activity = null) : base(ui, x, y, w, h, text, backColor)
        {
			sizeRightPanel = rightPanel;
			posts = inst;
			init(ui, img, url, w, h, posts[0].TimeStamp.ToString("d",
				  CultureInfo.CreateSpecificCulture("ru-RU")), posts[0].Likes);
			if (activity != null)
			{
				activity_number = activity;
				activity_count = int.Parse(activity_number.Text);
			}			
		}

		private void init(FrameProcessor ui, string img, string filename,  int w = 150, int h = 150, string date = "25 июня 2016, 00:00", string likes = "нет данных")
		{
			//if (frame != null)
			//{
			//	frame.Parent.Remove(frame);
			//	frame = null;
			//}
			//frame = this;

			maxImageSize = w;
			BackColor = new Color(25, 25, 25, 255);
			TextAlignment = Alignment.BaselineCenter;
			Font = ConstantFrameUI.sfLight18;

			var urls = String.IsNullOrEmpty(filename) ? new string[] { filename } : filename.Split(',');

			photo = new Frame(ui, 0, 0, w, h, "", new Color(25, 25, 25, 255))
			{
				ImageMode = FrameImageMode.Stretched,
				Image = ui.Game.Content.Load<DiscTexture>(img),
			};
			this.Y = (this.Height) / 2;
			this.X = (Game.RenderSystem.DisplayBounds.Width - photo.Image.Width - sizeRightPanel) / 2;

			createInfoPanel(ui, soc_icons[posts[0].SocialNetworkId], date, likes, img);
			createMessagePanel(ui, posts[0].Text);
			createEvaluationPanel(ui);

			loadPhoto(ui, img, 0, 0, w,  urls[0]);

			this.Add(photo);
			this.Height = photo.Height + message.Height + info.Height + ConstantFrameUI.gridUnits * 15;
			if (betweenPosts != null)
				this.Height += betweenPosts.Height;

			//updateFramePosition();

		}

		protected override void Update(GameTime gameTime)
		{
		}

		//void createTopPanel(FrameProcessor ui)
		//{
		//	sizeButton = ConstantFrameUI.gridUnits * 8;
		//	closeButton = new Button(ui, this.Width - sizeButton, 0, sizeButton, sizeButton, "", new Color(0, 0, 0, 127))
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		HoverColor = ColorConstant.HoverColor,
		//		Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-close"),

		//	};
		//	closeButton.Click += (s, a) =>
		//	{
		//		this.Parent.Remove(this);
		//		if (frame != null)
		//		{
		//			frame.Parent.Remove(frame);
		//			frame = null;
		//		}
		//		this.Height = 0;
		//		this.Width = 0;
		//		sendToHell = true;
		//		recalcSum = true;
		//	};
		//	this.Add(closeButton);

		//	sizeButton = (int)(ConstantFrameUI.gridUnits * 1.5);
		//	var bg = ui.Game.Content.Load<DiscTexture>("ui/btn_machine-emotion");
		//	var circle = new Frame(ui, sizeButton, sizeButton, bg.Width, bg.Height, "", Color.Zero)
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		Image = bg,
		//		ImageMode = FrameImageMode.Stretched,
		//		ImageColor = Color.White,
		//	};

		//	Color circleColor = new Color(165, 176, 184);
		//	if (posts[currentPost].Happiness > 0.5 || posts[currentPost].Polarity > 0)
		//	{
		//		circleColor = new Color(113, 185, 29);
		//	}
		//	else
		//	{
		//		if (posts[currentPost].Sadness > 0.5 || posts[currentPost].Polarity < 0)
		//		{
		//			circleColor = new Color(185, 38, 46);
		//		}				
		//	}
		//	var circleAutomaticEmotion = new Frame(ui, 0, 0, bg.Width, bg.Height, "", Color.Zero)
		//	//new Frame(ui, border, border, (sizeButton  - border) * 2, (sizeButton - border) * 2, "", Color.Zero)
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		Image = ui.Game.Content.Load<DiscTexture>("ui/btn-color_machine-emotion"),
		//		ImageMode = FrameImageMode.Stretched,
		//		ImageColor = circleColor,
		//	};
		//	circle.Add(circleAutomaticEmotion);
		//	this.Add(circle);
		//}

		void createEvaluationPanel(FrameProcessor ui)
		{
			sizeButton = ConstantFrameUI.gridUnits * 15;
			var distance = ConstantFrameUI.gridUnits;
			evaluationPanel = betweenPosts = new Frame(ui, 0, message.Y + message.Height + ConstantFrameUI.gridUnits * 5, photo.Width, distance * 10, "", new Color(25, 25, 25, 255));

			var frame_first = new Frame(ui, distance * 5, (int)(evaluationPanel.Height - distance * 8) / 2, distance * 14, distance * 4, "оценка", Color.Zero)
			{
				TextAlignment = Alignment.TopRight,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};
			evaluationPanel.Add(frame_first);

			var frame_second = new Frame(ui, distance * 5, frame_first.Y + frame_first.Height, distance * 14, distance * 4, "системы", Color.Zero)
			{
				TextAlignment = Alignment.TopRight,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};
			evaluationPanel.Add(frame_second);

			var machine_texture = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_empty");
			Color circleColor = new Color(165, 176, 184, 100);
			if (posts[currentPost].Happiness > 0.5 || posts[currentPost].Polarity > 0)
			{
				circleColor = new Color(113, 185, 29, 100);
				machine_texture = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_empty");
			}
			else
			{
				if (posts[currentPost].Sadness > 0.5 || posts[currentPost].Polarity < 0)
				{
					circleColor = new Color(185, 38, 46, 100);
					machine_texture = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_empty");
				}
			}
			var circleAutomaticEmotion = new Frame(ui, frame_second.X + frame_second.Width + distance * 4, 0, distance * 10, distance * 10, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				Image = machine_texture,
				ImageMode = FrameImageMode.Stretched,
				ImageColor = circleColor,
			};
			evaluationPanel.Add(circleAutomaticEmotion);

			var frame_third = new Frame(ui, circleAutomaticEmotion.X + circleAutomaticEmotion.Width + distance * 5, (int)(evaluationPanel.Height - distance * 8) / 2, distance * 12, distance * 4, "ваша", Color.Zero)
			{
				TextAlignment = Alignment.TopRight,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};
			evaluationPanel.Add(frame_third);

			var frame_fourth= new Frame(ui, circleAutomaticEmotion.X + circleAutomaticEmotion.Width + distance * 5, frame_first.Y + frame_first.Height, distance * 12, distance * 4, "оценка", Color.Zero)
			{
				TextAlignment = Alignment.TopRight,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};
			evaluationPanel.Add(frame_fourth);

			positiveButton = new Button(ui, frame_third.X + frame_third.Width + distance * 4, 0, distance * 10, distance * 10, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = Color.Zero,
				Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_empty"),
				ImageColor = new Color(113, 185, 29),
			};
			positiveButton.Click += (s, a) =>
			{
				if (increasePositiveValue)
				{
					if (!increaseNegativeValue)
					{
						posts[currentPost].User_Negative--;
						negativeButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_empty");
						increaseNegativeValue = true;
						Console.WriteLine("Negative:" + posts[currentPost].User_Negative);
					}
					if (!increaseNeutralValue)
					{
						posts[currentPost].User_Neutral--;
						increaseNeutralValue = true;
						neutralButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_empty");
					}
				}
				int changing = (increasePositiveValue) ? 1 : -1;
				posts[currentPost].User_Positive += changing;
				if (activity_number != null)
				{
					activity_count += changing;
					activity_number.Text = "" + activity_count;
				}
				buttonLeft.BackColor = (increasePositiveValue) ? ColorConstant.BlueAzure : Color.Black;
				buttonRight.BackColor = (increasePositiveValue) ? ColorConstant.BlueAzure : Color.Black;
				positiveButton.Image = (!increasePositiveValue) ? ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_empty") : ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_solid");
				if (increasePositiveValue)
				{
					positiveButton.ImageColor = Color.White;
					positiveButton.RunTransition("ImageColor", new Color(113, 185, 29), 0, 500);
					if (activity_number != null)
					{
						activity_number.ForeColor = new Color(113, 185, 29);
						activity_number.RunTransition("ForeColor", Color.White, 0, 1000);
					}					
				}
				increasePositiveValue = !increasePositiveValue;
				//Console.WriteLine(posts[currentPost].User_Positive);
				int values = posts[currentPost].User_Positive + posts[currentPost].User_Neutral + posts[currentPost].User_Negative;
				Console.WriteLine(values + "");



			};

			neutralButton = new Button(ui, positiveButton.X + positiveButton.Width + distance * 5, 0, distance * 10, distance * 10, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = Color.Zero,
				Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_empty"),
				ImageColor = new Color(165, 176, 184),
			};
			neutralButton.Click += (s, a) =>
			{
				if (increaseNeutralValue)
				{
					if (!increasePositiveValue)
					{
						posts[currentPost].User_Positive--;
						increasePositiveValue = true;
						positiveButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_empty");
						Console.WriteLine("Positive:" + posts[currentPost].User_Positive);
					}
					if (!increaseNegativeValue)
					{
						posts[currentPost].User_Negative--;
						negativeButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_empty");
						increaseNegativeValue = true;
					}
				}
				int changing = (increaseNeutralValue) ? 1 : -1;
				posts[currentPost].User_Neutral += changing;
				if (activity_number != null)
				{
					activity_count += changing;
					activity_number.Text = "" + activity_count;
				}
				buttonLeft.BackColor = (increaseNeutralValue) ? ColorConstant.BlueAzure : Color.Black;
				buttonRight.BackColor = (increaseNeutralValue) ? ColorConstant.BlueAzure : Color.Black;
				neutralButton.Image = (!increaseNeutralValue) ? ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_empty") : ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_solid");
				if (increaseNeutralValue)
				{
					neutralButton.ImageColor = Color.White;
					neutralButton.RunTransition("ImageColor", new Color(165, 176, 184), 0, 500);
					if (activity_number != null)
					{
						activity_number.ForeColor = new Color(165, 176, 184);
						activity_number.RunTransition("ForeColor", Color.White, 0, 1000);
					}
				}
				increaseNeutralValue = !increaseNeutralValue;
				Console.WriteLine(posts[currentPost].User_Neutral);
			};

			negativeButton = new Button(ui, neutralButton.X + neutralButton.Width + distance * 5, 0, distance * 10, distance * 10, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = Color.Zero,
				Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_empty"),
				ImageColor = new Color(185, 38, 46),
			};
			negativeButton.Click += (s, a) =>
			{
				if (increaseNegativeValue)
				{
					if (!increasePositiveValue)
					{
						posts[currentPost].User_Positive--;
						increasePositiveValue = true;
						positiveButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_happy_empty");
						Console.WriteLine("Positive:" + posts[currentPost].User_Positive);
					}
					if (!increaseNeutralValue)
					{
						posts[currentPost].User_Neutral--;
						increaseNeutralValue = true;
						neutralButton.Image = ui.Game.Content.Load<DiscTexture>("social/btns_40emo_neutr_empty");
					}
				}
				int changing = (increaseNegativeValue) ? 1 : -1;
				posts[currentPost].User_Negative += changing;
				if (activity_number != null)
				{
					activity_count += changing;
					activity_number.Text = "" + activity_count;
				}
				buttonLeft.BackColor = (increaseNegativeValue) ? ColorConstant.BlueAzure : Color.Black;
				buttonRight.BackColor = (increaseNegativeValue) ? ColorConstant.BlueAzure : Color.Black;
				negativeButton.Image = (!increaseNegativeValue) ? ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_empty") : ui.Game.Content.Load<DiscTexture>("social/btns_40emo_sad_solid");
				if (increaseNegativeValue)
				{
					negativeButton.ImageColor = Color.White;
					negativeButton.RunTransition("ImageColor", new Color(185, 38, 46), 0, 500);
					if (activity_number != null)
					{
						activity_number.ForeColor = new Color(185, 38, 46);
						activity_number.RunTransition("ForeColor", Color.White, 0, 1000);
					}
				}
				increaseNegativeValue = !increaseNegativeValue;
				Console.WriteLine(posts[currentPost].User_Negative);
			};
			evaluationPanel.Add(positiveButton);
			evaluationPanel.Add(negativeButton);
			evaluationPanel.Add(neutralButton);
			this.Add(evaluationPanel);
		}

		void createMessagePanel(FrameProcessor ui, string text)
		{
			text = text.Trim('"');
			if (String.IsNullOrEmpty(text))
			{
				message = new Frame(ui, ConstantFrameUI.gridUnits * 5, info.Y + info.Height, photo.Width , 0, "", new Color(25, 25, 25, 255))
				{
					TextAlignment = Alignment.MiddleCenter,
					Font = this.Font,
					//Border = 1,
				};
				
			} else
			{
				message = new RichTextBlock(ui, ConstantFrameUI.gridUnits * 5, info.Y + info.Height + ConstantFrameUI.gridUnits * 5, (photo.Width - ConstantFrameUI.gridUnits * 10), 0, text, new Color(25, 25, 25, 255), ConstantFrameUI.sfLight18, ConstantFrameUI.sfLight18.CapHeight)
				{
					TextAlignment = Alignment.MiddleCenter,
					Font = this.Font,
					//Border = 1,
				};
			}
			this.Add(message);
		}

		void createInfoPanel(FrameProcessor ui, string iconName, string dateString, string likes, string img)
		{
			var icon_text = ui.Game.Content.Load<DiscTexture>(iconName);
			int distance = ConstantFrameUI.gridUnits;

			info = new Frame(ui, 0, photo.Height + distance * 5, photo.Width, distance * 15, "", new Color(25, 25, 25, 255))
			{
				TextAlignment = Alignment.MiddleCenter,
			};

			var icon = new Frame(ui, (info.Width - distance * 10) / 2, + distance * 5, distance * 10, distance * 10, "", Color.Zero)
			{
				Image = icon_text,
				ImageMode = FrameImageMode.Stretched,
			};

			var date = new Frame(ui, icon.X - Font.MeasureString(dateString).Width - distance * 4, distance * 2 + distance * 5, Font.MeasureString(dateString).Width, distance * 3, dateString, Color.Zero)
			{
				TextAlignment = Alignment.MiddleRight,
				Text = dateString,
				Font = this.Font,
				ForeColor = new Color (Color.White.ToVector3(), 0.6f),
			};

			bool hasNoData = likes.Equals("нет данных");
			likes = hasNoData ? "0" : likes;
			
			var number_likes = new Frame(ui, icon.X - Font.MeasureString(likes).Width - distance * 4, date.Y + date.Height + distance, Font.MeasureString(likes).Width, distance * 3, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleRight,
				Text = "" + likes,
				Font = this.Font,
				ForeColor = new Color (Color.White.ToVector3(), 0.6f),
			};
			var heart_icon = ui.Game.Content.Load<DiscTexture>("social/btns_like_empty");
			var likes_icon = new Frame(ui, icon.X - 10 - number_likes.Width - distance * 6, date.Y + date.Height + distance, 10, 10, "", Color.Zero)
			{
				Image = heart_icon,
				ImageMode = FrameImageMode.Stretched,
			};

			var post = "Пост:";
			var post_post = new Frame(ui, icon.X + icon.Width + distance * 4, distance * 2 + distance * 5, Font.MeasureString(post).Width, distance * 3, post, Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				Text = post,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};

			var post_string = (currentPost + 1) + " из " + posts.Length;
			var post_number = new Frame(ui, icon.X + icon.Width +  distance * 4, likes_icon.Y, Font.MeasureString(post_string).Width, distance * 3, "", Color.Zero)
			{
				TextAlignment = Alignment.MiddleCenter,
				Text = post_string,
				Font = this.Font,
				ForeColor = new Color(Color.White.ToVector3(), 0.6f),
			};

			buttonLeft = new Button(ui, 0, distance * 5, distance * 10, distance * 10, "", Color.Black)
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = ColorConstant.HoverColor,
				Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-left"),

			};

			buttonLeft.Click += (s, a) =>
			{
				if (currentPost > 0)
				{
					currentPost--;
					updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
				  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
				}
				else
				{
					currentPost = posts.Length - 1;
					updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
				  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
				}
			};

			buttonRight = new Button(ui, info.Width - distance * 10, distance * 5, distance * 10, distance * 10, "", Color.Black)
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = ColorConstant.HoverColor,
				Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-right"),

			};
			buttonRight.Click += (s, a) =>
			{
				if (currentPost + 1 < posts.Length)
				{
					currentPost++;
					updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
				  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
				}
				else
				{
					currentPost = 0;
					updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
				  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
				}
			};

			info.Add(icon);
			info.Add(date);
			info.Add(likes_icon);
			info.Add(number_likes);
			info.Add(post_post);						
			info.Add(post_number);

			if (posts.Length != 1)
			{
				info.Add(buttonLeft);
				info.Add(buttonRight);
			}
			this.Add(info);
		}

		void addNextPreviousButton(FrameProcessor ui, string img, int x, string[] url)
		{
			if (buttonLeft != null)
			{
				buttonLeft.Parent.Remove(buttonLeft);
				buttonLeft = null;
			}
			if (buttonRight != null)
			{
				buttonRight.Parent.Remove(buttonRight);
				buttonRight = null;
			}

			if (url.Length == 1) return;

			int height = ConstantFrameUI.gridUnits * 15;
			int width = ConstantFrameUI.gridUnits * 8;
			buttonLeft = new Button(ui, 0, (photo.Height - height) / 2, width, height, "", new Color(0, 0, 0, 127))
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = ColorConstant.HoverColor,
				Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-left"),

			};
			
				buttonLeft.Click += (s, a) =>
				{
					if (currentPhoto > 0)
					{
						loadPhoto(ui, img, 0, 0, Width, url[currentPhoto - 1]);
						currentPhoto--;
					}
					
				};
						
			this.Add(buttonLeft);

			buttonRight = new Button(ui, photo.Width - width, (photo.Height - height) / 2, width, height, "", new Color(0, 0, 0, 127))
			{
				TextAlignment = Alignment.MiddleCenter,
				HoverColor = ColorConstant.HoverColor,
				Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-right"),
			};
			buttonRight.Click += (s, a) =>
				{
					if (currentPhoto + 1 < url.Length)
					{
						loadPhoto(ui, img, 0, 0, Width,  url[currentPhoto + 1]);
						currentPhoto++;
					}	
				};
			this.Add(buttonRight);
			

		}

		//void addButtonsBetweenPosts(FrameProcessor ui, string img)
		//{
		//	if (posts.Length == 1) return;
		//	sizeButton = ConstantFrameUI.gridUnits * 10;

		//	betweenPosts = new Frame(ui, 0, message.Y + message.Height + ConstantFrameUI.gridUnits * 5, photo.Width, sizeButton, "", new Color(25, 25, 25, 255))
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		Text = "Пост: " + (currentPost + 1) + " из " + posts.Length ,
		//		ForeColor = new Color(255,255,255,127),
		//	};
		//	var previousPost = new Button(ui, 0, 0, sizeButton, sizeButton, "", ColorConstant.BlueAzure)
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		HoverColor = ColorConstant.HoverColor,
		//		Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-left"),

		//	};

		//	previousPost.Click += (s, a) =>
		//	{
		//		if (currentPost > 0)
		//		{
		//			currentPost--;
		//			updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
		//		  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
		//		}
		//		else
		//		{
		//			currentPost = posts.Length - 1;
		//			updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
		//		  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
		//		}
		//	};
		//	betweenPosts.Add(previousPost);

		//	var nextPost = new Button(ui, photo.Width - sizeButton, 0, sizeButton, sizeButton, "", ColorConstant.BlueAzure)
		//	{
		//		TextAlignment = Alignment.MiddleCenter,
		//		HoverColor = ColorConstant.HoverColor,
		//		Image = ui.Game.Content.Load<DiscTexture>("social/btn-popup-right"),

		//	};
		//	nextPost.Click += (s, a) =>
		//	{
		//		if (currentPost + 1 < posts.Length)
		//		{
		//			currentPost++;
		//			updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
		//		  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
		//		}
		//		else
		//		{
		//			currentPost = 0;
		//			updatePanel(ui, img, posts[currentPost].Url, posts[currentPost].TimeStamp.ToString("d",
		//		  CultureInfo.CreateSpecificCulture("ru-RU")), posts[currentPost].Likes);
		//		}
		//	};
		//	betweenPosts.Add(nextPost);
		//	this.Add(betweenPosts);
		//}

		void updatePanel(FrameProcessor ui, string img, string filename, string date , string likes )
		{
			var urls = String.IsNullOrEmpty(filename) ? new string[] { filename } : filename.Split(',');
			frame = null;
			frame = this;

			info.Parent.Remove(info);
			info = null;

			message.Parent.Remove(message);
			message = null;

			evaluationPanel.Parent.Remove(evaluationPanel);
			evaluationPanel = null;

			createInfoPanel(ui, soc_icons[posts[currentPost].SocialNetworkId], date, likes, img);
			createMessagePanel(ui, posts[currentPost].Text);
			createEvaluationPanel(ui);

			loadPhoto(ui, img, 0, 0, maxImageSize, urls[0]);

			increasePositiveValue = true;
			increaseNeutralValue = true;
			increaseNegativeValue = true;

			recalcSum = true;
			//updateFramePosition();
		}

		void loadPhoto(FrameProcessor ui, string img, int x, int y, int size, string filename)
		{
			int gU = ConstantFrameUI.gridUnits;			
			var task = new Task(() => {
				var webClient = new WebClient();
				var fileName = Path.GetTempFileName();
				try
				{
					webClient.DownloadFile(filename, fileName);
					photo.Image = new UserTexture(ui.Game.RenderSystem, File.OpenRead(fileName), false);
					float coeff = (float)photo.Image.Height / photo.Image.Width;
					photo.Width = (int)(size );  
					photo.Height = (int)(size * coeff); 
					photo.ImageMode = FrameImageMode.Stretched;
					photo.Visible = true;


					info.Y = photo.Height;
					info.Width = (int)(size);

					message.Y = info.Y + info.Height + gU * 5;
					message.Width = (int)(size - gU * 10);

					int offset = (message.Height > 0) ? gU * 6 : 0;
					
					evaluationPanel.Y = message.Y + message.Height + offset;
					evaluationPanel.Width = (int)(size);
					
					this.Width = photo.Width;
					this.Height = evaluationPanel.Y + evaluationPanel.Height + gU * 6;
					
					this.X = (this.Parent.Width - this.Width - sizeRightPanel) / 2;
					this.Y = (this.Parent.Height - this.Height) / 2;
				}
				catch
				{
					info.Y = 0;
					message.Y = info.Y + info.Height + gU * 5;
					evaluationPanel.Y = message.Y + message.Height + gU * 6;

					Height = evaluationPanel.Y + evaluationPanel.Height + gU * 6;
					photo.Visible = false;
					photo.Height = 0;

					this.X = (this.Parent.Width - this.Width - sizeRightPanel) / 2;
					this.Y = (this.Parent.Height - this.Height) / 2;
				}

			});			
			task.Start();
			
		}

		void updateFramePosition()
		{
			int diffY = Game.RenderSystem.DisplayBounds.Height - (this.Y + this.Height);
			if (diffY < 0)
			{
				photo.Y -= diffY;
				info.Y -= diffY;
				message.Y -= diffY;
				evaluationPanel.Y -= diffY;
				this.Y -= diffY;

			}
		}
	}
}
