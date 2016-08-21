using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.ModelServer;
using City.UIFrames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Media;
using Fusion.Drivers.Graphics;

namespace City
{
	public class GeoBanner
	{
		Game Game;
		DVector2 lonLatPosition;
		DVector3 cartesianPosition;

		public DVector2 LonLatPositionRad	{ set { lonLatPosition = value; cartesianPosition = GeoHelper.SphericalToCartesian(lonLatPosition); } get { return lonLatPosition; } }
		public bool		IsVisible			{ set; get; }
		public bool		IsShowInfo			{ protected set; get; } = false;

		public int Width	{ protected set; get; }
		public int Height	{ protected set; get; }
		public string Text	{ protected set; get; }
		public SpriteFont Font { set; get; }


		public DiscTexture	Biohazard;
		public DiscTexture	Health;
		public DiscTexture	CornerDark;
		public DiscTexture	CornerLight;
		public string		CountBiozard	= "0";
		public string		CountHealth		= "0";
		public Rectangle	GlobalRectangle { protected set; get; }

		public DiseaseServerRespond DiseaseData { set; get; }
		public DateTime InitTime				{ set; get; }

		public GeoBanner(Game game, DVector2 lonLatRad, string name)
		{
			Game = game;
			LonLatPositionRad = lonLatRad;
			Text = name;

			Font = Game.Content.Load<SpriteFont>(@"fonts\segoeReg20");
			Biohazard	= Game.Content.Load<DiscTexture>("ui/biohazard");
			Health		= Game.Content.Load<DiscTexture>("ui/healthy");

			CornerDark	= Game.Content.Load<DiscTexture>("train/billboard_corner_dark");
			CornerLight = Game.Content.Load<DiscTexture>("train/billboard_corner_light");
		}


		public void Update(DateTime curTime)
		{
			if (DiseaseData == null || InitTime > curTime) return;

			var daysCount	= DiseaseData.infected.Length;
			var timeDiff	= curTime - InitTime;

			var totalSamples = timeDiff.TotalHours/4.0;

			var ind = (int)totalSamples;

			if (ind >= daysCount-1) {
				CountBiozard	= DiseaseData.infected.Last().ToString();
				CountHealth		= DiseaseData.recovered.Last().ToString();
				return;
			}

			var next	= ind + 1;

			float factor = (float)totalSamples - ind;
			int infected	= (int)(MathUtil.Lerp( DiseaseData.infected[ind],	DiseaseData.infected[next],		factor ));
			int recovered	= (int)(MathUtil.Lerp( DiseaseData.recovered[ind],	DiseaseData.recovered[next],	factor ));

			CountHealth		= recovered.ToString();
			CountBiozard	= infected.ToString();
        }


		public void Draw(GlobeCamera camera, SpriteLayer sb)
		{
			var gu = ConstantFrameUI.gridUnits;
            Height = gu * 9;
			 
			var screenPos = camera.CartesianToScreen(cartesianPosition);
			GlobalRectangle = new Rectangle((int)screenPos.X + gu + 3*gu, (int)screenPos.Y - Height/2, Width, Height);

			var obRec = new Rectangle((int)screenPos.X + gu/2 + 3*gu, (int)screenPos.Y - Height / 2 - gu/2, Width + gu, Height + gu);

			sb.Draw(Game.RenderSystem.WhiteTexture, obRec, new Color(255, 242, 0, 255));
			sb.Draw(CornerLight, new Rectangle(GlobalRectangle.X - CornerLight.Width, (int)screenPos.Y - CornerLight.Height/2, CornerLight.Width, CornerLight.Height), new Color(255, 242, 0, 255));

			sb.Draw(Game.RenderSystem.WhiteTexture, GlobalRectangle, new Color(30, 37, 43, 255));
			sb.Draw(CornerDark, new Rectangle(GlobalRectangle.X - CornerDark.Width, (int)screenPos.Y - CornerDark.Height / 2, CornerDark.Width, CornerDark.Height), new Color(30, 37, 43, 255));

			// CIty
			var sizeText = Font.MeasureString(Text);
			var xText = GlobalRectangle.X + gu * 4;
			var yText = GlobalRectangle.Y + Height/2 + Font.CapHeight / 2;


			// Count BIO
			var sizeBioText = Font.MeasureString(CountBiozard);
			var xBioText = xText + sizeText.Width +gu * 6;
			var yBioText = GlobalRectangle.Y + Height / 2 + Font.CapHeight / 2;

			// Count BIO picture
			var xBioImage = xBioText + sizeBioText.Width + gu*2;
			var yBioImage = GlobalRectangle.Y + Height / 2 - Biohazard.Height / 2;


			// Count HEA
			var sizeHeaText = Font.MeasureString(CountHealth);
			var xHeaText = xBioImage + Biohazard.Width + gu*4;
			var yHeaText = GlobalRectangle.Y + Height / 2 + Font.CapHeight / 2;

			// Count HEA picture
			var xHeaImage = xHeaText + sizeHeaText.Width + gu*2;
			var yHeaImage = GlobalRectangle.Y + Height / 2 - Health.Height / 2;

			Width = xHeaImage + Health.Width + gu*4 - GlobalRectangle.X;

			Font.DrawString(sb, Text, xText, yText, Color.White);
			Font.DrawString(sb, CountBiozard, xBioText, yBioText, Color.White);
			sb.Draw(Biohazard, xBioImage, yBioImage, Biohazard.Width, Biohazard.Height, ColorConstant.Biohazard);
			Font.DrawString(sb, CountHealth, xHeaText, yHeaText, Color.White);
			sb.Draw(Health, xHeaImage, yHeaImage, Health.Width, Health.Height, ColorConstant.Health);
		}


