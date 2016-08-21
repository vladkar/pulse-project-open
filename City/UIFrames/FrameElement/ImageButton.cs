using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class ImageButton : Button
    {
        public int sizeImageX { get; set; }
        public int sizeImageY { get; set; }
        public Color ColorImage { get; set; }
        public ImageButton(FrameProcessor ui) : base(ui)
        {
        }

        public ImageButton(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            int x = GlobalRectangle.X + this.Width / 2 - sizeImageX / 2;
            int y = GlobalRectangle.Y + this.Height / 2 - sizeImageY / 2;  //h/2 - ConstantFrameUI.sizeCheckbox/2

//            var whiteTex = Game.RenderSystem.WhiteTexture;
//            sb.Draw(whiteTex, new Rectangle(x, y, size, size), BackColor);
            sb.Draw(Image, new Rectangle(x, y, sizeImageX, sizeImageY), ColorImage, clipRectIndex);
//            this.Font.DrawString(sb, Text, GlobalRectangle.X + this.PaddingLeft + size * 2, GlobalRectangle.Y + this.Font.MeasureString(Text).Height, ForeColor);
        }
    }
}
