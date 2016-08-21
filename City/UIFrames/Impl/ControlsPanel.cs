using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.Panel;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;

namespace City.UIFrames.Impl
{
    public class ControlsPanel : Frame
    {
        private FrameProcessor ui;
        private Texture texture;
        private PanelManager panelManager;
        private TreeNode treeControls;
        private List<int> currentFilter;
        public ControlsPanel(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public ControlsPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        private void init(FrameProcessor ui)
        {
            this.ui = ui;
            currentFilter = new List<int>();
            this.X = this.Game.RenderSystem.DisplayBounds.Width - ConstantFrameUI.controlsFrameUIWidth;
            this.Y = ConstantFrameUI.controlsFrameUIY+ConstantFrameUI.topFrameUIHeight;
            this.Width = ConstantFrameUI.controlsFrameUIWidth;
            this.Height = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.statusFrameUIHeight - ConstantFrameUI.topFrameUIHeight;

            this.Name = ConstantFrameUI.controlsPanelName;
            this.BackColor = ColorConstant.BackColorScenario;
            this.Anchor = FrameAnchor.Top | FrameAnchor.Right | FrameAnchor.Bottom;
            ///transition(true);
            LoadTexture();

            var namePanel = FrameHelper.createLabel(ui, ConstantFrameUI.controlsPaddingLeft, ConstantFrameUI.controlsPaddingTop - ConstantFrameUI.segoeLight46.CapHeight/2, "Controls");
            namePanel.Font = ConstantFrameUI.segoeLight46;
            this.Add(namePanel);

            createBottomButton();
            // main element for list of control elements
            treeControls = FrameHelper.createTreeNode(ui, 0,
                ConstantFrameUI.controlsPaddingTop, 0, 0, "", Color.Zero);
            treeControls.IsExpand = true;
            this.Add(treeControls);
        }

        private void LoadTexture()
        {
            texture = ui.Game.Content.Load<DiscTexture>(@"ui\menu");
        }

        private ButtonIcon allViewLayers;
        private ButtonIcon ViewLayer1;
        private ButtonIcon ViewLayer2;
        private ButtonIcon ViewLayer3;
        private ButtonIcon ViewLayer4;

        private void createBottomButton()
        {
            var buttomPanel = new Frame(ui, 0, this.Height - ConstantFrameUI.controlsButtonHeight, ConstantFrameUI.controlsFrameUIWidth, ConstantFrameUI.controlsButtonHeight,"", ColorConstant.ScenarioBackColorButton)
            {
                Anchor = FrameAnchor.Bottom,
            };
            allViewLayers = createButton(ui, 16, this.Height - ConstantFrameUI.controlsButtonHeight,
                ConstantFrameUI.controlsButtonWidth, ConstantFrameUI.controlsButtonHeight, "All", texture, 0);
            ViewLayer1 = createButton(ui, 16, this.Height - ConstantFrameUI.controlsButtonHeight,
                ConstantFrameUI.controlsButtonWidth, ConstantFrameUI.controlsButtonHeight, "L1", texture, 1);
            ViewLayer2 = createButton(ui, 16, this.Height - ConstantFrameUI.controlsButtonHeight,
                ConstantFrameUI.controlsButtonWidth, ConstantFrameUI.controlsButtonHeight, "L2", texture, 2);
            ViewLayer3 = createButton(ui, 16, this.Height - ConstantFrameUI.controlsButtonHeight,
                ConstantFrameUI.controlsButtonWidth, ConstantFrameUI.controlsButtonHeight, "L3", texture, 3);
            ViewLayer4 = createButton(ui, 16, this.Height - ConstantFrameUI.controlsButtonHeight,
                ConstantFrameUI.controlsButtonWidth, ConstantFrameUI.controlsButtonHeight, "L4", texture, 4);
            this.Add(buttomPanel);
            this.Add(allViewLayers);
            this.Add(ViewLayer1);
            this.Add(ViewLayer2);
            this.Add(ViewLayer3);
            this.Add(ViewLayer4);
        }

        private ButtonIcon createButton(FrameProcessor ui, int x, int y, int w, int h, string text, Texture image, int position)
        {
            var button = new ButtonIcon(ui, x + w*position, y, w, h, text, ColorConstant.ScenarioBackColorButton)
            {
                ImageWidth = ConstantFrameUI.sizeIcon,
                ImageHeight = ConstantFrameUI.sizeIcon,
                HoverColor = ColorConstant.HoverColor,
                ColorActiveButton = ColorConstant.ActiveElement,
                ColorImage = ColorConstant.TextColor,
                Image = image,
                Anchor = FrameAnchor.Bottom,
                Font = ConstantFrameUI.segoeReg12,
            };
            button.Click += (sender, args) =>
            {
                if(panelManager==null)
                    return;
                if (currentFilter.Contains(position))
                {
                    currentFilter.Remove(position);
                }
                else
                {
                   currentFilter.Add(position);
                }
                if(!currentFilter.Contains(0))
                    setControls(panelManager.ControlElements.Select(pair => pair).Where((list, i) => currentFilter.Contains(list.Key)).Select(el => el.Value).SelectMany(el => el));
                else
                    setControls(panelManager.ControlElements.Select(pair => pair.Value).SelectMany(el => el));
            };
            return button;
        }

        public void setPanelManager(PanelManager panelManager)
        {
            this.panelManager = panelManager;

            allViewLayers.ChangeStatus(true);
            ViewLayer1.ChangeStatus(false);
            ViewLayer2.ChangeStatus(false);
            ViewLayer3.ChangeStatus(false);
            ViewLayer4.ChangeStatus(false);

            currentFilter.Add(0);
            setControls(panelManager.ControlElements.Select(pair => pair.Value).SelectMany(el => el));
        }

        private void setControls(IEnumerable<Frame> listControls)
        {
            treeControls.removeAllNode();
            foreach (var control in listControls)
            {
                if(control!=null)
                    treeControls.addNode(control);
            }
        }

        public void transition(bool open)
        {
            this.RunTransition("X", open ? this.Game.RenderSystem.DisplayBounds.Width - ConstantFrameUI.controlsFrameUIWidth : this.Game.RenderSystem.DisplayBounds.Width, 0, 500);
        }
    }
}
