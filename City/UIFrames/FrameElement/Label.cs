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
    class Label : Frame
    {
        public bool IsFlip = false;
        public Label(FrameProcessor ui) : base(ui)
        {
        }

        public Label(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            Font.DrawString(sb, Text, this.GlobalRectangle.X, this.GlobalRectangle.Y, Color.White, 0, 0, false, IsFlip);
        }
    }
}
