using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using City.UIFrames;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Framework;

namespace City.UIFrames.Impl
{
    class MenuPanel : Frame
    {
        private FrameProcessor ui;
        private ListBox listElement;
        private Frame additinalPanel;
        private Frame selectedScenario;
        private string commandScenario;

        private Frame expandPanel;

        private Texture MenuTexture;
        private Texture ScenarioTexture;
        private Texture HelpTexture;
        private Texture LayersTexture;
        private Texture SettingsTexture;

        private TypePanel typePanel;

        public MenuPanel(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public MenuPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        private void init(FrameProcessor ui)
        {
            this.ui = ui;
            this.X = ConstantFrameUI.menuFrameUIX;
            this.Y = ConstantFrameUI.menuFrameUIY + ConstantFrameUI.topFrameUIHeight;
            this.Width = ConstantFrameUI.menuFrameUIWidth;
            this.Height = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.topFrameUIHeight - ConstantFrameUI.statusFrameUIHeight;

            this.Name = ConstantFrameUI.menuPanelName;
            this.BackColor = ColorConstant.BackColorForm;
            this.Anchor= FrameAnchor.Bottom|FrameAnchor.Top|FrameAnchor.Left;

            LoadTexture();

            listElement = new ListBox(ui);

            var mainButton = FrameHelper.createMainButton(ui, 0, 0, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.menuFrameUIWidth,
                () => {
                    if (expandPanel == null)
                    {
                        expandPanel = createExpandPanel();
                        listElement.addElement(expandPanel);
                    }
                    else
                    {
                        listElement.RemoveElement(expandPanel);
                        expandPanel = null;
                    }
                });
            listElement.addElement(mainButton);

            this.Add(listElement);

            typePanel = TypePanel.None;
        }


        private void LoadTexture()
        {
            MenuTexture = ui.Game.Content.Load<DiscTexture>(@"ui\menu");
            ScenarioTexture = ui.Game.Content.Load<DiscTexture>(@"ui\scenarios");
            HelpTexture=  ui.Game.Content.Load<DiscTexture>(@"ui\help");
            LayersTexture = ui.Game.Content.Load<DiscTexture>(@"ui\layers");
            SettingsTexture = ui.Game.Content.Load<DiscTexture>(@"ui\settings");
        }

        private ListBox createExpandPanel()
        {
            var additinalListElement = new ListBox(ui);

            var layerButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.menuFrameUIWidth, "",
                LayersTexture,
                ConstantFrameUI.sizeIcon,
                ConstantFrameUI.sizeIcon,
                ColorConstant.TextColor,
                () =>
                {
                    UpdateAddinitnalPanel(this.Parent, createListLayer(this.Parent));
                });
            layerButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            additinalListElement.addElement(layerButton);

            var scenarioButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.menuFrameUIWidth, "",
                ScenarioTexture,
                ConstantFrameUI.sizeIcon,
                ConstantFrameUI.sizeIcon,
                ColorConstant.TextColor,
                () => { UpdateAddinitnalPanel(this.Parent, createListScenario(this.Parent)); });
            scenarioButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            additinalListElement.addElement(scenarioButton);

            var settingButton = FrameHelper.createButtonI(ui, 0, 0, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.menuFrameUIWidth, "",
                SettingsTexture,
                ConstantFrameUI.sizeIcon,
                ConstantFrameUI.sizeIcon,
                ColorConstant.TextColor,
                () => { UpdateAddinitnalPanel(this.Parent, createListSettings(this.Parent)); });
            settingButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            additinalListElement.addElement(settingButton);


            
            var helpButton = FrameHelper.createHelpButton(ui, 0, 0, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.menuFrameUIWidth, () => { });
            helpButton.PaddingLeft = (ConstantFrameUI.menuFrameUIWidth - ConstantFrameUI.sizeIcon) / 2;
            additinalListElement.addElement(helpButton);

            return additinalListElement;
        }