		public Rectangle GetRectangle(GlobeCamera camera)
		{
			var screenPos = camera.CartesianToScreen(cartesianPosition);
			return new Rectangle((int)screenPos.X + ConstantFrameUI.gridUnits + 3 * ConstantFrameUI.gridUnits - 4 * ConstantFrameUI.gridUnits, (int)screenPos.Y - Height / 2 - 4 * ConstantFrameUI.gridUnits, Width + 8 * ConstantFrameUI.gridUnits, Height + 8 * ConstantFrameUI.gridUnits);
		}


#if false
		string pathToFileWithContent;

		public DiscTexture	InfoTexture { get; protected set; }
		public Video		VideoFile	{ get; protected set; }

		VideoPlayer player = null;

		Vector2 infoTextureSize;
		Vector2 videoSize;

		DiscTexture bannerBackGround;
		DiscTexture corner;


		//public GeoBanner(Game game, DVector2 lonLatPosRad, string path)
		//{
		//	Game = game;
		//
		//	LonLatPositionRad		= lonLatPosRad;
		//	pathToFileWithContent	= path;
		//}

		public GeoBanner(Game game, DVector2 lonLatPosRad, string imageFileName, string videoFileName, int width)
		{
			Game = game;

			LonLatPositionRad	= lonLatPosRad;
			bannerBackGround	= Game.Content.Load<DiscTexture>("corners_wo_shadow2");
			corner				= Game.Content.Load<DiscTexture>("corner");

			if (imageFileName == "") InfoTexture = null;
			else InfoTexture = Game.Content.Load<DiscTexture>(imageFileName);

			if (videoFileName == "") VideoFile = null;
			else
			{
				player		= new VideoPlayer();
				VideoFile	= new Video(videoFileName);
			}


			Width = width;
		}


		Vector2 infoOffset = new Vector2(20, 9);
		public void Update(GlobeCamera camera, SpriteLayer layer)
		{
			var screenPos = camera.CartesianToScreen(cartesianPosition);

			if (!camera.Viewport.Bounds.Contains(screenPos)) return;

			if (IsVisible) {
				layer.Draw(Game.RenderSystem.WhiteTexture, new RectangleF(screenPos.X - 10, screenPos.Y - 10, 20, 20), Color.Red);
				//layer.Draw(Game.RenderSystem.WhiteTexture, new RectangleF(screenPos.X, screenPos.Y, 5, 5), Color.Green);
			}

			if(IsShowInfo && (InfoTexture != null || VideoFile != null)) {
				var startPoint = screenPos + infoOffset;

				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X, (int)startPoint.Y, Width, Height), new Rectangle(35,35, 1, 1), Color.White);
				
				layer.Draw(corner, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)infoOffset.X, (int)infoOffset.Y + 10), Color.White);
				
				// Corners
				//layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X - 13, (int)startPoint.Y - 13, 13, 13),			new Rectangle(0, 0, 13, 13), Color.White); // left top
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X - 9, (int)startPoint.Y - 9, 9, 9),				new Rectangle(35, 35, 1, 1), Color.White); // left top
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X + Width, (int)startPoint.Y - 13, 13, 13),		new Rectangle(57, 0, 13, 13), Color.White); // right top
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X - 13, (int)startPoint.Y + Height, 13, 13),		new Rectangle(0, 57, 13, 13), Color.White); // left bottom
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X + Width, (int)startPoint.Y + Height, 13, 13),	new Rectangle(57, 57, 13, 13), Color.White);
				// Sides
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X - 9, (int)startPoint.Y, 9 , Height),		new Rectangle(35, 35, 1, 1), Color.White); // left
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X, (int)startPoint.Y - 9, Width, 9),			new Rectangle(35, 35, 1, 1), Color.White); // top
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X + Width, (int)startPoint.Y, 9, Height),	new Rectangle(35, 35, 1, 1), Color.White); // right
				layer.Draw(bannerBackGround, new Rectangle((int)startPoint.X, (int)startPoint.Y + Height, Width, 9),	new Rectangle(35, 35, 1, 1), Color.White); // bottom

				// Draw data
				var point = startPoint;

				if(VideoFile != null && player != null) {
					var videoFrame = player.GetTexture();
					layer.Draw(videoFrame, new Rectangle((int)point.X, (int)point.Y, (int)videoSize.X, (int)videoSize.Y), Color.White);
					point.Y += videoSize.Y + distanceBetweenVideoAndImage;
                }
				if(InfoTexture != null) layer.Draw(InfoTexture, new Rectangle((int)point.X, (int)point.Y, (int)infoTextureSize.X, (int)infoTextureSize.Y), Color.White);
			}


		}


		int distanceBetweenVideoAndImage = 10;
		void CalculateHeightAndSizes()
		{
			videoSize = infoTextureSize = Vector2.Zero;

			if(VideoFile != null) {
				videoSize.X = width;
				float aspect	= (float)width / (float)VideoFile.Width;
				videoSize.Y = VideoFile.Height * aspect;
            }

			if(InfoTexture != null) {
				infoTextureSize.X	= width;
				float aspect			= (float)width / (float)InfoTexture.Width;
				infoTextureSize.Y	= InfoTexture.Height * aspect;
			}

			Height = (int)(videoSize.Y + (videoSize.Y == 0 ? 0 : distanceBetweenVideoAndImage) + infoTextureSize.Y);
        }


		public void ToggleShowInfo()
		{
			IsShowInfo = !IsShowInfo;

			if(!IsShowInfo)
			{
				if(player != null && VideoFile != null) {
					player.Stop();
				}
				return;
			} else {
				if (player != null && VideoFile != null) {
					player.Play(VideoFile);
				}
			}

		}

#endif

	}
}
