using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.ControlsClient.DomainClient.VKFest.UI
{
    public class NewsFeedElement : Frame {
        private FrameProcessor ui;
        public string Url;
        public bool IsLoadedPhoto = false;
        public const int SizePicture = 50;

        public const int offsetX = 10;
        public const int offsetY = 10;

        public const int offsetXElement = 20;

        private Frame photo;
        private RichTextBlock text;
        private Frame dateFrame;

        public NewsFeedElement(FrameProcessor ui) : base(ui)
        {
            Init(ui);
        }

        public NewsFeedElement(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init(ui);
        }

        private void Init(FrameProcessor ui)
        {
            this.ui = ui;
            photo = new Frame(ui, offsetX, offsetY, SizePicture, SizePicture, "", Color.Zero);
            text = new RichTextBlock(ui, offsetX + SizePicture + offsetXElement, offsetY,
                this.Width - offsetX - SizePicture - offsetXElement - offsetX, this.Height - offsetY*2, this.Text, Color.Zero, this.Font, this.Font.CapHeight);
            text.IsShortText = true;
            this.Add(photo);
            this.Add(text);
        }


        protected override void Update(GameTime gameTime)
        {
            if (!IsLoadedPhoto && Url !=null)
            {
                FrameHelper.loadPhoto(this.Game.RenderSystem, photo, Url, SizePicture);
                IsLoadedPhoto = true;
            }
           
            base.Update(gameTime);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
        }
    }
}
