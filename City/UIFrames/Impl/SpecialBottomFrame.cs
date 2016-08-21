using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using City.ControlsClient.DomainClient.Train;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.YandexMaps;

namespace City.UIFrames.Impl
{
    class SpecialBottomFrame : Frame {
        private FrameProcessor ui;

//        readonly int widthButton = 27 * ConstantFrameUI.gridUnits;

        private Frame menuButton;
        private Frame expandPanel;
        private Frame helpButton;

		private Texture HelpTexture;

	    private Action onServerShutDown;

	    private Action startScenatio;

        private int workleftX = 0;
        private int workrightX = 0;
        private Frame logoFrame;

        public SpecialBottomFrame(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public SpecialBottomFrame(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        public void init(FrameProcessor ui)
        {
            this.ui = ui;
            this.X = ConstantFrameUI.statusFrameUIX;
            this.Y = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.specBottomFrameUIHeight;
            this.Width = this.Game.RenderSystem.DisplayBounds.Width;
            this.Height = ConstantFrameUI.specBottomFrameUIHeight;
            this.BackColor = ColorConstant.trainBottomFrameBackColor;
            this.Border = 1;
            this.BorderColor = ColorConstant.trainBorderColor;
            this.Anchor = FrameAnchor.Bottom | FrameAnchor.Left | FrameAnchor.Right;

            var textMainButton = "Сценарии";
            var fontText = ConstantFrameUI.segoeLight34;
            var sizeText = fontText.MeasureString(textMainButton);
            menuButton = new ImageWithText(ui, 0, 0, ConstantFrameUI.specBottomFrameUIHeight + sizeText.Width+20, ConstantFrameUI.specBottomFrameUIHeight, textMainButton, Color.Zero)
            {
                PaddingLeft = (ConstantFrameUI.specBottomFrameUIHeight - ConstantFrameUI.sizeIcon) / 2,
                sizePicture = ConstantFrameUI.sizeIcon,
                offset = (ConstantFrameUI.specBottomFrameUIHeight - ConstantFrameUI.sizeIcon)/2,
                imageColor = Color.White,
                Font = fontText,
                Image = ui.Game.Content.Load<DiscTexture>(@"ui\menu"),
            };
            menuButton.StatusChanged += (e, a) => {
                if (a.Status == FrameStatus.Pushed)
                    menuButton.BackColor = ColorConstant.ActiveElement;
                if(a.Status == FrameStatus.None)
                    menuButton.BackColor = Color.Zero;
            };
            menuButton.Click += (e, a) => {
                if (expandPanel == null)
                {
                    expandPanel = createExpandPanel();
                    ui.RootFrame.Add(expandPanel);
                }
                else
                {
                    ui.RootFrame.Remove(expandPanel);
                    expandPanel.Clear(expandPanel);
                    expandPanel = null;
                }
            };
            //menuButton = FrameHelper.createMainButton(ui, 0, 0, ConstantFrameUI.specBottomFrameUIHeight, ConstantFrameUI.specBottomFrameUIHeight,
            //    () => {
            //        if (expandPanel == null)
            //        {
            //            expandPanel = createExpandPanel();
            //            ui.RootFrame.Add(expandPanel);
            //        }
            //        else
            //        {
            //            ui.RootFrame.Remove(expandPanel);
            //            expandPanel.Clear(expandPanel);
            //            expandPanel = null;
            //        }  
            //    });
            helpButton = FrameHelper.createHelpButton(ui, menuButton.Width, 0, ConstantFrameUI.specBottomFrameUIHeight, ConstantFrameUI.specBottomFrameUIHeight, HelpAction);
//            menuButton.Click += (sender, args) => {
                // TODO: delete
                //                ui.RootFrame.Add(createMapControls(ui, ui.RootFrame.Width - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize, ui.RootFrame.Height - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize * 4));
                //                var WagonButton = createеTrainControls(ui, 10);
                //                WagonButton.X = this.Game.RenderSystem.DisplayBounds.Width/2 - WagonButton.Width/2;
                //                WagonButton.Y = this.Game.RenderSystem.DisplayBounds.Height - WagonButton.Height - ConstantFrameUI.offsetTrainButtonList;
                //                ui.RootFrame.Add(WagonButton);
//                var frame = new Frame(ui, 0, 0, 500, 500, "", ColorConstant.BackColor);
//                frame.Add(FrameHelper.createTrainMapLabel(ui, 100, 100, "Москва", "1", "1000"));
//
//                frame.Add(FrameHelper.createTrainMapLabel(ui, 200, 200, "Владивосток", "11111111111", "1"));
//                //                frame.Add(createMapControls(ui, frame.Width - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize, frame.Height - ConstantFrameUI.offsetMapButtonList - ConstantFrameUI.mapButtonSize * 4));
//                //                addGraphics(ui, frame);
//                ui.RootFrame.Add(frame);
//            };
            this.Add(menuButton);
            //var fontText = ConstantFrameUI.segoeLight34;
            //var sizeText = fontText.MeasureString("Сценарии");
            //this.Add(new Frame(ui, menuButton.Width, 0, sizeText.Width, ConstantFrameUI.specBottomFrameUIHeight, "Сценарии", Color.Zero)
            //{
            //    Font = fontText,
            //    Anchor = FrameAnchor.Left,
            //    TextAlignment = Alignment.MiddleCenter
            //});
            //this.Add(helpButton);

            var logo = ui.Game.Content.Load<DiscTexture>(@"ui\itmo-logo-train");
            var logoOffset = 32;
            var yLogo = (this.Height - logo.Height)/2;
            logoFrame = new Frame(ui, this.Width - logo.Width - logoOffset, yLogo, logo.Width, logo.Height,
                "", Color.Zero)
            {
                Image = logo,
                Anchor = FrameAnchor.Right,
            };
            this.Add(logoFrame);

            workleftX = menuButton.Width + sizeText.Width;
            workrightX = this.Width-logo.Width - logoOffset;

            addInfoElements();
        }

        private int Panding = 20;

        private void addInfoElements()
        {
            
            destinationButton = createField(0, DefaultTextDest);
            this.Add(destinationButton);
            infectionButton = createField(1, DefaultTextInfe);
            this.Add(infectionButton);
            var timePattern = "hh:mm";
            var timeButton = createField(2, timePattern);
            timeButton.TextAlignment = Alignment.MiddleLeft;
            timeButton.Tick += (sender, args) => {
                timeButton.Text = "Текущее время: " + TrainControl.Time.ToLongTimeString();	
            };
            this.Add(timeButton);


        }

        private Frame createField(int position, string text)
        {
            var widthElement = (workrightX - workleftX) / 3;
            var font = ConstantFrameUI.segoeLight34;
            var frame = new Frame(ui, workleftX + position*widthElement, 0, widthElement, ConstantFrameUI.specBottomFrameUIHeight, text, ColorConstant.Zero)
            {
                Padding = 5,
                TextAlignment = Alignment.MiddleCenter,
                OverallColor = ColorConstant.TextStatus,
                Font = font,
            };
            ui.Game.RenderSystem.DisplayBoundsChanged += (sender, args) => {
                workrightX = (sender as GraphicsDevice).DisplayBounds.Width - logoFrame.Width;
                widthElement = (workrightX - workleftX) / 3;
                frame.X = workleftX + position * widthElement;
                frame.Width = widthElement;
            };
            return frame;
        }

        private static readonly int expandPanelWidth = (int) (60*ConstantFrameUI.gridUnits);
        private static readonly int expandPanelHeight = (int)(90 * ConstantFrameUI.gridUnits);
        private static readonly int offsetExpandPanel = 8*ConstantFrameUI.gridUnits;
        private static readonly int offsetYExpandPanel = 4 * ConstantFrameUI.gridUnits;
        private static readonly int offsetCheckboxExpandPanel = 2 * ConstantFrameUI.gridUnits;
        private static readonly int expandElementHeight = (int)(6 * ConstantFrameUI.gridUnits);

        private static readonly int widthButtonExpandPanel = 30*ConstantFrameUI.gridUnits;
        private static readonly int heightButtonExpandPanel = 8 * ConstantFrameUI.gridUnits;

        private static readonly string[] pathList = new[] {"Адлер - Москва", "Адлер - Пермь", "Адлер - Минск"};
        private static readonly string[] infectionList = new[] { "Инфекция 1", "Инфекция 2", "Инфекция 3" };

        public int selectedPath = 0;
        public int selectedInfection = 0;

        public List<Checkbox> listPathCheckbox = new List<Checkbox>();
        public List<Checkbox> listInfectionCheckbox = new List<Checkbox>();
        private string DefaultTextDest = "Текущий маршрут: ";
        private Frame destinationButton;
        private string DefaultTextInfe = "Тип инфекции: ";
        private Frame infectionButton;


        private Frame createExpandPanel()
        {
			listPathCheckbox.Clear();
			listInfectionCheckbox.Clear();

			var y = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.specBottomFrameUIHeight - expandPanelHeight - 2;
            var ePanel = new Frame(ui, 0, y, expandPanelWidth, expandPanelHeight, "", ColorConstant.trainBackColor)
            {
                Anchor = FrameAnchor.Bottom | FrameAnchor.Left,
                Border = 1,
                BorderColor = ColorConstant.trainBorderColor
            };
            var frame = new Frame(ui, offsetExpandPanel, offsetExpandPanel, 0, 0, "Настройки", Color.Zero)
            {
                Font = ConstantFrameUI.segoeSemiLight24
            };
            var sizeText = frame.Font.MeasureString(frame.Text);
            frame.Width = sizeText.Width;
            frame.Height = sizeText.Height;
            ePanel.Add(frame);
            var yLastElement = ePanel.Children.Last().Y + ePanel.Children.Last().Height + offsetYExpandPanel;
            ePanel.Add(FrameHelper.createLable(ui, offsetExpandPanel, yLastElement, "Выбор маршрута", ConstantFrameUI.segoeReg20));
            yLastElement = ePanel.Children.Last().Y + ePanel.Children.Last().Height + offsetCheckboxExpandPanel;
            var pathFrameList = new ListBox(ui, offsetExpandPanel, yLastElement, 0, 0, "", Color.Zero);
            for (int i = 0; i < pathList.Length; i++)
            {
                var path = pathList[i];
                var checkbox = FrameHelper.createCheckbox(ui, offsetExpandPanel, 0, expandPanelWidth - offsetExpandPanel,
                    expandElementHeight, path, () => { },
                    ui.Game.Content.Load<DiscTexture>(@"ui\radioBut-active"),
                    ui.Game.Content.Load<DiscTexture>(@"ui\radioBut"));
                checkbox.ActiveColor = ColorConstant.TextColor;
                checkbox.IsChecked = i == selectedPath;
                checkbox.Click += (e, a) => {
                    ActionCheckBox(checkbox, listPathCheckbox);
                };
                listPathCheckbox.Add(checkbox);
                pathFrameList.addElement(checkbox);
            }
            
            ePanel.Add(pathFrameList);
            yLastElement = ePanel.Children.Last().Y + ePanel.Children.Last().Height + offsetYExpandPanel;
            ePanel.Add(FrameHelper.createLable(ui, offsetExpandPanel, yLastElement, "Выбор сценария", ConstantFrameUI.segoeReg20));
            yLastElement = ePanel.Children.Last().Y + ePanel.Children.Last().Height + offsetCheckboxExpandPanel;
            var infectionFrameList = new ListBox(ui, offsetExpandPanel, yLastElement, 0, 0, "", Color.Zero);

            for (int i = 0; i < infectionList.Length; i++)
            {
                var infection = infectionList[i];
                var checkbox = FrameHelper.createCheckbox(ui, offsetExpandPanel, 0, expandPanelWidth - offsetExpandPanel,
                expandElementHeight, infection, () => {},
                ui.Game.Content.Load<DiscTexture>(@"ui\radioBut-active"),
                ui.Game.Content.Load<DiscTexture>(@"ui\radioBut"));
                checkbox.ActiveColor = ColorConstant.TextColor;
                checkbox.IsChecked = i == selectedInfection;
                checkbox.Click += (sender, args) => {
                    ActionCheckBox(checkbox, listInfectionCheckbox);
                };
                listInfectionCheckbox.Add(checkbox);
                infectionFrameList.addElement(checkbox);
            }


	        startScenatio = () => {
		        var ind = listPathCheckbox.FindIndex(x => x.IsChecked);
                var ind2 = listInfectionCheckbox.FindIndex(x => x.IsChecked);
                selectedPath = ind;
	            selectedInfection = ind2;
	            destinationButton.Text = DefaultTextDest + listPathCheckbox[selectedPath].Text;
                infectionButton.Text = DefaultTextInfe + listInfectionCheckbox[selectedInfection].Text;

	            var infectionSuff = "";

	                                  switch (ind2)
	                                  {
	                                      case 0:
	                                          infectionSuff = "_inf_1";
	                                          break;
	                                      case 1:
	                                          infectionSuff = "_inf_2";
	                                          break;
	                                      case 2:
	                                          infectionSuff = "_inf_3";
	                                          break;
	                                      default:
	                                          infectionSuff = "_inf_1";
	                                          break;
	                                  }

	            switch (ind)
                {
                    case 0:
                        Game.Invoker.PushAndExecute("map trainmoscow" + infectionSuff);
                        break;
                    case 1:
                        Game.Invoker.PushAndExecute("map trainperm" + infectionSuff);
                        break;
                    case 2:
                        Game.Invoker.PushAndExecute("map trainminsk" + infectionSuff);
                        break;
                    default:
                        Game.Invoker.PushAndExecute("map trainmoscow" + infectionSuff);
                        break;
                }
                ui.RootFrame.Remove(expandPanel);
                expandPanel.Clear(expandPanel);
                expandPanel = null;
            };


			onServerShutDown = () => {
				startScenatio();

				Game.GameServer.OnServerShutdown -= onServerShutDown;
				Game.Touch.Tap += (Game.GameInterface as CustomGameInterface).TapHandler;
			};

			ePanel.Add(infectionFrameList);
            yLastElement = ePanel.Children.Last().Y + ePanel.Children.Last().Height + offsetYExpandPanel;
            var button = FrameHelper.createButton(ui, offsetExpandPanel, yLastElement, widthButtonExpandPanel, heightButtonExpandPanel, "Начать", ()=> {
	            if (!Game.GameServer.IsAlive) {
		            startScenatio();
		            return;
	            }

				Game.Touch.Tap -= (Game.GameInterface as CustomGameInterface).TapHandler;
				Game.GameServer.OnServerShutdown += onServerShutDown;

				Game.Invoker.PushAndExecute("killServer");
			});

            button.BackColor		= Color.Gray;
            button.DefaultBackColor = Color.Gray;
            ePanel.Add(button);
            return ePanel;
        }

        private void HelpAction()
        {
            throw new NotImplementedException();
        }

        

        public void ActionCheckBox(Checkbox checkbox, List<Checkbox> listCheckBox)
        {
            var checkedElement = listCheckBox.FindAll(e => e.IsChecked).ToList();
            if (checkedElement.Any()) {
                checkedElement.ForEach(e=>e.IsChecked = false);
            }
            checkbox.IsChecked = true;
        }

        public void DefaultInitScenario()
        {
            destinationButton.Text = DefaultTextDest + pathList[0];
            infectionButton.Text = DefaultTextInfe + infectionList[0];
            Game.Invoker.PushAndExecute("map trainmoscow_inf_1");
        }
    }
}
