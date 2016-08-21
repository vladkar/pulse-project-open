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
    class Legenda : Frame
    {

        public string Label { get; set; }
        public string MinValue { get; set; }
        public string maxValue { get; set; }

        public int imageWidth { get; set; }
        public int imageHeight { get; set; }

        public Legenda(FrameProcessor ui) : base(ui)
        {
            Init();
        }

        public Legenda(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init();
        }

        private void Init()
        {
            Image = this.Game.Content.Load<DiscTexture>(@"ui\palette");
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            imageWidth = 20;
            imageHeight = this.Height - this.Font.LineHeight - 4 * ConstantFrameUI.gridUnits;

            int xImage = GlobalRectangle.X + this.Width / 2 - imageWidth / 2;
            int yImage = GlobalRectangle.Y + 4 * ConstantFrameUI.gridUnits;
            sb.Draw(Image, new Rectangle(xImage, yImage, imageWidth, imageHeight), Color.White, clipRectIndex);


            if (Label != null)
            {
                var sizeLabel = this.Font.MeasureString(Label);
                int xLabel = GlobalRectangle.X + this.Width / 2 - sizeLabel.Width / 2;
                int yLabel = GlobalRectangle.Y;
                Font.DrawString(sb, Label, xLabel, yLabel, Color.White, clipRectIndex, 0, true);
            }

            if (MinValue!=null)
            {
                var minValueSize = this.Font.MeasureString(MinValue);
                int xMinValue = xImage - minValueSize.Width - 2 * ConstantFrameUI.gridUnits;
                int yMinValue = yImage + imageHeight - 2;
                Font.DrawString(sb, MinValue, xMinValue, yMinValue, Color.White, clipRectIndex, 0, true);
            }


            if (maxValue != null)
            {
                var maxValueSize = this.Font.MeasureString(maxValue);
                int xMaxValue = xImage - maxValueSize.Width - 2*ConstantFrameUI.gridUnits;
                int yMaxValue = yImage + maxValueSize.Height - 2;
                Font.DrawString(sb, maxValue, xMaxValue, yMaxValue, Color.White, clipRectIndex, 0, true);
            }
        }
    }
}
