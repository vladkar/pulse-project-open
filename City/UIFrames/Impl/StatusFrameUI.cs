using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.Impl
{
    class StatusFrameUI : Frame
    {

        public StatusFrameUI(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public StatusFrameUI(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        public void init(FrameProcessor ui)
        {
            this.X = ConstantFrameUI.statusFrameUIX;
            this.Y = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.statusFrameUIHeight;
            this.Width = this.Game.RenderSystem.DisplayBounds.Width;
            this.Height = ConstantFrameUI.statusFrameUIHeight;

            this.Name = ConstantFrameUI.statusPanelName;
            this.BackColor = ColorConstant.BackColor;

            this.Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right;

            addControlElement(ui);
        }

        private void addControlElement(FrameProcessor ui)
        {
            var widthButton = 27 * ConstantFrameUI.gridUnits;
            var heightButton = this.Height;
            var timePattern = "hh:mm";
            var timeButton = new Frame(ui, this.Width - widthButton, 0, widthButton, heightButton, DateTime.Now.ToString(timePattern), ColorConstant.Zero)
            {
                //TODO: text.style = Title
                Padding = 5,
                Anchor = FrameAnchor.Right,
                TextAlignment = Alignment.MiddleCenter,
                OverallColor = ColorConstant.TextStatus,
                Font = ConstantFrameUI.segoeLight34
            };
            timeButton.Tick += (sender, args) =>
            {
                timeButton.Text = DateTime.Now.ToString(timePattern);
            };
            this.Add(timeButton);

            var widthButtonlayerStatus = 70 * ConstantFrameUI.gridUnits;
            var LayerStatusButton = new Button(ui, this.Width - widthButton - widthButtonlayerStatus, 0, widthButtonlayerStatus, heightButton, "", ColorConstant.Zero)
            {
                //TODO: text.style = Title
//                HoverColor = ColorConstant.Gray,
//                ClickColor = ColorConstant.Blue,
                Anchor = FrameAnchor.Right,
                TextAlignment = Alignment.MiddleLeft,
                OverallColor = Color.White,
            };
            this.Add(LayerStatusButton);

//            var langaugeDropDown = FrameHelper.createDropDownList(ui, this.Game.RenderSystem.DisplayBounds.Width - ConstantFrameUI.languageWidth, 0,
//            ConstantFrameUI.languageWidth, this.Height, ConstantFrameUI.DefaultDictionary, FrameHelper.languageList,
//            FrameHelper.setDictionary);
//            this.Add(langaugeDropDown);
        }
    }
}
