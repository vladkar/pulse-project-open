using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement.ModelElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;
using Pulse.Common.DeltaStamp;

namespace City.UIFrames.FrameElement
{
    public class Plot : Frame {
        public static DateTime MinDate = DateTime.Now.Date;
        public List<float> stepsForY = new List<float>(){0.5f, 1, 5, 10, 25, 50, 100, 200, 500, 1000};
        public int currentStep = 0;
        public int thicknessLine = 3;

        private Texture beamTexture;
        //fonts 
        private SpriteFont AxisTextFont;
        // main 
        double minX = 0;
        double maxX = 0;
        double minY = 0;
        double maxY = 0;
        private bool IsDrawing = true;
        private double DistanceXMax = 0;
        private double DistanceYMax = 0;
        private int pixelAxisX = 0;
        private int pixelAxisY = 0;
        private Dictionary<int, List<Vector2>> ValueList;

        public float AddValueForMax = 0;
        public bool IsTime = false;

        // for limit the field of point
        public float limitMinPointPercent = 0;
        public float limitMaxPointPercent = 1;

        // for limit the count of points
        public int RangePoint = 0;
        public float maxDifferenceX = 0;
        //        public float limitMaxPointPercent = 1;

        // for drawing
        private Dictionary<int, int> pointForSmoothDrawing;
        private Dictionary<int, Int2> pointForDrawing;
        public Dictionary<int, ConfigPlot> configPlots;
        private float time = 0;
        // grid
        //        public int CountAxisY = 10;
        public int CountAxisX = 4;
        private int minStepGridY = 30;
        private int maxStepGridY = 60;

        public int stepGridX = 0;
        public int stepGridY = 0;

        public bool IsAxisX = true;
        public bool IsAxisY = true;

        public bool IsFilled = false;
        public bool IsStacked = false;

        public bool IsMainAxisX = false;

        public Plot(FrameProcessor ui) : base(ui)
        {
            init();
        }

        public Plot(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init();
        }

        void init()
        {
            beamTexture = Game.Content.Load<DiscTexture>(@"ui\beam.tga");
            ValueList = new Dictionary<int, List<Vector2>>();
            pointForDrawing = new Dictionary<int, Int2>();
            pointForSmoothDrawing= new Dictionary<int, int>();
            stepGridX = this.Width / CountAxisX;
            stepGridY = (int) translateToPixel(new Vector2(0, stepsForY[currentStep])).Y;
            AxisTextFont = ConstantFrameUI.sfReg15;
            //            updateMinMaxPlot();
        }

        public void addPoint(Vector2 newPoint, int numberPlot = 0)
        {
            if (!ValueList.ContainsKey(numberPlot))
            {
                this.ValueList.Add(numberPlot, new List<Vector2>());
                this.pointForDrawing.Add(numberPlot, new Int2(0, 0));
                this.pointForSmoothDrawing.Add(numberPlot, 0);
            }
                
            this.ValueList[numberPlot].Add(newPoint);
            this.pointForDrawing[numberPlot] = getRangePoint(this.ValueList[numberPlot].Count);
            updateMinMaxPlotPoint(newPoint);
            IsDrawing = true;
        }

        public void RemovePlot(int numberPlot)
        {
            this.ValueList.Remove(numberPlot);
            this.pointForDrawing.Remove(numberPlot);
            this.pointForSmoothDrawing.Remove(numberPlot);
            updateMinMaxPlot();
        }

        public void addPlot(List<Vector2> newPlot)
        {
            if (newPlot == null)
                return;
            this.ValueList.Add(ValueList.Count, newPlot);
            this.pointForSmoothDrawing.Add(ValueList.Count, 0);
            this.pointForDrawing.Add(ValueList.Count, getRangePoint(newPlot.Count));
            updateMinMaxPlot();
            IsDrawing = true;
        }

        public void addPlots(List<List<Vector2>> PlotList)
        {
            if (PlotList == null)
                return;
            foreach (var plot in PlotList)
            {
                addPlot(plot);
            }
            updateMinMaxPlot();
            IsDrawing = true;
        }