        private Frame createListScenario(Frame parent)
        {
            List<Frame> listScenario = new List<Frame>();

            listScenario.Add(FrameHelper.createIconWithText(ui, 0,0,
                ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight,
                "Sentiment scenario", "Sentiment", "map sent",
                this.Game.Content.Load<DiscTexture>(@"ui\palette"),
                setScenario, selectScenario));

            listScenario.Add(FrameHelper.createIconWithText(ui, 0, 0,
                ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight,
                "Zenit arena scenario", "Stadium", "map krest",
                this.Game.Content.Load<DiscTexture>(@"ui\palette"),
                setScenario, selectScenario));

            listScenario.Add(FrameHelper.createIconWithText(ui, 0, 0,
                ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight,
                "***", "...", "map ht",
                this.Game.Content.Load<DiscTexture>(@"ui\palette"),
                setScenario, selectScenario));

            listScenario.Add(FrameHelper.createIconWithText(ui, 0, 0,
                ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight,
                "Name scenario", "Description scenario", "",
                this.Game.Content.Load<DiscTexture>(@"ui\palette"),
                setScenario, selectScenario));

            listScenario.Add(FrameHelper.createIconWithText(ui, 0, 0,
                ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight,
                "Name scenario", "Description scenario", "",
                this.Game.Content.Load<DiscTexture>(@"ui\palette"),
                 setScenario, selectScenario));

            var ScenarioPanel = createAddPanel(parent, "Scenario", TypePanel.Scenario,
                () =>
                {
                    selectScenario();
                }, null, listScenario);
            return ScenarioPanel;
        }

        private void selectScenario()
        {
            UpdateAddinitnalPanel(this.Parent);
            if (commandScenario != null)
                this.Game.Invoker.Push(commandScenario);
            typePanel = TypePanel.None;
        }

        private void setScenario(Frame newScenario)
        {
            if (selectedScenario is IconWithText)
            {
                ((IconWithText)selectedScenario).IsActive = false;
            }
            if (newScenario is IconWithText)
            {
                commandScenario = ((IconWithText)newScenario).command;
            }
            selectedScenario = newScenario;
        }

        private Frame createListSettings(Frame parent)
        {
            var ScenarioPanel = createAddPanel(parent, "Settings", TypePanel.Settings,
                () =>
                {
                    UpdateAddinitnalPanel(parent);
                    typePanel = TypePanel.None;
                },
                () =>
                {
                    UpdateAddinitnalPanel(parent);
                    typePanel = TypePanel.None;
                });
            return ScenarioPanel;
        }

        private Frame createListLayer(Frame parent)
        {
            List<Frame> listCheckBox = new List<Frame>();
            listCheckBox.Add(FrameHelper.createCheckbox(ui, 0, 0, ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight, "Test Layer 1", () => { }));
            listCheckBox.Add(FrameHelper.createCheckbox(ui, 0, 0, ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight, "Test Layer 2", () => { }));
            listCheckBox.Add(FrameHelper.createCheckbox(ui, 0, 0, ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight, "Test Layer 3", () => { }));
            listCheckBox.Add(FrameHelper.createCheckbox(ui, 0, 0, ConstantFrameUI.ListLayerPanelWidth, ConstantFrameUI.menuListElementHeight, "Test Layer 4", () => { }));
            var LaeyrsPanel = createAddPanel(parent, "Layers", TypePanel.Layer,
                () =>
                {
                    UpdateAddinitnalPanel(parent);
                    Log.Message(listCheckBox.Where(f => f is Checkbox).Count(frame => ((Checkbox)frame).IsChecked).ToString());
                    typePanel = TypePanel.None;
                },
                () =>
                {
                    UpdateAddinitnalPanel(parent);
                    typePanel = TypePanel.None;
                },
                listCheckBox);
            return LaeyrsPanel;
        }

