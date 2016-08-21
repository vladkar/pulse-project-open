using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class Slider : Frame, INotifyPropertyChanged
    {
        public float MinValue = 0;
        public float MaxValue = 1;
        
        public float Value { get; set; }

        public int SliderWidth = 5;
        public int widthControlElement = 20;
        public int heightControlElement = 20;

        private bool IsDrag = false;
        private bool IsHovered = false;
        public Color backColorForSlider;
        public Color HoverColor { get; set; }
        public Color HoverForeColor { get; set; }

        public bool ShowResult = false;

        public Slider(FrameProcessor ui) : base(ui)
        {
            Text = "";
            initAllEvent();
        }

        public Slider(FrameProcessor ui, int x, int y, int w, int h, string text,  Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.BackColor = Color.Zero;
            initAllEvent();
        }

        void initAllEvent()
        {
            // moving slider
            MouseMove += Slider_MouseMove;
            MouseDown += Slider_MouseDown;
            MouseUp += Slider_Click;

            // hovered
            MouseIn += Slider_MouseIn;
            MouseOut += Slider_MouseOut;
        }

        void Slider_MouseDown(object sender, EventArgs e)
        {
            ChangeSliderPosition();
            IsDrag = true;
        }

        void Slider_Click(object sender, EventArgs e)
        {
            IsDrag = false;
        }

        void Slider_MouseIn(object sender, EventArgs e)
        {
            IsHovered = true;
        }

        void Slider_MouseOut(object sender, EventArgs e)
        {
            IsHovered = false;
        }
        void Slider_MouseMove(object sender, EventArgs e)
        {
            if (IsDrag)
            {
                ChangeSliderPosition();
            }
        }

        void ChangeSliderPosition()
        {
            
            var p = new Vector2
            {
                X = this.Game.Mouse.Position.X - this.GlobalRectangle.X,
                Y = this.Game.Mouse.Position.Y - this.GlobalRectangle.Y,
            };
            Value = ((p.X - this.PaddingLeft) / (float)(GlobalRectangle.Width - this.PaddingLeft * 2)) * (MaxValue - MinValue) + MinValue;
            Value = MathUtil.Clamp(Value, MinValue, MaxValue);
            OnPropertyChanged("Value");

            var r = this.Font.MeasureString(Text);
            var w = r.Width + 2 * this.PaddingLeft;
            var h = r.Height + 2 * this.PaddingBottom + SliderWidth;
        
            Width = Math.Max(w, Width);
            Height = Math.Max(h, Height);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            
            int vw = (int)((GlobalRectangle.Width - this.PaddingLeft * 2) * ((Value - MinValue) / (MaxValue - MinValue)));

            int x = GlobalRectangle.X + this.PaddingLeft;
            int y = GlobalRectangle.Y + this.Font.LineHeight;
            int w = GlobalRectangle.Width - 2 * this.PaddingLeft;
            int h = SliderWidth;
            var whiteTex = Game.RenderSystem.WhiteTexture;

            sb.Draw(whiteTex, new Rectangle(x, y, w, h), /*IsHovered ? HoverColor :*/ backColorForSlider);
            sb.Draw(whiteTex, new Rectangle(x, y, vw, h),/* IsHovered ?*/ HoverForeColor /*: ForeColor*/);
            if (Image != null)
            {
                sb.Draw(Image, new Rectangle(GlobalRectangle.X + vw-widthControlElement/2 + SliderWidth/2, y-heightControlElement/2, widthControlElement, heightControlElement), ColorConstant.HoverForeColor);
            }

            var outText = ShowResult ? $"{Text}  {this.Value:0.00}": Text;
            this.Font.DrawString(sb, outText, GlobalRectangle.X + this.PaddingLeft, GlobalRectangle.Y + this.PaddingTop, ColorConstant.TextColor, clipRectIndex, 0, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
