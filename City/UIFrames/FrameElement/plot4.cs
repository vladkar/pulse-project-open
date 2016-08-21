using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;


namespace BiohazardMonitor.Controls
{

    public class Plot4 : Frame
    {
        private FrameProcessor ui;
        Texture2D beamTexture;

        public struct Row
        {
            public Color Color;
            public String SName;
            public List<float> Data;
            public List<List<int>> ByClass;
            public List<string> SocClassName;

        }

        List<string> dates = new List<string>();
        Dictionary<int, Row> rows = new Dictionary<int, Row>();




        public RectangleF Bounds;
        public Color GridColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        public bool showLegend = false;
        public bool showSolids = true;

        int ticksCount = 0;
        int maximumSamples = 200;
        float maxValue = 500.0f;
        int xOffset = 0;
        int maxOffset;

        float snapingValue = 500;

        int layoutGridUnit = 5;

        List<int> snapingValues = new List<int>();
        //int oddCheck = 0;
        Color firstColor;
        Color secondColor;


        /// <summary>
        /// 
        /// </summary>
        public Plot4(FrameProcessor ui, int w, int h) : base(ui)
        {
            this.ui = ui;
            Width = w;
            Height = h;
//            ClippingMode = ClippingMode.ClipByPadding;
            BackColor = Color.Zero;


            beamTexture = Game.Content.Load<Texture2D>(@"UI\beam");

            AddButton(w - 70, 0, 70, 25, "Легенда", FrameAnchor.Top | FrameAnchor.Right, () => { showLegend = !showLegend; });
            AddButton(w - 140, 0, 70, 25, "Заливка", FrameAnchor.Top | FrameAnchor.Right, () => { showSolids = !showSolids; });

            snapingValues.AddRange(new[] { 0, 10, 50, 100, 500, 1000, 5000, 10000, 50000, 100000, 500000, 600000, 700000, 800000, 900000, 1000000, 5000000 });


            Tick += OnTickMacroPlot1;

            maxOffset = maximumSamples / 6;
        }


        void OnTickMacroPlot1(object sender, EventArgs eventArgs)
        {
            ticksCount++;
            if (ticksCount < 13) return;
            ticksCount = 0;
            if (rows.Count == 0) return;

//            if (dashboard.simState.NewGraphData1 == null) return;
//            var data = dashboard.simState.NewGraphData1;

//            rows.Clear();

            string dateScale;
//            if (data.ValNames[0].Contains("МО"))
//            {
//                firstColor = new Color(0, 160, 255, 255);
//                secondColor = Color.Red;
//                dateScale = "ч";
//            }
//            else
//            {
                firstColor = Color.Red; secondColor = new Color(0, 160, 255, 255);
                dateScale = "д";
//            }


            //maximumSamples = 200;
            //maxValue = ;
//            maxOffset = maximumSamples/6;

//            if (data.Values[0].Length <= 1) return;

//            dates.Clear();
//            foreach (var d in data.Values[0])
//            {
//                AddMacroData(0, (float)d, firstColor, data.ValNames[0], null);
//            }

//            foreach (var d in data.Values[1])
//            {
//                AddMacroData(1, (float)d, secondColor, data.ValNames[1], null);
//            }


            for (int d = 0; d < rows.Values.First().Data.Count; d++)
            {
                dates.Add(d.ToString() + dateScale);
            }
            var maxValues = new List<float>();


            foreach (var row in rows)
            {
                maxValues.Add(row.Value.Data.Max());
            }

            //           maximumSamples = data.Values[0].Length; maximumSamples = data.Values[0].Length;
            maximumSamples = rows.Values.First().Data.Count;
            //if(data.Values[0].Length > maximumSamples) maximumSamples = data.Values[0].Length;
            maxOffset = 1;

            var newMax = maxValues.Max();
            maxValue = newMax;


            // Snap max value
            for (int i = 0; i < snapingValues.Count - 1; i++)
            {
                if (maxValue > snapingValues[i] && maxValue <= snapingValues[i + 1])
                    snapingValue = snapingValues[i + 1];
            }
            maxValue = (float)Math.Floor((maxValue / snapingValue) + 1) * snapingValue;

        }


        void AddMacroData(int ind, float data, Color color, string name, List<string> socclassnames)
        {
            if (!rows.ContainsKey(ind))
            {
                rows.Add(ind, new Row() { Color = color, SName = name, Data = new List<float>(), SocClassName = socclassnames, ByClass = new List<List<int>>() });
            }

            rows[ind].Data.Add(data);
            //rows[ind].ByClass.Add(disbyclass);
        }



        Color MulAlpha(Color c, float factor)
        {
            return new Color(c.R, c.G, c.B, (byte)(c.A * factor));
        }


