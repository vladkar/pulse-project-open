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
    class TrainMapLabel : Frame
    {
        public Texture biohazard;
        public Texture health;
        public string countBiozard;
        public string countHealth;

        public TrainMapLabel(FrameProcessor ui) : base(ui)
        {
        }

        public TrainMapLabel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            // CIty
            var sizeText = Font.MeasureString(Text);
            var xText = this.GlobalRectangle.X + ConstantFrameUI.mapLegendMapOffset * 2;
            var yText = this.GlobalRectangle.Y +  ConstantFrameUI.mapLegendMapHeight / 2 + sizeText.Height/3;

            // Count BIO
            var sizeBioText = Font.MeasureString(countBiozard);
            var xBioText = xText + sizeText.Width + ConstantFrameUI.mapLegendMapOffset * 3;
            var yBioText = this.GlobalRectangle.Y + ConstantFrameUI.mapLegendMapHeight / 2 + sizeBioText.Height/3;

            // Count BIO picture
            var xBioImage = xBioText + sizeBioText.Width + ConstantFrameUI.mapLegendMapOffset;
            var yBioImage = this.GlobalRectangle.Y + ConstantFrameUI.mapLegendMapHeight / 2 - biohazard.Height / 2;


            // Count HEA
            var sizeHeaText = Font.MeasureString(countHealth);
            var xHeaText = xBioImage + biohazard.Width + ConstantFrameUI.mapLegendMapOffset * 2;
            var yHeaText = this.GlobalRectangle.Y + ConstantFrameUI.mapLegendMapHeight / 2 + sizeHeaText.Height/3;

            // Count HEA picture
            var xHeaImage = xHeaText + sizeHeaText.Width + ConstantFrameUI.mapLegendMapOffset;
            var yHeaImage = this.GlobalRectangle.Y + ConstantFrameUI.mapLegendMapHeight / 2 - health.Height / 2;

            this.Width = sizeText.Width + sizeBioText.Width + biohazard.Width + sizeHeaText.Width + health.Width +
                         ConstantFrameUI.mapLegendMapOffset*11;
            this.Width = sizeText.Width + sizeBioText.Width + biohazard.Width + sizeHeaText.Width + health.Width +
             ConstantFrameUI.mapLegendMapOffset * 11;

            Font.DrawString(sb, Text, xText, yText, Color.White);
            Font.DrawString(sb, countBiozard, xBioText, yBioText, Color.White);
            sb.Draw(biohazard, xBioImage, yBioImage, biohazard.Width, biohazard.Height, ColorConstant.Biohazard);
            Font.DrawString(sb, countHealth, xHeaText, yHeaText, Color.White);
            sb.Draw(health, xHeaImage, yHeaImage, health.Width, health.Height, ColorConstant.Health);
        }
    }
}
