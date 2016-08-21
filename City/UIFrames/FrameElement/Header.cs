using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    class Header : Frame
    {
        private bool IsDrag = false;
        public Vector2? PrevPosition = null;
        public Header(FrameProcessor ui) : base(ui)
        {
            initAllEvent();
        }

        public Header(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            initAllEvent();
        }

        void initAllEvent()
        {
            // moving slider
            MouseMove += Header_MouseMove;
            MouseDown += Header_MouseDown;
            MouseUp += Header_Click;
        }

        void Header_MouseDown(object sender, EventArgs e)
        {
            IsDrag = true;
        }

        void Header_Click(object sender, EventArgs e)
        {
            IsDrag = false;
            PrevPosition = null;
        }

        void Header_MouseMove(object sender, EventArgs e)
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
            var grandparent = this.Parent.Parent.GlobalRectangle;
            var parent = this.Parent.GlobalRectangle;
            if (grandparent.X < parent.X +(int)p.X - (int)PrevPosition.Value.X && grandparent.Width + grandparent.X > parent.Width + parent.X + (int)p.X - (int)PrevPosition.Value.X)
                this.Parent.X += (int)p.X - (int)PrevPosition.Value.X;
            if (grandparent.Y < parent.Y + (int)p.Y - (int)PrevPosition.Value.Y && grandparent.Height + grandparent.Y > parent.Height + parent.Y + (int)p.Y - (int)PrevPosition.Value.Y)
                this.Parent.Y += (int)p.Y - (int)PrevPosition.Value.Y;

            PrevPosition = p;
        }
    }
}
