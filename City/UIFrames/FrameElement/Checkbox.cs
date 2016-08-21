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
    public class Checkbox : Frame
    {

        public Texture Checked;
        public Texture CheckedSolid;
        public Texture None;
        public Texture NoneSolid;

//        public Color HoverColor { get; set; }
        public Color ColorCheckbox { get; set; }
        public Color ActiveColor { get; set; }
        public bool IsChecked;
        public bool IsHovered;

        public Checkbox(FrameProcessor ui) : base(ui)
        {
            Text = "";
            init(ui);
        }

        public Checkbox(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        void init(FrameProcessor ui)
        {
            IsHovered = false;
            IsChecked = false;
           // this.Anchor = FrameAnchor.Bottom | FrameAnchor.Top;
            this.ImageMode = FrameImageMode.Stretched;
            this.MouseIn += ChangeBackColor;
            this.MouseOut += ChangeBackColor;
            this.Click += Checkbox_Click;

            this.Image = None;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            //this.Width = ConstantFrameUI.sizeCheckbox + this.Font.MeasureString(Text).Width + ConstantFrameUI.sizeCheckbox * 2;
            // this.Height = ConstantFrameUI.sizeCheckbox + this.Font.MeasureString(Text).Height;

            int sizeCheck = ConstantFrameUI.sizeCheckbox;
            int xCheck = GlobalRectangle.X + this.PaddingLeft;
            int yCheck = GlobalRectangle.Y + this.Height / 2 - sizeCheck/2;

            int xText = GlobalRectangle.X + this.PaddingLeft + sizeCheck + 20;
            int yText = GlobalRectangle.Y + this.Height / 2 + this.Font.LineHeight/2 - 3;
            

            var whiteTex = Game.RenderSystem.WhiteTexture;
//            sb.Draw(whiteTex, new Rectangle(x, y, size, size), ColorCheckbox);
            sb.Draw(IsHovered ? (IsChecked ? CheckedSolid: NoneSolid) : (IsChecked ? Checked : None), new Rectangle(xCheck, yCheck, sizeCheck, sizeCheck), IsChecked ? ActiveColor:ColorCheckbox, clipRectIndex);
            this.Font.DrawString(sb, Text, xText, yText, ForeColor, clipRectIndex);
        }


        void Checkbox_Click(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;
        }

        void ChangeBackColor(object sender, EventArgs e)
        {
            IsHovered = !IsHovered;
        }
        
    }
}
