using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.IniParser.Model;
using Fusion.Core.Mathematics;
using Fusion.Engine.Graphics;
using Fusion.Engine.Frames;

namespace City.UIFrames.Impl
{
    public class TopPanel : Frame
    {
        private Texture Close;
        private Texture Expand;
        private Texture Minimize;

        public TopPanel(FrameProcessor ui) : base(ui)
        {
            init(ui);
        }

        public TopPanel(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor)
            : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        private void init(FrameProcessor ui)
        {
            this.X = 0;
            this.Y = 0;
            this.Width = this.Game.RenderSystem.DisplayBounds.Width;
            this.Height = ConstantFrameUI.topFrameUIHeight;
            this.Name = ConstantFrameUI.topPanelName;
            this.TextAlignment = Alignment.MiddleCenter;
            this.BackColor = ColorConstant.BackColor;
            this.Anchor = FrameAnchor.Top | FrameAnchor.Left | FrameAnchor.Right;

            this.Close = ui.Game.Content.Load<DiscTexture>(@"ui\close");
            this.Expand = ui.Game.Content.Load<DiscTexture>(@"ui\maximize");
            this.Minimize = ui.Game.Content.Load<DiscTexture>(@"ui\minimize");

            addButton(ui);
        }

        private void addButton(FrameProcessor ui)
        {
            var widthButton = 12*ConstantFrameUI.gridUnits;
            var heightButton = this.Height;
            
			
			var closeButton = new Button(ui, this.Width - widthButton, 0, widthButton, heightButton, "", ColorConstant.BackColor)
            {
                Image = Close,
                Anchor = FrameAnchor.Right,
                TextAlignment = Alignment.MiddleCenter,
                HoverColor = ColorConstant.DarkRed,
                ClickColor = ColorConstant.Red
            };
            closeButton.Click += (sender, args) => this.Game.Exit();


            var expandButton = new Button(ui, this.Width - 2*widthButton, 0, widthButton, heightButton, "", ColorConstant.BackColor)
            {
                Image = Expand,
                Anchor = FrameAnchor.Right,
                TextAlignment = Alignment.MiddleCenter,
                HoverColor = ColorConstant.Gray,
                ClickColor = ColorConstant.Blue
            };
            // TODO: expand
            expandButton.Click += (sender, args) => { 
				ui.Game.RenderSystem.Fullscreen = !ui.Game.RenderSystem.Fullscreen;

				expandButton.Image = ui.Game.RenderSystem.Fullscreen ? Minimize : Expand;
            };


            /*var trayButton = new Button(ui, this.Width - 3*widthButton, 0, widthButton, heightButton, "", ColorConstant.BackColor)
            {
                Image = Minimaze,
                Anchor = FrameAnchor.Right,
                TextAlignment = Alignment.MiddleCenter,
                HoverColor = ColorConstant.Gray,
                ClickColor = ColorConstant.Blue
            };
            // TODO: expand
            trayButton.Click += (sender, args) => { };*/
            this.Add(closeButton);
            this.Add(expandButton);
            //this.Add(trayButton);
        }
    }
}
