using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;

namespace City.UIFrames.Impl
{
    public class MultiViewPanel : Frame
    {
        private FrameProcessor ui;
        public List<ViewFrame> listFrame;
        public Orientation orientation { get; set; }
        private Frame ControlPanel;

        public MultiViewPanel(FrameProcessor ui) : base(ui)
        {
            Init(ui);
        }

        public MultiViewPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init(ui);
        }

        private void Init(FrameProcessor ui)
        {
            this.ui = ui;
        }

        public void ConfiguratePanel(Orientation orientation)
        {
            // clear
            if(listFrame != null)
                foreach (var vl in listFrame)
                {
                    Game.RenderSystem.RemoveLayer(vl.viewLayers);
                }
            this.orientation = orientation;
            Anchor = FrameAnchor.All;
            listFrame = new List<ViewFrame>();
            // init process
            switch (orientation)
            {
                case Orientation.Instagram:
                case Orientation.VkFest:
                case Orientation.One:
                    // f1 -> l1
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width, this.Height, "", ColorConstant.Zero, ColorConstant.Zero));

                    break;
                case Orientation.TwoVertical:
                    //                    f1 -> l1 left
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width/2, this.Height, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, 0, this.Width / 2, this.Height, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    //                    f2 -> l2 right
                    break;
                case Orientation.TwoHoriz:
                    //                    f1 -> l1 top
                    //                    f2 -> l2 bottom
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height/2, this.Width, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
                case Orientation.OneLeftTwoRight:
                    //                    f1 -> l1 left
                    //                    f2 -> l2 right top
                    //                    f3 -> l3 right bottom
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width / 2, this.Height, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, 0, this.Width / 2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, this.Height / 2, this.Width / 2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
                case Orientation.TwoLeftOneRight:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 left bottom 
                    //                    f3 -> l3 right 
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width / 2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height / 2, this.Width / 2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, 0, this.Width / 2, this.Height, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
                case Orientation.OneTopTwoBottom:
                    //                    f1 -> l1 top
                    //                    f2 -> l2 left bottom 
                    //                    f3 -> l3 right bottom  
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height/2, this.Width/2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width/2, this.Height/2, this.Width/2, this.Height/2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
                case Orientation.TwoTopOneBottom:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 right top 
                    //                    f3 -> l3  bottom  
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width/2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, 0, this.Width / 2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height / 2, this.Width, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
                case Orientation.Train:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 right top 
                    //                    f3 -> l3  bottom  
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width * 2 / 3, this.Height * 2 / 3, "", ColorConstant.trainBackColor, ColorConstant.trainBorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width * 2 / 3, 0, this.Width / 3, this.Height * 2 / 3, "", ColorConstant.trainBackColor, ColorConstant.trainBorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height * 2 / 3, this.Width, this.Height / 3, "", ColorConstant.trainBackColor, ColorConstant.trainBorderColor));
                    break;
                case Orientation.All:
                    listFrame.Add(new ViewFrame(ui, 0, 0, this.Width / 2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, 0, this.Width / 2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, 0, this.Height / 2, this.Width / 2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    listFrame.Add(new ViewFrame(ui, this.Width / 2, this.Height / 2, this.Width / 2, this.Height / 2, "", ColorConstant.Zero, ColorConstant.BorderColor));
                    break;
            }

			int i = 0;
            foreach (var frame in listFrame)
            {
                this.Add(frame);

				var targetFormat = TargetFormat.LowDynamicRange;

#warning This should be changed
				if((i == 0 || i == 2) && Game.RenderSystem.MsaaEnabled) {
					targetFormat = TargetFormat.LowDynamicRangeMSAA;
				}

				var viewLayer = new RenderLayer(this.Game)
                {
                    Target = new TargetTexture(Game.RenderSystem, frame.Width - frame.BorderLeft - frame.BorderRight, frame.Height - frame.BorderTop - frame.BorderBottom, targetFormat),
                    Clear = true,
                };
                frame.Image = viewLayer.Target;
				frame.ImageMode = FrameImageMode.Stretched;
                frame.init(viewLayer.GlobeCamera, viewLayer);

                Game.RenderSystem.AddLayer(viewLayer);
				i++;
            }

        }

        public void resize(CustomGameInterface.UIenum uiLayout)
        {
            var newWidth = 0;
            var newHeight = 0;
            switch (uiLayout)
            {
                case CustomGameInterface.UIenum.ht:
                    newWidth = this.Game.RenderSystem.DisplayBounds.Width - ConstantFrameUI.menuFrameUIWidth -
                         ConstantFrameUI.controlsFrameUIWidth;
                    newHeight = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.statusFrameUIHeight -
                                    ConstantFrameUI.topFrameUIHeight;
                    break;
                case CustomGameInterface.UIenum.train:
                    newWidth = this.Game.RenderSystem.DisplayBounds.Width;
                    newHeight = this.Game.RenderSystem.DisplayBounds.Height - ConstantFrameUI.specBottomFrameUIHeight;
                    break;
                case CustomGameInterface.UIenum.ins:
                    newWidth = this.Game.RenderSystem.DisplayBounds.Width;
                    newHeight = this.Game.RenderSystem.DisplayBounds.Height;
                    break;
            }
            switch (orientation)
            {
                case Orientation.VkFest:
                case Orientation.Instagram:
                case Orientation.One:
                    // f1 -> l1
                    updateViewFrame(listFrame[0], 0, 0, newWidth, newHeight);
                    break;
                case Orientation.TwoVertical:
                    //                    f1 -> l1 left
                    updateViewFrame(listFrame[0], 0, 0, newWidth / 2, newHeight);
                    updateViewFrame(listFrame[1], newWidth / 2, 0, newWidth /2, newHeight);
                    //                    f2 -> l2 right
                    break;
                case Orientation.TwoHoriz:
                    //                    f1 -> l1 top
                    //                    f2 -> l2 bottom
                    updateViewFrame(listFrame[0], 0, 0, newWidth, newHeight/2);
                    updateViewFrame(listFrame[1], 0, newHeight / 2, newWidth / 2, newHeight / 2);
                    break;
                case Orientation.OneLeftTwoRight:
                    //                    f1 -> l1 left
                    //                    f2 -> l2 right top
                    //                    f3 -> l3 right bottom
                    updateViewFrame(listFrame[0], 0, 0, newWidth/2, newHeight);
                    updateViewFrame(listFrame[1], newWidth / 2, 0, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[2], newWidth / 2, newHeight / 2, newWidth/2, newHeight / 2);
                    break;
                case Orientation.TwoLeftOneRight:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 left bottom 
                    //                    f3 -> l3 right 
                    updateViewFrame(listFrame[0], 0, 0, newWidth / 2, newHeight/2);
                    updateViewFrame(listFrame[1], 0, newHeight / 2, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[2], newWidth / 2, 0, newWidth / 2, newHeight);
                    break;
                case Orientation.OneTopTwoBottom:
                    //                    f1 -> l1 top
                    //                    f2 -> l2 left bottom 
                    //                    f3 -> l3 right bottom  
                    updateViewFrame(listFrame[0], 0, 0, newWidth, newHeight / 2);
                    updateViewFrame(listFrame[1], 0, newHeight / 2, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[2], newWidth / 2, newHeight / 2, newWidth / 2, newHeight/2);
                    break;
                case Orientation.TwoTopOneBottom:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 right top 
                    //                    f3 -> l3  bottom  
                    updateViewFrame(listFrame[0], 0, 0, newWidth/2, newHeight / 2);
                    updateViewFrame(listFrame[1], newWidth / 2, 0, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[2], 0, newHeight / 2, newWidth, newHeight / 2);
                    break;
                case Orientation.Train:
                    //                    f1 -> l1 left top
                    //                    f2 -> l2 right top 
                    //                    f3 -> l3  bottom  
                    updateViewFrame(listFrame[0], 0, 0, newWidth * 2 / 3, newHeight * 2 / 3);
                    updateViewFrame(listFrame[1], newWidth * 2 / 3, 0, newWidth / 3, newHeight * 2 / 3);
                    updateViewFrame(listFrame[2], 0, newHeight * 2 / 3, newWidth, newHeight / 3);
                    break;
                case Orientation.All:
                    updateViewFrame(listFrame[0], 0, 0, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[1], newWidth / 2, 0, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[2], 0, newHeight / 2, newWidth / 2, newHeight / 2);
                    updateViewFrame(listFrame[3], newWidth / 2, newHeight / 2, newWidth / 2, newHeight / 2);
                    break;
            }
            //for (int i = 0;  i<listFrame.Count; i++)
            //{
            //    listFrame[i].X = (int)((float)listFrame[i].X * (float)newWidth / (float)this.Width);
            //    listFrame[i].Y = (int)((float)listFrame[i].Y * (float)newHeight / (float)this.Height);
            //    listFrame[i].Width = (int)((float)listFrame[i].Width * (float)newWidth / (float) this.Width);
            //    listFrame[i].Height = (int)((float)listFrame[i].Height * (float)newHeight / this.Height);
            //    listFrame[i].viewLayers.Target = 
            //        new TargetTexture(Game.RenderSystem,
            //        listFrame[i].Width - listFrame[i].BorderLeft - listFrame[i].BorderRight,
            //        listFrame[i].Height - listFrame[i].BorderTop - listFrame[i].BorderBottom,
            //        TargetFormat.LowDynamicRange);
            //    listFrame[i].Image = listFrame[i].viewLayers.Target;
            //}
        }

        private void updateViewFrame(ViewFrame vFrame, int x, int y, int width, int height)
        {
            vFrame.X = x;
            vFrame.Y = y;
            vFrame.Width = width;
            vFrame.Height = height;
            vFrame.viewLayers.Target =
                new TargetTexture(Game.RenderSystem,
                vFrame.Width - vFrame.BorderLeft - vFrame.BorderRight,
                vFrame.Height - vFrame.BorderTop - vFrame.BorderBottom,
                TargetFormat.LowDynamicRange);
            vFrame.Image = vFrame.viewLayers.Target;
        }
    }
}
