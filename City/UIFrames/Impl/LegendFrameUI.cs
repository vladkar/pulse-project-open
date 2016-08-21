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
    class LegendFrameUI : Frame
    {
        public LegendFrameUI(FrameProcessor ui) : base(ui)
        {
            this.X = ConstantFrameUI.legendFrameUIX;
            this.Y = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.statusFrameUIHeight - ConstantFrameUI.legendFrameUIHeight;
            this.Width = ConstantFrameUI.legendFrameUIWidth;
            this.Height = ConstantFrameUI.legendFrameUIHeight;
            this.Name = ConstantFrameUI.legendPanelName;
            this.BackColor = ColorConstant.BackColorForm;
            this.Anchor = FrameAnchor.Bottom | FrameAnchor.Left;
        }

        public LegendFrameUI(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }
    }
}
