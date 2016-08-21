using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames.FrameElement
{
    public class ImageWithText : Button {
        public bool IsCenter = false;
        public int offset = 15;
        public int sizePicture { get; set; }
        public Color imageColor { get; set; }

        public ImageWithText(FrameProcessor ui) : base(ui)
        {
            Init();
        }

        public ImageWithText(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init();
        }

        private void Init()
        {
            if(sizePicture==0)
                sizePicture = this.Height;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            int startOffsetX = 0;
            if(IsCenter)
            {
                var sizeText = this.Font.MeasureString(Text);
                int widthElement = sizeText.Width + sizePicture + offset;
                startOffsetX = (this.Width - widthElement)/2;
            }

            
            int xImage = GlobalRectangle.X + PaddingLeft + startOffsetX;
            int yImage = GlobalRectangle.Y + this.Height / 2 - sizePicture / 2;

            int xText = GlobalRectangle.X + PaddingLeft + sizePicture + offset + startOffsetX;
            int yText = GlobalRectangle.Y + this.Height/2 + this.Font.CapHeight/2;

            sb.Draw(Image, new Rectangle(xImage, yImage, sizePicture, sizePicture), imageColor!=null? imageColor: Color.White, 0);
            this.Font.DrawString(sb, Text, xText, yText, Color.White, 0);
        }
    }
}
