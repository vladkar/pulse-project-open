using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.Panel;
using City.Snapshot.Snapshot;
using City.UIFrames;
using City.UIFrames.FrameElement;
using City.UIFrames.FrameElement.ModelElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Pulse.Plugin.SimpleInfection.Infection;

namespace City.ControlsClient.DomainClient.Train
{
    public class TrainGraphics : AbstractPulseView<TrainControl>
    {

        private GraphicBase graphicPlot;

        public TrainGraphics(TrainControl trainControl)
        {
            Control = trainControl;
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
            var newPoint = Control.GetInfectionPlotData();
            if (newPoint == null)
                return null;
            foreach (var data in newPoint)
            {
                var numberGraphic = data.Key;
                var points = data.Value;
                foreach (var p in points)
                {
                    graphicPlot.addPointToPlot(p, numberGraphic);
                }
            }
            return null;
        }

        protected override void UnloadView()
        {
        }

        private void addGraphics(FrameProcessor ui, Frame parentFrame)
        {

            var nameLabel = new Frame(ui, ConstantFrameUI.offsetGraphic, ConstantFrameUI.offsetGraphic - ConstantFrameUI.sfLight50.CapHeight/2,
                parentFrame.Width - ConstantFrameUI.offsetGraphic * 2, 50,  "Эпидемия", Color.Zero )
            {
                Font = ConstantFrameUI.sfLight50,
                OverallColor = ColorConstant.Title,
            }; 
            parentFrame.Add(nameLabel);
            var helpTexture = ui.Game.Content.Load<DiscTexture>(@"ui\help-btn");
            var labelInfo = FrameHelper.createButtonI(ui, parentFrame.Width - ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetGraphic, ConstantFrameUI.offsetGraphic,
                helpTexture.Width, helpTexture.Height, "",
                helpTexture,
                ConstantFrameUI.offsetGraphic,
                ConstantFrameUI.offsetGraphic,
                ColorConstant.TextColor,
                () => {
                    var helpFrame = new Frame(ui, 0, 0, ui.Game.RenderSystem.DisplayBounds.Width,
                        ui.Game.RenderSystem.DisplayBounds.Height, "", new Color(0, 0, 0, 170))
                    {
                        Anchor = FrameAnchor.All,
                        Image = ui.Game.Content.Load<DiscTexture>(@"ui\Train_info"),
                    };
                    helpFrame.Click += (e, a) => {
                        ui.RootFrame.Remove(helpFrame);
                    };
                    ui.RootFrame.Add(helpFrame);
                }
                );
            labelInfo.Anchor = FrameAnchor.Right | FrameAnchor.Top;
            parentFrame.Add(labelInfo);
            var text1 =
                "Интерактивное приложение демонстрирует исследования Университета ИТМО в области моделирования распространения заболеваний на различных масштабах. В приложении демонстрируются модели распространения различных инфекционных заболеваний (от легких до смертоносных) на примере поездов дальнего следования: “Адлер – Москва”, “Адлер - Пермь” и “Адлер - Минск.";
                

            var richTextBox1 = new RichTextBlock(ui, ConstantFrameUI.offsetGraphic + ConstantFrameUI.offsetRichText, 
                ConstantFrameUI.offsetYRichText,
                parentFrame.Width - ConstantFrameUI.ofsetXRightRichText - ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText, 0, text1, ColorConstant.Zero, ConstantFrameUI.sfLight18, ConstantFrameUI.sfLight18.CapHeight)
            {
                ForeColor = new Color(ColorConstant.TextColor.ToVector3(), 0.8f),
            };
            parentFrame.Add(richTextBox1);

            var text2 = "Настройте параметры: выберите тип заболевания, маршрут следования поезда и запустите моделирование."
                + " Далее наблюдайте за процессом распространения эпидемии на микро-уровне(среди пассажиров поезда) и его влияние на макро - уровне(распространение заболевания в городах пути следования поезда).";
            var richTextBox2 = new RichTextBlock(ui, ConstantFrameUI.offsetGraphic + ConstantFrameUI.offsetRichText,
            richTextBox1.Y + richTextBox1.Height + ConstantFrameUI.offsetBetweenText,
            parentFrame.Width - ConstantFrameUI.ofsetXRightRichText - ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText, 0, text2, ColorConstant.Zero, ConstantFrameUI.sfBold15, ConstantFrameUI.sfBold15.CapHeight)
            {
                OverallColor = ColorConstant.RichTextWhite,
            };
            parentFrame.Add(richTextBox2);

//            var color = ColorConstant.defaultConfigPlot;
            var color = new Dictionary<int, ConfigPlot>
            {
                { (int)BaseInfectionStage.InfectionStates.IM, new ConfigPlot(Color.Blue, "Иммунитет") },
                { (int)BaseInfectionStage.InfectionStates.S, new ConfigPlot(Color.Green, "Здоровый") },
                { (int)BaseInfectionStage.InfectionStates.E, new ConfigPlot(Color.Yellow, "Зараженный") },
                { (int)BaseInfectionStage.InfectionStates.I2, new ConfigPlot(Color.Red, "Больной") }
            };

            var yWorkSpace = parentFrame.Height - ConstantFrameUI.offsetGraphic/2 - ConstantFrameUI.WorkSpaceGraphicHeight;
            graphicPlot = FrameHelper.createPlot(ui, ConstantFrameUI.offsetGraphic, yWorkSpace,
                parentFrame.Width - ConstantFrameUI.offsetGraphic*2,
                ConstantFrameUI.WorkSpaceGraphicHeight, null, "Статистика по пассажирам в поезде", "чел", "ч", color);
//            graphicPlot.setRangePointPlot(100);
            graphicPlot.setAddValueForMax(10000);
            graphicPlot.setMaxDistanceX(3*3600000f);
            graphicPlot.setThinknessPlot(7);
            graphicPlot.setConfig(true, true, false, false, true);

            ui.Game.RenderSystem.DisplayBoundsChanged += (sender, args) => {
                richTextBox1.Width = parentFrame.Width - ConstantFrameUI.ofsetXRightRichText -
                                     ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText;
                richTextBox2.Width = parentFrame.Width - ConstantFrameUI.ofsetXRightRichText -
                     ConstantFrameUI.offsetGraphic - ConstantFrameUI.offsetRichText;
                richTextBox1.init(ui, richTextBox1.Font, richTextBox1.OffsetLine);
                richTextBox2.init(ui, richTextBox2.Font, richTextBox2.OffsetLine);
                richTextBox1.Y = ConstantFrameUI.offsetYRichText;
                richTextBox2.Y = richTextBox1.Y + richTextBox1.Height + ConstantFrameUI.offsetBetweenText;

                graphicPlot.Y = parentFrame.Height - ConstantFrameUI.offsetGraphic / 2 - ConstantFrameUI.WorkSpaceGraphicHeight;
                graphicPlot.Width = parentFrame.Width - ConstantFrameUI.offsetGraphic*2;
                graphicPlot.resizeChild();
            };
            parentFrame.Add(graphicPlot);

        }
    }
}