        private Int2 getRangePoint(int countPoints)
        {
            var range = new Int2((int) (countPoints*limitMinPointPercent), (int) (countPoints*limitMaxPointPercent));
            if (RangePoint == 0 && maxDifferenceX == 0)
            {
                return range;
            }
            if (RangePoint > 0)
            {
                var startPosition = countPoints * limitMaxPointPercent - RangePoint;
                if (startPosition < 0)
                    startPosition = 0;
                range.X = (int) startPosition;
            }
//            if (maxDifferenceX < DistanceXMax)
//            {
//                minX = maxX - maxDifferenceX;
//                DistanceXMax = maxDifferenceX;
//            }

            return range;
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            stepGridX = this.Width / CountAxisX;
//            stepGridY = this.Height / CountAxisY;
            if (IsDrawing)
            {
                if (time > 0.01f)
                {
                    foreach (var key in ValueList.Keys)
                    {
                        while (pointForSmoothDrawing[key] < pointForDrawing[key].Y-1)
                            pointForSmoothDrawing[key]++;
                    }
                    time = 0;
                }
                time += gameTime.ElapsedSec;
            }
            else
            {
                foreach (var key in ValueList.Keys)
                {
                     pointForSmoothDrawing[key] = pointForDrawing[key].Y;
                }
            }

			#warning PASS CLIPRECTINDEX TO THESE FUNCTION TO ENABLE CLIPPING
			updateMinMaxPlot();
            updateStep();
            UpdateCoorAxis();
            
            drawAxis(sb);
            drawGraphic(sb);
            drawMainAxis(sb);
        }

        private void updateStep()
        {
            int countAxis = 0;
            var step = 0;
            var minusCircle = false;
            var plusCircle = false;
            while (stepsForY.Count >= currentStep+2)
            {
                if (DistanceYMax == 0)
                    break;
                countAxis = (int)Math.Ceiling(DistanceYMax / stepsForY[currentStep]);
                step = this.Height / countAxis;
                if (step >= minStepGridY &&
                    step <= maxStepGridY)
                {
                    break;
                }
                else if (step > maxStepGridY)
                {
                    if (plusCircle)
                        break;
                    currentStep--;
                    minusCircle = true;
                    if (currentStep == -1)
                    {
                        stepsForY.Insert(0, stepsForY[0]/2);
                        currentStep = 0;
                    }
                }
                else if (step < minStepGridY)
                {
                    if (minusCircle)
                        break;
                    currentStep++;
                    plusCircle = true;
                }
            }
            if (stepsForY[currentStep] != 0)
            {
                stepGridY = step > 0 ? step : minStepGridY;
                minY = minY - minY % stepsForY[currentStep];
                maxY = minY + stepsForY[currentStep] * countAxis;
                DistanceYMax = maxY - minY;
                UpdateCoorAxis();
            }
        }

        private void drawGraphic(SpriteLayer sb)
        {
            //updateMinMaxPlot();
//            int numberPlot = 0;
            foreach (var list in ValueList)
            {
                ConfigPlot confPlot = configPlots[list.Key];
                var barChartColor = confPlot.color;
                if (!confPlot.visible)
                    continue;
                for (int i = pointForDrawing[list.Key].X; i < pointForSmoothDrawing[list.Key]; i++)
                {
                    if (minX > list.Value[i].X)
                        continue;
                    var firstPoint = translateToPixel(list.Value[i]);
                    var secondPoint = translateToPixel(list.Value[i + 1]);

                    if (IsFilled)
                    {
                        var countPoint = secondPoint.X - firstPoint.X;
                        for (var x = 0; x < countPoint; x++)
                        {
                            var xPointTop = new Vector2(firstPoint.X + x,
                                (secondPoint.Y - firstPoint.Y)/(secondPoint.X - firstPoint.X)*x + firstPoint.Y);
                            sb.DrawBeam(beamTexture, xPointTop, new Vector2(xPointTop.X, pixelAxisX),
                                new Color(barChartColor.ToVector3(), 0.5f), new Color(barChartColor.ToVector3(), 0.5f), 1);
                        }
//                        sb.DrawBeam(beamTexture, firstPoint, new Vector2(firstPoint.X, pixelAxisX),
//                           new Color(barChartColor.ToVector3(), 0.5f), new Color(barChartColor.ToVector3(), 0.5f), 1);

//                        var yGraphic = 0f;
//                        if (list.Value[i].Y > 0)
//                        {
//                            yGraphic = pixelAxisX + (firstPoint.Y - pixelAxisX) / 2;
//                        }
//                        else
//                        {
//                            yGraphic = pixelAxisX - (pixelAxisX - firstPoint.Y) / 2;
//                        }
//                        sb.DrawSprite(this.Game.RenderSystem.WhiteTexture, firstPoint.X + (secondPoint.X - firstPoint.X) / 2, yGraphic, secondPoint.X - firstPoint.X, Math.Abs(pixelAxisX - firstPoint.Y), 0, new Color(barChartColor.ToVector3(), 0.2f));
                    }
                    sb.DrawBeam(beamTexture, firstPoint, secondPoint, barChartColor, barChartColor, thicknessLine);
                    
                }
            }
        }