        private Frame createAddPanel(Frame parent, string namePanel, TypePanel newTypePanel, Action firstAction, Action secondAction = null, List<Frame> listControl = null)
        {
            if (!checkType(newTypePanel))
                return null;
            var ExtraPanel = new Frame(ui, ConstantFrameUI.menuFrameUIWidth, ConstantFrameUI.topFrameUIHeight, ConstantFrameUI.ListLayerPanelWidth,
                parent.Height - ConstantFrameUI.topFrameUIHeight - ConstantFrameUI.statusFrameUIHeight,
                "",
                ColorConstant.BackColorScenario)
            {
                Anchor = FrameAnchor.Top | FrameAnchor.Bottom | FrameAnchor.Left,
                Padding = 12,
            };

            var label = new Label(
                            ui,
                            ExtraPanel.PaddingLeft,
                            ConstantFrameUI.menuListElementHeight / 2 + this.Font.MeasureString(namePanel).Height / 2,
                            ConstantFrameUI.menuFrameUIWidth,
                            ConstantFrameUI.menuListElementHeight,
                            namePanel.ToUpper(),
                            ColorConstant.Zero)
            {
                Font = ConstantFrameUI.segoeSemiBold15,
            };
            ExtraPanel.Add(label);
            var ListElement = new ListBox(ui)
            {
                BackColor = ColorConstant.Zero,
                Y = 50
            };
            if(listControl!=null)
                ListElement.addListElement(listControl);

            var widthButton = ConstantFrameUI.menuPanelButtonWidth;
            var heightButton = ConstantFrameUI.menuPanelButtonHeight;

            Color ColorButton = new Color(255, 255, 255, 50);


            if (secondAction != null)
            {
                var acceptButton = new Button(ui, ExtraPanel.Width/2 - widthButton - ConstantFrameUI.gridUnits/2,
                    ExtraPanel.Height - ExtraPanel.PaddingBottom - heightButton, widthButton, heightButton,
                    "Accept", ColorButton)
                {
                    ForeColor = ColorConstant.ForeColor,
                    HoverColor = ColorConstant.HoverColor,
                    TextAlignment = Alignment.MiddleCenter,
                    Anchor = FrameAnchor.Bottom
                };
                acceptButton.Click += (e, a) =>
                {
                    firstAction();
                };
                ExtraPanel.Add(acceptButton);

                var cancelButton = new Button(ui, ExtraPanel.Width/2 + ConstantFrameUI.gridUnits/2,
                    ExtraPanel.Height - ExtraPanel.PaddingBottom - heightButton, widthButton, heightButton, "Cancel",
                    ColorButton)
                {
                    ForeColor = ColorConstant.ForeColor,
                    HoverColor = ColorConstant.HoverColor,
                    TextAlignment = Alignment.MiddleCenter,
                    Anchor = FrameAnchor.Bottom
                };
                cancelButton.Click += (e, a) =>
                {
                    secondAction();
                };
                ExtraPanel.Add(cancelButton);
            }
            else
            {
                var showButton = new Button(ui, ExtraPanel.Width / 2 + ConstantFrameUI.gridUnits / 2,
                    ExtraPanel.Height - ExtraPanel.PaddingBottom - heightButton, widthButton, heightButton, "Show",
                    ColorButton)
                {
                    ForeColor = ColorConstant.ForeColor,
                    HoverColor = ColorConstant.HoverColor,
                    TextAlignment = Alignment.MiddleCenter,
                    Anchor = FrameAnchor.Bottom
                };
                showButton.Click += (e, a) =>
                {
                    firstAction();
                };
                ExtraPanel.Add(showButton);
            }
            ExtraPanel.Add(ListElement);
            return ExtraPanel;
        }

        private void UpdateAddinitnalPanel(Frame parent, Frame newAdditinalPanel=null)
        {
            if (additinalPanel == null && newAdditinalPanel != null)
            {
                additinalPanel = newAdditinalPanel;
                this.Parent.Add(newAdditinalPanel);
            }
            else
            {
                this.Parent.Remove(additinalPanel);
                additinalPanel = newAdditinalPanel;
                if (newAdditinalPanel != null)
                    this.Parent.Add(newAdditinalPanel);
            }
        }

        enum TypePanel
        {
            Layer,
            Scenario,
            Settings,
            None,
        }

        private bool checkType(TypePanel newTypePanel)
        {
            if (typePanel == newTypePanel)
            {
                typePanel = TypePanel.None;
                return false;
            }
            else
            {
                typePanel = newTypePanel;
                return true;
            }
        }
    }
}
