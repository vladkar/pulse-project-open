using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;
using Pulse.Common.Model.Legend;

namespace City.UIFrames.FrameElement
{
    public class BarChart : Frame
    {
        public struct PairCoor
        {
            public string name { get; set; }
            public double value { get; set; }

            public PairCoor(string name, double value)
            {
                this.name = name;
                this.value = value;
            }
        }

        private bool IsGroupBarChart = true;

        private double maxValue;
        private List<List<PairCoor>> ValueList;

        private int quantityPointMax = 0;

        public int percent = 15;
        public int stepGridX = 0;
        public int stepGridY = 0;

        public BarChart(FrameProcessor ui) : base(ui)
        {
            init();
        }

        public BarChart(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init();
        }

        void init()
        {
            stepGridX = this.Width * percent / 100;
            stepGridY = this.Height * percent / 100;
            ValueList = new List<List<PairCoor>>();
            maxValue = 0;
        }

        public void addValue(List<PairCoor> BarValues)
        {
            this.ValueList.Add(BarValues);
            updateMaxValueBar();
        }

        public void addValues(List<List<PairCoor>> ValueList)
        {
            this.ValueList = ValueList;
            updateMaxValueBar();
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
			#warning PASS CLIPRECTINDEX TO THESE FUNCTION TO ENABLE CLIPPING
            drawBarChart(sb);
            drawGrid(sb);
        }

        private void drawBarChart(SpriteLayer sb)
        {
            var countPart = ValueList.First().Count;
            var step = IsGroupBarChart ? this.Width/(countPart*(2*ValueList.Count+1)) : this.Width / (2 * countPart + 1);
            var widthBar = IsGroupBarChart ? (this.Width / countPart - 2*step) / ValueList.Count : this.Width / countPart * 1/2;

            List<List<int>> heightElements = Enumerable.Range(0, quantityPointMax).Select(i => new List<int>()).ToList();

            int numberBarChart = 0;
            foreach (var list in ValueList)
            {
                var positionForBar = 0;
                Color barChartColor = ColorConstant.BarChartColor[numberBarChart];
                foreach (var pairCoor in list)
                {
                    var heightElement = getHeightBar(pairCoor.value);
                    heightElements[positionForBar].Add(heightElement);
                    int positionX = 0;
                    int positionY = 0;
                    if (!IsGroupBarChart)
                    {
                        positionX = this.GlobalRectangle.X + this.Width * (2 * positionForBar + 1)  / countPart / 2;
                        positionY = this.GlobalRectangle.Y + this.Width - heightElements[positionForBar].Sum();
                    }
                    else
                    {
                        positionX = this.GlobalRectangle.X + (this.Width / countPart) * positionForBar + (this.Width / countPart) / 2 - widthBar * ValueList.Count / 2 + widthBar * numberBarChart + step;
                        positionY = this.GlobalRectangle.Y + this.Width - heightElements[positionForBar][numberBarChart];
                    }

                    sb.Draw(Game.RenderSystem.WhiteTexture, new Rectangle(positionX - widthBar / 2, positionY, widthBar, heightElement), barChartColor);

                    // label grid
                    if (numberBarChart == 0)
                    {
                        var sizeLabel = this.Font.MeasureString(pairCoor.name);
                        this.Font.DrawString(sb, pairCoor.name, this.GlobalRectangle.X + this.Width * (2 * positionForBar + 1) / countPart / 2 - sizeLabel.Width/2, this.GlobalRectangle.Y + this.Width + sizeLabel.Height / 2, ColorConstant.TextColor);
                    }
                    

                    // shift for next barChart
                    positionForBar += 1;
                }
                numberBarChart++;
            }
        }

        private void drawGrid(SpriteLayer sb)
        {
            // grid x
            for (int y = stepGridY; y < this.Height; y += stepGridY)
            {
                var stringValue = Math.Round(maxValue / this.Height * (y), 2);
                sb.Draw(Game.RenderSystem.WhiteTexture, new Rectangle(this.GlobalRectangle.X, this.Height + this.GlobalRectangle.Y - y, this.Width, 1), ColorConstant.AxisColor);
                this.Font.DrawString(sb, stringValue.ToString(), this.GlobalRectangle.X, this.Height + this.GlobalRectangle.Y - y, ColorConstant.TextColor);
            }
            // grid Y
            for (int x = stepGridX; x < this.Height; x += stepGridX)
            {
                sb.Draw(Game.RenderSystem.WhiteTexture, new Rectangle(this.GlobalRectangle.X + x, this.GlobalRectangle.Y, 1, this.Height), ColorConstant.AxisColor);
            }
        }


        private int getHeightBar(double value)
        {
            return (int)((value/maxValue)*this.Height);
        }

        private void updateMaxValueBar()
        {
            List<double> result = Enumerable.Repeat(0.0, ValueList.First().Count).ToList();
            foreach (var list in ValueList)
            {
                if (quantityPointMax < list.Count)
                    quantityPointMax = list.Count;
                if (!IsGroupBarChart)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        result[i] += list[i].value;
                    }
                }
                else
                {
                    var max = list.Max(e => e.value);
                    if(maxValue < max)
                        maxValue = max;
                }
            }
            if(!IsGroupBarChart)
                maxValue =  result.Max();
        }
    }       
}
