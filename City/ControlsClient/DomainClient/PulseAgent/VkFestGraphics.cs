using System.Collections.Generic;
using System.Linq;
using City.Panel;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.FrameElement;
using City.UIFrames.FrameElement.ModelElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Pulse.Common.Model.Agent;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Infection;

namespace City.ControlsClient.DomainClient.PulseAgent
{
    public class VkFestGraphics : AbstractPulseView<VkFestAgentControl>
    {

        private GraphicBase _plot1;
        private GraphicBase _plot2;
        private GraphicBase _plot3;
        private GraphicBase _plot4;

        public VkFestGraphics(VkFestAgentControl control)
        {
            Control = control;
        }

        public override Frame AddControlsToUI()
        {
            return null;
        }

        protected override void InitializeView()
        {
        }

        protected override void LoadView(ControlInfo controlInfo)
        {
            var ui = ((CustomGameInterface)Game.GameInterface).ui;
            var frame = (Panel as GisPanel).Frame;
            frame.Image = null;
            ui.Game.RenderSystem.DisplayBoundsChanged += (e, i) => {
                frame.Image = null;
            };
            addGraphics(ui, frame);
        }

        protected override ICommandSnapshot UpdateView(GameTime gameTime)
        {
            var agents = Control.GetAgents().OfType<ISfAgent>();
            var probes = agents.Where(a => a.Role == 140).ToArray();;

            for (int i = 0; i < probes.Length; i++)
            {
                var agent = probes[i];
                var ap = new PulseVector2(agent.X, agent.Y);
                var n = probes.Where(a => new PulseVector2(a.X, a.Y).DistanceTo(ap) < 5);

                var timeMilliSeconds = (float)Control.ModelTime.Subtract(Plot.MinDate).TotalMilliseconds;
//                var pr = n.Average(a => a.Pressure);
//                var d = n.Average(a => a.StepDist);

                var pr = agent.Pressure;
                var d = agent.StepDist;

                _plot3.addPointToPlot(new Vector2((float)Control.ModelTime.TimeOfDay.TotalMilliseconds, (float)pr), i);
                _plot4.addPointToPlot(new Vector2(timeMilliSeconds, (float)d), i);
            }


            return null;

            foreach (var agent in agents)
            {
               
            }

            var newPoint = new Dictionary<int, List<Vector2>>();
            if (newPoint == null)
                return null;
            foreach (var data in newPoint)
            {
                var numberGraphic = data.Key;
                var points = data.Value;
                foreach (var p in points)
                {
                    _plot3.addPointToPlot(p, numberGraphic);
                }
            }
            return null;
        }

        protected override void UnloadView()
        {
        }

        private void addGraphics(FrameProcessor ui, Frame parentFrame)
        {
            var color = new Dictionary<int, ConfigPlot>
            {
                { 0, new ConfigPlot(Color.Blue, "1") },
                { 1, new ConfigPlot(Color.Green, "2") },
                { 2, new ConfigPlot(Color.Yellow, "3") },
                { 3, new ConfigPlot(Color.Red, "4") },
                { 4, new ConfigPlot(Color.Purple, "5") },
                { 5, new ConfigPlot(Color.Beige, "6") },
                { 6, new ConfigPlot(Color.Cyan, "7") },
                { 7, new ConfigPlot(Color.WhiteSmoke, "8") },
                { 8, new ConfigPlot(Color.Brown, "9") },
                { 9, new ConfigPlot(Color.DarkGreen, "10") },
                { 10, new ConfigPlot(Color.Pink, "11") },
                { 11, new ConfigPlot(Color.Ivory, "12") },
                { 12, new ConfigPlot(Color.Silver, "13") },
                { 13, new ConfigPlot(Color.Ivory, "14") },
                { 14, new ConfigPlot(Color.DarkKhaki, "15") }
            };


//            var yWorkSpace1 = ConstantFrameUI.WorkSpaceGraphicHeight;


            _plot1 = createPlot(ui, 0, 0, parentFrame.Width, parentFrame.Height/3, "Plot1", color);
            parentFrame.Add(_plot1);

            _plot2 = createPlot(ui, 0, parentFrame.Height / 3, parentFrame.Width, parentFrame.Height / 3, "Plot2", color);
            parentFrame.Add(_plot2);

            _plot3 = createPlot(ui, 0, parentFrame.Height * 2 / 3, parentFrame.Width/2, parentFrame.Height / 3, "Plot3", color);
            parentFrame.Add(_plot3);

            _plot4 = createPlot(ui, parentFrame.Width / 2, parentFrame.Height * 2 / 3, parentFrame.Width/2, parentFrame.Height / 3, "Plot4", color);
            parentFrame.Add(_plot4);


            ui.Game.RenderSystem.DisplayBoundsChanged += (sender, args) => {
//                richTextBox1.Width = parentFrame.Width - ConstantFrameUI.ofsetXRightRichText -
//                                     ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText;
//                richTextBox2.Width = parentFrame.Width - ConstantFrameUI.ofsetXRightRichText -
//                     ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText;
//                richTextBox1.init(ui, richTextBox1.Font, richTextBox1.OffsetLine);
//                richTextBox2.init(ui, richTextBox2.Font, richTextBox2.OffsetLine);
//                richTextBox1.Y = ConstantFrameUI.offsetYRichText;
//                richTextBox2.Y = richTextBox1.Y + richTextBox1.Height + ConstantFrameUI.offsetBetweenText;
//
//                _plot2.Y = parentFrame.Height - ConstantFrameUI.offsetGraphic / 2 - ConstantFrameUI.WorkSpaceGraphicHeight;
//                _plot2.Width = parentFrame.Width - ConstantFrameUI.offsetGraphic * 2;
//                _plot2.resizeChild();
            };
            parentFrame.Add(_plot2);
        }

        private GraphicBase createPlot(FrameProcessor ui, int x, int y, int width, int height, string namePlot, Dictionary<int, ConfigPlot> color)
        {
           var plot = FrameHelper.createPlot(ui, x, y, width, height, null, namePlot, "чел", "ч", color);
            plot.setRangePointPlot(1000);
            plot.setAddValueForMax(10000);
            plot.setThinknessPlot(7);
            plot.setConfig(false, true, false, false, true);
            return plot;
        } 
    }
}