        private void drawAxis(SpriteLayer sb)
        {

                // grid x > 0
            var i = 0;
            for (int y = pixelAxisX; y >= this.GlobalRectangle.Y; y -= stepGridY)
            {
                drawAxisY(sb, y, i);
                i++;
            }

            // grid x < 0
            i = 0;
            for (int y = pixelAxisX; y <= this.GlobalRectangle.Y + this.Height; y += stepGridY)
            {
                drawAxisY(sb, y, i);
                i++;
            }

            // grid Y > 0
            for (int x = pixelAxisY; x <= this.GlobalRectangle.X + this.Width; x += stepGridX)
            {
                drawAxisX(sb, x);
            }


            // grid Y < 0
            for (int x = pixelAxisY; x >= this.GlobalRectangle.X; x -= stepGridX)
            {
                drawAxisX(sb, x);
            }
        }
        private void drawMainAxis(SpriteLayer sb)
        {
            // Axis x
            sb.DrawBeam(beamTexture, new Vector2(this.GlobalRectangle.X, pixelAxisX), new Vector2(this.GlobalRectangle.X + this.Width, pixelAxisX), ColorConstant.MainAxisColor, ColorConstant.MainAxisColor, 8);
            //this.Font.DrawString(sb, "", this.GlobalRectangle.X - this.Font.MeasureString("x").Width, pixelAxisX, ColorConstant.TextColor);
            // Axis y
            if (IsMainAxisX)
            {
                sb.DrawBeam(beamTexture, new Vector2(pixelAxisY, this.GlobalRectangle.Y), new Vector2(pixelAxisY, this.GlobalRectangle.Y + this.Height), ColorConstant.MainAxisColor, ColorConstant.MainAxisColor, 8);
            }
            //this.Font.DrawString(sb, "", pixelAxisY, this.GlobalRectangle.Y - this.Font.MeasureString("y").Height / 2, ColorConstant.TextColor);
        }


        private void drawAxisX(SpriteLayer sb, int x)
        {
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            var stringValue = DistanceXMax / (this.Width) * (x - pixelAxisY);
            stringValue += minX > 0 ? minX : maxX < 0 ? maxX : 0;
            stringValue = Math.Round(stringValue, 0);
            if (IsAxisY)
            {
                sb.Draw(whiteTex, new Rectangle(x, this.GlobalRectangle.Y, 1, this.Height),
                    new Color(Color.White.ToVector3(), 0.2f));
            }
            var str = stringValue.ToString();
            if (IsTime)
            {
                var time =
                    TimeSpan.FromMilliseconds(stringValue +
                                              MinDate.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds);
                str = $@"{time:hh\:mm}"; 
            }
                
            var sizeText = AxisTextFont.MeasureString(str);
            var xText = x - sizeText.Width / 2;
            var yText = pixelAxisX + ConstantFrameUI.gridUnits*3 + AxisTextFont.CapHeight;
            AxisTextFont.DrawString(sb, str, xText, yText, ColorConstant.TextColor);
        }

        private void drawAxisY(SpriteLayer sb, int y, int indexArray)
        {
            var whiteTex = this.Game.RenderSystem.WhiteTexture;
            var stringValue = 0D;
            if (minY*maxY > 0)
            {
                if (maxY < 0)
                    stringValue = maxY - indexArray*stepsForY[currentStep];
                else
                    stringValue = minY + indexArray*stepsForY[currentStep];
            }
            else
                stringValue = indexArray * stepsForY[currentStep];



            //stringValue += minY > 0 ? minY : maxY < 0 ? maxY : 0;
            //stringValue = Math.Round(stringValue, 1);
            if (IsAxisX)
            {
                sb.Draw(whiteTex, new Rectangle(this.GlobalRectangle.X, y, this.Width, 1),
                    new Color(Color.White.ToVector3(), 0.2f));
            }
            var sizeText = AxisTextFont.MeasureString(stringValue.ToString());
            var xText = this.GlobalRectangle.X - sizeText.Width - ConstantFrameUI.gridUnits*2;
            var yText = y + AxisTextFont.CapHeight/2;
            AxisTextFont.DrawString(sb, stringValue.ToString(), xText, yText, ColorConstant.TextColor);
        }


