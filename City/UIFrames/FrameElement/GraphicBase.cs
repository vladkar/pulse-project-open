using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement.ModelElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class GraphicBase : Frame
    {
        private FrameProcessor ui;
        // TODO: empty font
        public SpriteFont legendFont;
        public SpriteFont WorkSpaceNameFont;

        private bool showLegend;    // TODO: create Legend field

        public string GraphicName;   
        public string rowName;     
        public string columnName;

        private Frame GraphicNameFrame;
        private Frame RowNameFrame;
        private Frame ColumnNameFrame;


        private Frame workSpace;
        private Plot plot;

        public GraphicBase(FrameProcessor ui) : base(ui)
        {
            this.ui = ui;
        }

        public GraphicBase(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
        }

        public void init(Frame workSpace)
        {
            legendFont = ConstantFrameUI.sfReg15;
            this.workSpace = workSpace;
            this.Add(workSpace);
            addNameBarChart();
            addLabels();
        }

        public void init(Plot plot)
        {
            legendFont = ConstantFrameUI.sfReg12;
            this.workSpace = (Frame)plot;
            this.plot = plot;
            this.Add(plot);
            addNameBarChart();
            //addLabels();
            addLegend();
        }

        public void addNameBarChart()
        {
            if ("".Equals(GraphicName))
                return;
            var font = WorkSpaceNameFont ?? this.Font;
            var sizeText = font.MeasureString(GraphicName);
            GraphicNameFrame = new Frame(ui, this.Width / 2 - sizeText.Width / 2, 0, sizeText.Width, sizeText.Height, GraphicName, Color.Zero)
            {
                Font = font,
            };
            this.Add(GraphicNameFrame);
        }

        private void addLabels()
        {
            if ("".Equals(rowName) || "".Equals(columnName))
                return;
            // axe X
            var columnSizeText = this.Font.MeasureString(columnName);
            ColumnNameFrame = FrameHelper.createLabel(ui, workSpace.X + workSpace.Width / 2 - columnSizeText.Width / 2, workSpace.Y + workSpace.Height + columnSizeText.Height, columnName);
            this.Add(ColumnNameFrame);
            // axe Y
            var rowSizeText = this.Font.MeasureString(rowName);
            RowNameFrame = FrameHelper.createLabel(ui, workSpace.X - rowSizeText.Height * 3 / 2 - 15, workSpace.Y + workSpace.Height / 2 + rowSizeText.Width / 2, rowName, true);
            this.Add(RowNameFrame);
        }

        private List<Frame> legendList;
        private void addLegend()
        {
            if (plot == null)
                return;

            var widthElement = this.Width/plot.configPlots.Count;
            var textureChecked = ui.Game.Content.Load<DiscTexture>(@"ui\legend-checked");
            var textureUnchecked = ui.Game.Content.Load<DiscTexture>(@"ui\legend-unchecked");
            legendList = new List<Frame>();
            var i = 0;
            foreach (var config in plot.configPlots) 
            {
                Frame icon = new ImageWithText(ui, widthElement * i, this.Height - ConstantFrameUI.gridUnits * 2, widthElement, ConstantFrameUI.gridUnits*2, config.Value.legend,
                    ColorConstant.Zero)
                {
                    Image = textureChecked,
                    sizePicture = textureChecked.Width,
                    imageColor = config.Value.color,
                    IsCenter = true,
                    Font = legendFont,
                    offset = 7
                };
                icon.Click += (e, a) => {
                    plot.configPlots[config.Key].visible = !plot.configPlots[config.Key].visible;
                    icon.Image = plot.configPlots[config.Key].visible ? textureChecked : textureUnchecked;
                };
                this.Add(icon);
                legendList.Add(icon);
                i++;
            }
            
        }

        public void addPointToPlot(Vector2 value, int numberPlot = 0)
        {
            (workSpace as Plot)?.addPoint(value, numberPlot);
        }

        public void addColor(int value, ConfigPlot config)
        {
            (workSpace as Plot)?.configPlots.Add(value, config);
        }

        public void addPlot(List<Vector2> value)
        {
            (workSpace as Plot)?.addPlot(value);
        }

        public void setLimitsPlot(float minCountOfPointPercent, float maxCountofPointPercent)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.limitMinPointPercent = minCountOfPointPercent;
                plot.limitMaxPointPercent = maxCountofPointPercent;
            }
        }

        public void setRangePointPlot(int rangePoint)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.RangePoint = rangePoint;
            }
        }
        public void setAddValueForMax(float addValueForMax)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.AddValueForMax = addValueForMax;
            }
        }

        public void setConfig(bool isFill, bool isAxisX, bool isAxisY, bool IsMainAxisX, bool IsTime)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.IsFilled = isFill;
                plot.IsAxisX = isAxisX;
                plot.IsAxisY = isAxisY;
                plot.IsMainAxisX = IsMainAxisX;
                plot.IsTime = IsTime;
            }
        }

        public void removePlot(int id)
        {
            var plot = workSpace as Plot;
            plot?.RemovePlot(id);
        }

        public int getCountPlot()
        {
            var plot = workSpace as Plot;
            return plot?.getCountPlot() ?? 0;
        }

        public void resizeChild()
        { 
            workSpace.Width = this.Width - ConstantFrameUI.gridUnits * 5;
            workSpace.Height = this.Height - 10 * ConstantFrameUI.gridUnits - 10*ConstantFrameUI.gridUnits;
            workSpace.X = ConstantFrameUI.gridUnits * 5;
            workSpace.Y = 10 * ConstantFrameUI.gridUnits;
            var sizeText = GraphicNameFrame.Font.MeasureString(GraphicNameFrame.Text);
            GraphicNameFrame.X = this.Width / 2 - sizeText.Width / 2;


            if (ColumnNameFrame != null)
            {
                var columnSizeText = ColumnNameFrame.Font.MeasureString(ColumnNameFrame.Text);
                ColumnNameFrame.X = workSpace.X + workSpace.Width / 2 - columnSizeText.Width / 2;
                ColumnNameFrame.Y = workSpace.Y + workSpace.Height + columnSizeText.Height;
            }

            if (RowNameFrame != null)
            {
                var rowSizeText = RowNameFrame.Font.MeasureString(RowNameFrame.Text);
                RowNameFrame.X = workSpace.X - rowSizeText.Height*3/2 - 15;
                RowNameFrame.Y = workSpace.Y + workSpace.Height/2 + rowSizeText.Width/2;
            }

            var widthLegendElement = this.Width / plot.configPlots.Count;
            var i = 0;
            foreach (var element in legendList)
            {
                element.X = i*widthLegendElement;
                element.Width = widthLegendElement;
                i++;
            }
        }

        public void setThinknessPlot(int thickness)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.thicknessLine = thickness;
            }
        }

        public void setMaxDistanceX(float difference)
        {
            var plot = workSpace as Plot;
            if (plot != null)
            {
                plot.maxDifferenceX = difference;
            }
        }
    }
}
