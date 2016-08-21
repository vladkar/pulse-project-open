using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.Impl
{
    public class ComplexLegend : Frame
    {
        private FrameProcessor ui;
        private ListBox ListLegend;

        public ComplexLegend(FrameProcessor ui) : base(ui)
        {
            Init(ui);
        }

        public ComplexLegend(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init(ui);
        }

        private void Init(FrameProcessor ui)
        {
            this.ui = ui;
            ListLegend = new ListBox(ui)
            {
                IsHoriz = true
            };
            this.Add(ListLegend);
        }

        public void addNewELement(Frame frame)
        {
            this.Height = Math.Max(this.Height, frame.Height);
            this.Width += frame.Width;
            this.X -= frame.Width;
            ListLegend.addElement(frame);
        }
    }
}