        private void updateMinMaxPlot()
        {
            bool firstIter = true;
            foreach (var plotValue in ValueList)
            {
                if (ValueList[plotValue.Key].Count == 0)
                    continue;
                if (!configPlots[plotValue.Key].visible)
                    continue;
                if (firstIter)
                {
                    minX = ValueList[plotValue.Key][pointForSmoothDrawing[plotValue.Key]].X;
                    maxX = ValueList[plotValue.Key][pointForSmoothDrawing[plotValue.Key]].X;

                    minY = ValueList[plotValue.Key][pointForSmoothDrawing[plotValue.Key]].Y;
                    maxY = ValueList[plotValue.Key][pointForSmoothDrawing[plotValue.Key]].Y;

                    for (int i = pointForDrawing[plotValue.Key].X; i < pointForSmoothDrawing[plotValue.Key]; i++)
                    {
                        updateMinMaxPlotPoint(ValueList[plotValue.Key][i]);
                    }
                    firstIter = false;
                }
                else
                {
                    for (int i = pointForDrawing[plotValue.Key].X; i < pointForSmoothDrawing[plotValue.Key]; i++)
                    {
                        updateMinMaxPlotPoint(ValueList[plotValue.Key][i]);
                    }
                }
            }
            DistanceXMax = maxX - minX;
            if (DistanceXMax > maxDifferenceX && maxDifferenceX > 0)
            {
                minX = maxX - maxDifferenceX;
                DistanceXMax = maxDifferenceX;
            }  
            DistanceYMax = maxY - minY;
            UpdateCoorAxis();
        }

        private void updateMinMaxPlotPoint(Vector2 element)
        {
            var resultMaxX = element.X + AddValueForMax;
            if (minX > element.X)
                minX = element.X;
            if (maxX < resultMaxX)
                maxX = resultMaxX;

            if (minY > element.Y)
                minY = element.Y;
            if (maxY < element.Y)
                maxY = element.Y;
            DistanceXMax = maxX - minX;
            DistanceYMax = maxY - minY;
            UpdateCoorAxis();
        }

        private void UpdateCoorAxis()
        {
            if (DistanceYMax <= 0)
            {
                pixelAxisX = (this.GlobalRectangle.Y + this.GlobalRectangle.Height);
            }
            else
            {
                if (maxY >= 0 && minY <= 0)
                {
                    pixelAxisX = this.GlobalRectangle.Y + Height - (int)((this.Height) * (DistanceYMax - maxY) / DistanceYMax);
                }
                if (maxY > 0 && minY > 0)
                {
                    pixelAxisX = this.GlobalRectangle.Y + this.Height;
                }
                else if (maxY < 0 && minY < 0)
                {
                    pixelAxisX = this.GlobalRectangle.Y;
                }
            }  


            // Axis y
            if (DistanceXMax <= 0)
            {
                pixelAxisY = (this.GlobalRectangle.X);
            }
            else
            {
                if (maxX > 0 && minX <= 0)
                {
                    pixelAxisY = this.GlobalRectangle.X + (int)((this.Width) * (DistanceXMax - maxX) / DistanceXMax);
                }
                if (maxX > 0 && minX > 0)
                {
                    pixelAxisY = this.GlobalRectangle.X;
                }
                else if (maxX < 0 && minX < 0)
                {
                    pixelAxisY = this.GlobalRectangle.X + this.GlobalRectangle.Width;
                }
            }
            pixelAxisX += 1;

        }

        Vector2 translateToPixel(Vector2 coor)
        {
            int pixelX = pixelAxisY;
            if (Math.Abs(maxX - minX) > 1e-6)
            {
                pixelX = this.GlobalRectangle.X + (int)((this.Width) * ((coor.X - minX) / (maxX - minX)));
            }
            int pixelY = pixelAxisX;
            if (Math.Abs(maxY - minY) > 1e-6)
            {
                pixelY = this.GlobalRectangle.Y + Height - (int)((this.Height) * ((coor.Y - minY) / (maxY - minY)));
            } 
            return new Vector2(pixelX, pixelY);
        }

        public void updateLimit(float limitMin, float limitMax)
        {
            limitMinPointPercent = limitMin;
            limitMaxPointPercent = limitMax;
            foreach (var plotValue in ValueList)
            {
                pointForDrawing[plotValue.Key] = new Int2((int)((ValueList[plotValue.Key].Count - 1) * limitMinPointPercent), (int)((ValueList[plotValue.Key].Count - 1) * limitMaxPointPercent));
                pointForSmoothDrawing[plotValue.Key] = pointForDrawing[plotValue.Key].X;
            }
            IsDrawing = false;
        }

        public int getCountPlot()
        {
            return ValueList.Count;
        }
    }
}
