using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class Button : Frame {
        public bool IsChangeStatus=true;
        public Color HoverColor { get; set; }
        public Color ClickColor { get; set; }
        public Color DefaultBackColor { get; set; }

        public Button(FrameProcessor ui) : base(ui)
        {
            init();
        }

        public Button(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init();
        }

        void init()
        {
            DefaultBackColor = BackColor;
            this.StatusChanged += (sender, args) =>
            {
                if(!IsChangeStatus)
                    return;
                switch (args.Status)
                {
                    case FrameStatus.Hovered:
                        BackColor = HoverColor;
                        break;
                    case FrameStatus.None:
                        BackColor = DefaultBackColor;
                        break;
                    case FrameStatus.Pushed:
                        BackColor = ClickColor;
                        break;
                }
            };
        }
    }
}
