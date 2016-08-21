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
    class ButtonIcon : Button
    {
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public Color ColorImage { get; set; }
        public bool IsActive;

        public Color ColorActiveButton { get; set; }
        public Color HoverColor { get; set; }
        public Color DefaultBackColor { get; set; }

        public int shift = 5;


        public ButtonIcon(FrameProcessor ui) : base(ui)
        {
            Init();
        }

        public ButtonIcon(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init();
        }

        private void Init()
        {
            DefaultBackColor = BackColor;
            this.StatusChanged += (sender, args) =>
            {
                switch (args.Status)
                {
                    case FrameStatus.Hovered:
                        BackColor = IsActive ? ColorActiveButton : HoverColor;
                        break;
                    case FrameStatus.None:
                        BackColor = IsActive ? ColorActiveButton : DefaultBackColor;
                        break;
                    case FrameStatus.Pushed:
                        IsActive = !IsActive;
                        BackColor = ColorActiveButton;
                        break;
                }
            };
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            int xImage = GlobalRectangle.X + this.Width/2 - ImageWidth/2;
            int yImage = GlobalRectangle.Y + (this.Height - ImageHeight - this.Font.MeasureString(Text).Height)/2;

            int xText = GlobalRectangle.X + this.Width/2 - this.Font.MeasureString(Text).Width/2;
            int yText = yImage + ImageHeight + this.Font.MeasureString(Text).Height/2 + shift;
            sb.Draw(Image, new Rectangle(xImage, yImage, ImageWidth, ImageHeight), ColorImage, clipRectIndex);
            this.Font.DrawString(sb, Text, xText, yText, ForeColor, clipRectIndex);
        }

        public void ChangeStatus(bool isActive)
        {
            IsActive = isActive;
            BackColor = isActive ? ColorActiveButton : DefaultBackColor;
        }
    }
}
