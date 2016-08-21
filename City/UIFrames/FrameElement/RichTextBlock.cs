using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames.FrameElement
{
    public class RichTextBlock : Frame {

        private FrameProcessor ui;
        public int OffsetLine;

        public bool IsShortText = false;
        private int BaseHeight = 0;

        public RichTextBlock(FrameProcessor ui, SpriteFont font, int offsetLine) : base(ui)
        {
            init(ui, font, offsetLine);
        }

        public RichTextBlock(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor, SpriteFont font, int offsetLine) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui, font, offsetLine);
        }

        public void init(FrameProcessor ui, SpriteFont font, int offsetLine)
        {
            BaseHeight = this.Height;
            this.OffsetLine = offsetLine;
            this.ui = ui;
            this.Font = font;
            splitByString();
            var textOffset = 0;
            foreach (var str in strForDraw)
            {
               textOffset += this.Font.CapHeight + OffsetLine;
            }
            this.Height = textOffset + this.Font.CapHeight;

            if (IsShortText)
            {
                this.Height = BaseHeight;
            }
        }

        private List<string> strForDraw; 

        private void splitByString()
        {
            strForDraw = new List<string>();
            var words = Text.Replace("\n", " \n ").Replace("#", " #").Split(' ');
            bool haveMoreOneWord = false;
            var currentStr = "";
            var currentStrSize = 0;
            foreach (var word in words)
            {
                if (word.Equals("\n"))
                {
                    strForDraw.Add(currentStr);
                    currentStrSize = 0;
                    currentStr = "";
                    continue;
                }
                    
                var sizeText = this.Font.MeasureString(word);
                if (sizeText.Width + currentStrSize > this.Width)
                {
                    if (!haveMoreOneWord)
                    {
                        strForDraw.Add(word);
                        currentStrSize = 0;
                        currentStr = "";
                    }
                    else
                    {
                        strForDraw.Add(currentStr); 
                        currentStr = word + " ";
                        currentStrSize = sizeText.Width;
                    }
                }
                else
                {
                    haveMoreOneWord = true;
                    currentStr += word+" ";
                    currentStrSize += sizeText.Width + this.Font.SpaceWidth;
                }

            }
			strForDraw.Add(currentStr);
        }

        protected override void DrawFrame(GameTime gameTime, SpriteLayer sb, int clipRectIndex)
        {
            splitByString();
            var textOffset = 0;
            foreach (var str in strForDraw)
            {
                if (IsShortText && textOffset  > this.Height - this.Font.CapHeight - OffsetLine)
                {
                    this.Font.DrawString(sb, str.Length > 3 ? str.Remove(str.Length - 3) + "..." : "...", this.GlobalRectangle.X, this.GlobalRectangle.Y + textOffset,
                        ForeColor, 0, 0, false);
                    break;
                }
                this.Font.DrawString(sb, str, this.GlobalRectangle.X, this.GlobalRectangle.Y + textOffset,
                    ForeColor, 0, 0, false);
                textOffset += this.Font.CapHeight + OffsetLine;
            }
            this.Height = !IsShortText ? textOffset : BaseHeight;
        }
    }
}
