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
    public class IconWithText : Button
    {
        public int sizePicture { get; set; }
        public string Description { get; set; }
        public bool IsActive;
        public bool IsHovered;
        public string command;
        public Color ColorActiveButton { get; set; }

        public IconWithText(FrameProcessor ui) : base(ui)
        {
            Init();
        }

        public IconWithText(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init();
        }

        private void Init()
        {
            sizePicture = this.Height;
            DefaultBackColor = BackColor;
            this.StatusChanged += (sender, args) =>
            {
                switch (args.Status)
                {
                    case FrameStatus.Hovered:
                        IsHovered = true;
                        break;
                    case FrameStatus.None:
                        IsHovered = false;
                        BackColor = IsActive ? ColorActiveButton : DefaultBackColor;
                        break;
                    case FrameStatus.Pushed:
                        IsActive = !IsActive;
                        break;
                }
            };
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            BackColor = IsActive ? ColorActiveButton : IsHovered ? HoverColor:DefaultBackColor;
            int xImage = GlobalRectangle.X + PaddingLeft;
            int yImage = GlobalRectangle.Y + this.Height/2 - sizePicture/2;

            int xText = xImage + sizePicture + 20;
            int yText = yImage + ConstantFrameUI.segoeSemiBold15.MeasureString(Text).Height / 2 + sizePicture/4;

            int xDescription = xText;
            int yDescription = yText + ConstantFrameUI.segoeSemiBold15.MeasureString(Text).Height/2 + ConstantFrameUI.segoeReg15.MeasureString(Description).Height / 2 ;

            sb.Draw(Image, new Rectangle(xImage, yImage, sizePicture, sizePicture), Color.White, clipRectIndex);
            ConstantFrameUI.segoeSemiBold15.DrawString(sb, Text, xText, yText, Color.White, clipRectIndex);
            ConstantFrameUI.segoeReg15.DrawString(sb, Description, xDescription, yDescription, new Color(255,255,255, 140), clipRectIndex);
        }
    }
}
