using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Newtonsoft.Json;

namespace City.UIFrames.FrameElement
{
    public class PlayerControlPanel : Frame
    {
        private List<Slider> listSlider;
        private Button playButton;
        private FrameProcessor ui;

        private int paddingLeftRight = 45*ConstantFrameUI.gridUnits;
        private int sizeButton = 12 * ConstantFrameUI.gridUnits;
        private int HeightSlider = 30;
        private int offsetSlider = 2*ConstantFrameUI.gridUnits;
        private int offsetText = 4 * ConstantFrameUI.gridUnits;
        private int offsetTop = 10*ConstantFrameUI.gridUnits;
        private int offsetBottom = 2 * ConstantFrameUI.gridUnits;
        private int heightCurrentTime = 7 * ConstantFrameUI.gridUnits;

        private int ControlElementWidthSlider = 7*ConstantFrameUI.gridUnits;
        private int ControlElementHeightSlider = 7 * ConstantFrameUI.gridUnits;
        private int widthSlider = ConstantFrameUI.gridUnits/2;

        public Color colorText = ColorConstant.TextColor;

        public PlayerControlPanel(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public PlayerControlPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        void init(FrameProcessor ui)
        {
            this.ui = ui;
            this.Height = offsetTop + sizeButton + offsetTop - heightCurrentTime + offsetBottom;
            listSlider = new List<Slider>();
            this.Font = ConstantFrameUI.segoeReg12;
            createControlButtons();
        }

        private void createControlButtons()
        {
            playButton = new Button(ui, this.Width/2 - sizeButton/2, this.Height - sizeButton - offsetBottom, sizeButton,
                sizeButton, "", ColorConstant.Zero)
            {
                Image = this.Game.Content.Load<DiscTexture>(@"ui\play"),
                HoverColor = ColorConstant.HoverColor
            };
            this.Add(playButton);
        }

        public void addSlider(string name, float minValue, float maxValue, float currentValue, Action<float> action)
        {
            var newSlider = FrameHelper.createSlider(ui, 
                paddingLeftRight, offsetTop + (HeightSlider + offsetSlider) * listSlider.Count,
                this.Width - paddingLeftRight * 2, HeightSlider,
                "",
                minValue, maxValue, currentValue, 
                action);
            newSlider.Name = name;
            newSlider.Image = this.Game.Content.Load<DiscTexture>("big_circle");
            newSlider.SliderWidth = widthSlider;
            newSlider.widthControlElement = ControlElementWidthSlider;
            newSlider.heightControlElement = ControlElementHeightSlider;
            listSlider.Add(newSlider);
            this.Add(newSlider);

            playButton.Y += HeightSlider + offsetSlider;

            this.Y -= HeightSlider + offsetSlider;
            this.Height += HeightSlider + offsetSlider;
        }

        public void addSlider(Slider slider)
        {
            slider.Width = this.Width - paddingLeftRight * 2;
            slider.Height = HeightSlider;

            slider.X = paddingLeftRight;
            slider.Y = offsetTop + (HeightSlider + offsetSlider) * listSlider.Count;

            listSlider.Add(slider);

            playButton.Y += HeightSlider + offsetSlider;
            this.Y -= HeightSlider + offsetSlider;
            this.Height += HeightSlider + offsetSlider;
            this.Add(slider);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            var stringCurrentTime = "12:00";
            var sizeCurrentTime = this.Font.MeasureString(stringCurrentTime);
            var xCurrentTime = this.GlobalRectangle.X + this.Width/2 - sizeCurrentTime.Width/2;
            var yCurrentTime = this.GlobalRectangle.Y + heightCurrentTime/2 + sizeCurrentTime.Height;
            this.Font.DrawString(sb, stringCurrentTime, xCurrentTime, yCurrentTime, colorText, clipRectIndex);

            foreach (var slider in listSlider)
            {
                var name = slider.Name;
                var sizeName = this.Font.MeasureString(name);
                var xText = this.GlobalRectangle.X + paddingLeftRight - offsetText - sizeName.Width;
                var yText = this.GlobalRectangle.Y + slider.Y + slider.Height / 2 - sizeName.Height/2 + slider.Font.CapHeight ;
                this.Font.DrawString(sb, name, xText, yText, colorText, clipRectIndex);
            }
            var currentSlider = listSlider.Last();
            var startTime = "00:00";
            var sizeStartTime = this.Font.MeasureString(startTime);
            var xStartTime = this.GlobalRectangle.X + currentSlider.X;
            var yStartTime = this.GlobalRectangle.Y + currentSlider.Y + currentSlider.Height + sizeStartTime.Height/2;
            this.Font.DrawString(sb, startTime, xStartTime, yStartTime, colorText, clipRectIndex);

            var endTime = "23:59";
            var sizeEndTime = this.Font.MeasureString(endTime);
            this.Font.DrawString(sb, endTime, xStartTime + currentSlider.Width - sizeEndTime.Width, yStartTime, colorText, clipRectIndex);
        }
    }
}