        protected override void Draw(GameTime gameTime, SpriteLayer sb, Color colorMultiplier)
        {
//            base.Draw(gameTime, sb, colorMultiplier);

            var pr = GetPaddedRectangle();

            int yAxisHeight = 20;
            int yAxisLineWidth = 1;
            int textHeight = 4;
            int leftPad = 40;

            int legendHeight = layoutGridUnit * 5;

            int minYCueStep = layoutGridUnit * 8;
            int maxYCueStep = layoutGridUnit * 24;

            float x = pr.X + leftPad + legendHeight;
            float y = pr.Y + 30;

            int w = pr.Width - leftPad - legendHeight;
            int h = pr.Height - 30 - yAxisHeight - legendHeight - 15;



            // Draw grid
            int yCueCount = (int)(maxValue / snapingValue);

            yCueCount = yCueCount == 0 ? 1 : yCueCount;
            if (yCueCount == 1) yCueCount = yCueCount * 2;

            if ((h / yCueCount) < minYCueStep) yCueCount = yCueCount / 2;
            if ((h / yCueCount) > maxYCueStep) yCueCount = yCueCount * 2;

            int xCueCount = maximumSamples / maxOffset;
            float step = w / maximumSamples;

            if ((w / xCueCount) < 85) { xCueCount = xCueCount / 2; }
            //if ((w / xCueCount) > 170) { xCueCount = xCueCount * 2; }

            var newMaxOffset = maximumSamples / xCueCount;


            int xCueStep = newMaxOffset * (int)step;
            int yCueStep = h / yCueCount;

            string yAxisLabel = "Время от начала распространения";
            var yAxisLabelSize = Font.MeasureString(yAxisLabel);
            int yAxisLabelXPos = pr.X + leftPad + (w - yAxisLabelSize.Width - leftPad) / 2;
            int yAxisLabelYPos = pr.Y + pr.Height - (legendHeight - yAxisLabelSize.Height / 2) / 2;

            string xAxisLabel = "Число людей";
            var xAxisLabelSize = Font.MeasureString(xAxisLabel);
            int xAxisLabelXPos = pr.X;
            int xAxisLabelYPos = pr.Y + h / 2 + xAxisLabelSize.Width;



            // Draw y axis
            var axisColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            sb.DrawBeam(this.Game.RenderSystem.WhiteTexture, new Vector2(pr.X, y + h), new Vector2(pr.X + pr.Width, y + h), axisColor, axisColor, yAxisLineWidth);
            sb.DrawBeam(this.Game.RenderSystem.WhiteTexture, new Vector2(pr.X, y + h + yAxisHeight * 2 - 3), new Vector2(pr.X + pr.Width, y + h + yAxisHeight * 2 - 3), axisColor, axisColor, yAxisLineWidth);


            // Draw axis labels
            Font.DrawString(sb, yAxisLabel, yAxisLabelXPos, yAxisLabelYPos, Color.White);
            Font.DrawString(sb, xAxisLabel, xAxisLabelXPos, xAxisLabelYPos, Color.White, 0, true, true);


            // Draw vertical lines
            for (int i = 0; i < xCueCount + 1; i++)
            {
                var xPos = (int)(x + xCueStep * i + 1);
                sb.DrawBeam(this.Game.RenderSystem.WhiteTexture, new Vector2(xPos, y + 1), new Vector2(xPos, y + h), GridColor, GridColor, 1);
                int timeOffset = xOffset + newMaxOffset * i;

                if (timeOffset < dates.Count)
                {
                    Font.DrawString(sb, dates[timeOffset], xPos, y + h + yAxisHeight - yAxisLineWidth - textHeight + 1, Color.White);
                }
            }


            // Draw horizontal lines
            for (int i = 1; i <= yCueCount; i++)
            {
                var yPos = y + h - (int)Math.Ceiling((double)i * yCueStep);
                sb.DrawBeam(this.Game.RenderSystem.WhiteTexture, new Vector2(pr.X + legendHeight, yPos), new Vector2(pr.X + pr.Width, yPos), GridColor, GridColor, 1);

                Font.DrawString(sb, (maxValue / yCueCount * i).ToString(), pr.X + legendHeight, yPos - textHeight, Color.White);
            }
            Font.DrawString(sb, 0.ToString(), pr.X + legendHeight, y + h - textHeight, Color.White);


            // Draw solids


            if (showSolids == true)
            {

                foreach (var row in rows)
                {

                    for (int i = 0; i < row.Value.Data.Count - 1; i++)
                    {
                        float xLeft = x + i * step;
                        float xRight = x + (i + 1) * step;

                        float yLeft = y + (1.0f - row.Value.Data[i] / maxValue) * h;
                        float yRight = y + (1.0f - row.Value.Data[i + 1] / maxValue) * h;
                        float yGround = y + h + 2 - yAxisLineWidth;

                        
                        sb.DrawSprite(this.Game.RenderSystem.WhiteTexture, xLeft, yLeft, xRight- xLeft, yRight- yLeft, 0, MulAlpha(row.Value.Color, 0.15f));
//                        sb.DrawQuad(this.Game.RenderSystem.WhiteTexture,
//                            new SpriteVertex(xLeft, yLeft, 0f, MulAlpha(row.Value.Color, 0.15f), 0f, 0f),
//                            new SpriteVertex(xLeft, yGround, 0f, MulAlpha(row.Value.Color, 0.15f), 0f, 0f),
//                            new SpriteVertex(xRight, yGround, 0f, MulAlpha(row.Value.Color, 0.15f), 0f, 0f),
//                            new SpriteVertex(xRight, yRight, 0f, MulAlpha(row.Value.Color, 0.15f), 0f, 0f));

                    }
                }
            }


            //Draw lines
            foreach (var row in rows) 
            {

                for (int i = 0; i < row.Value.Data.Count - 1; i++)
                {
                    float xLeft = x + i * step;
                    float xRight = x + (i + 1) * step;

                    float yLeft = y + (1.0f - row.Value.Data[i] / maxValue) * h;
                    float yRight = y + (1.0f - row.Value.Data[i + 1] / maxValue) * h;

                    sb.DrawBeam(this.Game.RenderSystem.WhiteTexture, new Vector2(xLeft, yLeft), new Vector2(xRight, yRight), row.Value.Color, row.Value.Color,
                        6);
                }

            }


            // Draw legend
            if (showLegend == true)
            {

                int n = 0;
                int maxNameWidth = 0;
                int legendPadding = 10;
                int legendRowHeight = 20;
                Color legendBackColor = new Color(0, 0, 0, 64);

                // Find max legend name length

                foreach (var row in rows)
                {
                    if (Font.MeasureString(row.Value.SName).Width > maxNameWidth)
                    {
                        maxNameWidth = Font.MeasureString(row.Value.SName).Width;
                    };
                }

                int legendBGHeight = legendPadding * 2 + rows.Count * legendRowHeight - legendRowHeight / 2;
                int legendBGWidth = legendPadding * 2 + maxNameWidth;
                int xRight = pr.X + pr.Width;
                var yRight = y + legendPadding;

                // Draw background rectangle
                sb.DrawSprite(this.Game.RenderSystem.WhiteTexture, xRight - legendBGWidth, yRight, legendBGWidth, legendBGHeight, 0, legendBackColor);

//                sb.DrawQuad(this.Game.RenderSystem.WhiteTexture,
//                    new SpriteVertex(xRight, yRight, 0f, legendBackColor, 0f, 0f),
//                    new SpriteVertex(xRight, yRight + legendBGHeight, 0f, legendBackColor, 0f, 0f),
//                    new SpriteVertex(xRight - legendBGWidth, yRight + legendBGHeight, 0f, legendBackColor, 0f, 0f),
//                    new SpriteVertex(xRight - legendBGWidth, yRight, 0f, legendBackColor, 0f, 0f));

                // Draw legend text
                foreach (var row in rows)
                {

                    string disName = row.Value.SName;
                    var disNameSize = Font.MeasureString(disName);
                    Font.DrawString(sb, disName, xRight - legendPadding - disNameSize.Width, yRight + (legendPadding * 2) - (legendPadding / 3) + legendRowHeight * n++, row.Value.Color);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="color"></param>
        /// <param name="data"></param>
        public void AddData(int ind, Color color, string name, float data, List<string> socclassnames, List<int> disbyclass)
        {
            //if (ind == 1) return;
            if (!rows.ContainsKey(ind))
            {
                rows.Add(ind, new Row() { Color = color, SName = name, Data = new List<float>(), SocClassName = socclassnames, ByClass = new List<List<int>>() });
            }

            rows[ind].Data.Add(data);
            rows[ind].ByClass.Add(disbyclass);

            if (rows[ind].Data.Count > maximumSamples)
            {
                rows[ind].Data.RemoveAt(0);
            }
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="anchor"></param>
        /// <param name="action"></param>
        void AddButton(int x, int y, int w, int h, string text, FrameAnchor anchor, Action action)
        {
            var button = new Frame(ui, x, y, w, h, text, Color.White)
            {
                Anchor = anchor,
                TextAlignment = Alignment.MiddleCenter,
                PaddingLeft = 0,
            };

            if (action != null)
            {
                button.Click += (s, e) => action();
            }

            int c = 0;

            button.StatusChanged += (s, e) =>
            {
                if (e.Status == FrameStatus.None) { button.BackColor = new Color(c, c, c, (byte)0); }
                if (e.Status == FrameStatus.Hovered) { button.BackColor = new Color((byte)62, (byte)106, (byte)181, (byte)255); }
                if (e.Status == FrameStatus.Pushed) { button.BackColor = new Color((byte)99, (byte)132, (byte)181, (byte)255); }
            };

            Add(button);
        }
    }
}
