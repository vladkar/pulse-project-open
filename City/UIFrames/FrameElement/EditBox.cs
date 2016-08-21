using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Input;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class EditBox : Frame, INotifyPropertyChanged
    {
        public string Label;
        
        private FrameProcessor ui;
        private bool isFixWidth = true;
        private bool isHovered = false;
        private bool isActive = false;

        public int FontSize = 3;

        public bool HidePassword = false;
        public string AltText = null;


        public Color HoverColor;
        public Color BorderActive;

        private int PositionSubstr = 0; 
        const float carretBlinkTime = 0.4f;
        private bool inBound = true;


        public EditBox(FrameProcessor ui) : base(ui)
        {
            Text = "";
            init(ui);
        }

        public EditBox(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui); 
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public void init(FrameProcessor ui)
        {
            this.ui = ui;
            this.Game.GameInterface.Game.Keyboard.FormKeyPress += EditBox_KeyPress;
            this.Game.GameInterface.Game.Keyboard.KeyDown += EditBox_KeyDown;
            this.StatusChanged += (s, e) =>
            {
                isHovered = e.Status == FrameStatus.Hovered;
            };
            
            this.Click += (sender, args) =>
            {
                changeActiveStatus();
            };
        }


        private void changeActiveStatus()
        {
            isActive = !isActive;
            if(!isActive)
                OnPropertyChanged("Text");
        }


        /// <summary>
        /// Key-down event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditBox_KeyDown(object sender, Fusion.Engine.Input.KeyEventArgs e)
        {
            if (!isActive)
                return;
            counter = carretBlinkTime / 2;
            if (e.Key == Keys.Left)
            {
                carretPos = carretPos - 1;
            }
            if (e.Key == Keys.Right)
            {
                carretPos = carretPos + 1;
            }
            if (e.Key == Keys.Home)
            {
                carretPos = 0;
            }
            if (e.Key == Keys.End)
            {
                carretPos = int.MaxValue;
            }
            if (e.Key == Keys.Delete && carretPos < Text.Length)
            {
                Text = Text.Remove(carretPos, 1);
            }
        }


        /// <summary>
        /// Key-press event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void EditBox_KeyPress(object sender, KeyPressArgs e)
        {
            if (!isActive)
                return;
            counter = carretBlinkTime / 2;

            if (e.KeyChar == '\b')
            {
                if (carretPos > 0)
                {
                    Text = Text.Remove(carretPos - 1, 1);
                    carretPos = carretPos - 1;
                }
                return;
            }

            if (e.KeyChar == '\t' || e.KeyChar == '\r' || e.KeyChar == '\n')
            {
                return;
            }

            Text = Text.Insert(carretPos, new string(e.KeyChar, 1));

            carretPos++;
        }




        int _carretPos;

        int carretPos
        {
            get
            {
                return _carretPos;
            }
            set
            {
                _carretPos = MathUtil.Clamp(value, 0, Text.Length);
            }
        }


        bool showCarret;
        float counter = 0;


        /// <summary>
        /// Update
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void Update(GameTime gameTime)
        {
            if(!this.ui.TargetFrame.Equals(this) && isActive)
                changeActiveStatus();
            if (!isFixWidth)
            {
                var r = this.Font.MeasureString(Text);

                var w = r.Width + 2 * PaddingLeft;
                var h = r.Height + 2 * PaddingBottom;

                Width = Math.Max(w, Width);
                Height = Math.Max(h, Height);
            }
            

            //
            //	Handle carret blink
            //
            counter -= gameTime.ElapsedSec;

            if (counter < 0)
            {
                counter = carretBlinkTime;
            }
            showCarret = counter <= carretBlinkTime / 2;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentOpacity"></param>
        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            bool showAltText = false;

            var value = Text.Substring(PositionSubstr);

            if (HidePassword)
            {
                value = new string((char)0x2022, value.Length);
            }

            if (!string.IsNullOrEmpty(AltText) && string.IsNullOrEmpty(Text) && !isActive)
            {
                value = AltText;
                showAltText = true;
            }

            var sizeText = this.Font.MeasureString(value);
            int carretOffs = this.Font.MeasureString(value.Substring(0, carretPos)).Width;

            var gr = GlobalRectangle;
            int labelHeight = 0;
            if (!string.IsNullOrEmpty(Label))
            {
                labelHeight = this.Font.CapHeight*3/2;
                this.Font.DrawString(sb, Label, gr.X + PaddingLeft, gr.Y, Color.White, clipRectIndex, 0, false);
            }
                
            
            var texWhite = this.Game.RenderSystem.WhiteTexture;
            sb.Draw(texWhite, gr.X, gr.Y + labelHeight, gr.Width, gr.Height - labelHeight, isHovered ? HoverColor : BackColor, clipRectIndex);

            if (isActive && showCarret)
            {
                sb.Draw(texWhite, gr.X + PaddingLeft + carretOffs, gr.Y + PaddingBottom + labelHeight, 2, sizeText.Height, Color.White, clipRectIndex);
            }
            this.Font.DrawString(sb, value, gr.X + PaddingLeft, gr.Y + PaddingBottom + labelHeight, showAltText ? Color.Gray : Color.White, clipRectIndex, 0, false);

            sb.Draw(texWhite, (float)gr.X, (float)gr.Y + labelHeight, (float)gr.Width, (float)BorderTop, BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)gr.X, (float)(gr.Y + gr.Height - BorderBottom), (float)gr.Width, (float)BorderBottom, BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)gr.X, (float)(gr.Y + BorderTop + labelHeight), (float)BorderLeft, (float)(gr.Height - labelHeight - BorderTop - BorderBottom), BorderActive, clipRectIndex);
            sb.Draw(texWhite, (float)(gr.X + gr.Width - BorderRight), (float)(gr.Y + BorderTop + labelHeight), (float)BorderRight, (float)(gr.Height - labelHeight - BorderTop - BorderBottom), BorderActive, clipRectIndex);
        }

        private int getCountSub(string text)
        {
            var x =  (int) ((Font.MeasureString(text).Width - this.Width)/this.Font.SpaceWidth);
            return x;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
