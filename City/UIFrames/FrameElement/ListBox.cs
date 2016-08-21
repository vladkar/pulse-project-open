using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    class ListBox : Frame
    {
        private FrameProcessor ui;
        public int step = 0;
        public int shift = 0;

        public bool IsHoriz = false;

        private Header header;
        public ListBox(FrameProcessor ui, bool withHeader = false) : base(ui)
        {
            init(ui, withHeader);
        }

        public ListBox(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor, bool withHeader = false) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui, withHeader);
        }

        private void init(FrameProcessor ui, bool withHeader)
        {
            this.ui = ui;
            Text = null;
            Width = 0;
            Height = 0;
            if (!withHeader)
                return;
            header = new Header(ui, 0, 0, 0, ConstantFrameUI.HeaderHeight, "", ColorConstant.BorderColor);
            this.Add(header);
            this.Height += header.Height;
        }

        public void addElement(Frame element)
        {
            initNode(element);
            this.Add(element);
        }

        public void RemoveElement(Frame expandPanel)
        {
            this.Remove(expandPanel);
            this.Height -= expandPanel.Height;
        }

        public void addListElement(List<Frame> listElement)
        {
            foreach (var elem in listElement)
            {
                addElement(elem);
            }
        }

        private void initNode(Frame node)
        {
            if (IsHoriz)
            {
                node.X = this.Width;
                node.Y = 0;
                this.Width = node.Width + this.Width + shift;
                this.Height = Math.Max(node.Height, this.Height + shift);
            }
            else
            {
                node.X = shift; 
                node.Y = this.Height + shift / 2;
                this.Width = Math.Max(node.Width, this.Width + shift);
                this.Height += node.Height + step;
            }
            

            if (header==null)
                return;
            header.Width = this.Width;
        }

    }
}
