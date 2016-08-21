using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    class Scroll : Frame
    {
        private bool IsDrag = false;
        public Vector2? PrevPosition = null;
        public bool IsHorizontal = true;
        public Action<float, float> actionForMove;

        public Scroll(FrameProcessor ui) : base(ui)
        {
            initAllEvent();
        }

        public Scroll(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            initAllEvent();
        }

        void initAllEvent()
        {
            // moving slider
            MouseMove += Scroll_MouseMove;
            MouseDown += Scroll_MouseDown;
            MouseUp += Scroll_Click;
        }

        void Scroll_MouseDown(object sender, EventArgs e)
        {
            IsDrag = true;
        }

        void Scroll_Click(object sender, EventArgs e)
        {
            IsDrag = false;
            PrevPosition = null;
        }

        void Scroll_MouseMove(object sender, EventArgs e)
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
                X = this.Game.Mouse.Position.X,
                Y = this.Game.Mouse.Position.Y,
            };
            if (PrevPosition == null)
            {
                PrevPosition = p;
            }
            var parent = this.Parent.GlobalRectangle;
            var scrollRectangle= this.GlobalRectangle;
            if (parent.X < scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X && parent.Width + parent.X > scrollRectangle.Width + scrollRectangle.X + (int)p.X - (int)PrevPosition.Value.X)
                this.X += (int)p.X - (int)PrevPosition.Value.X;
            if (parent.Y < scrollRectangle.Y + (int)p.Y - (int)PrevPosition.Value.Y && parent.Height + parent.Y > scrollRectangle.Height + scrollRectangle.Y + (int)p.Y - (int)PrevPosition.Value.Y)
                this.Y += (int)p.Y - (int)PrevPosition.Value.Y;
            PrevPosition = p;
            if(IsHorizontal)
                actionForMove((float)this.X/parent.Width, (float)(this.X + this.Width) / parent.Width);
            else
                actionForMove((float)this.Y / parent.Height, (float)(this.Y + this.Height) / parent.Height);
        }
    }
}
