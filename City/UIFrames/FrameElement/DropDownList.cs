using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    class DropDownList : Frame
    {
        public String selectedItem;
        public List<String> itemList { get; set; }
        private List<Frame> dropDownList; 
        private FrameProcessor ui;
        private bool isDropDown=false;
        public bool directionDown = false;

        public Action<string> Changed = (e) => { };
         
        public DropDownList(FrameProcessor ui) : base(ui)
        {
            Text = "";
            init(ui);
        }

        public DropDownList(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui);
        }

        void init(FrameProcessor ui)
        {
            this.ui = ui;
            var sizeText = this.Font.MeasureString(this.Text);
            this.Width = sizeText.Width;
            this.Height = sizeText.Height;
            itemList = new List<string>();
            Click += Click_dropDownList;
            dropDownList = new List<Frame>();
        }

        void Click_dropDownList(object sender, EventArgs e)
        {
            if (isDropDown)
            {
                setDropDownElement(this.Text);
                return;
            }
            isDropDown = true;
            int y = this.GlobalRectangle.Y;
            foreach (var item in itemList)
            {
                if (directionDown)
                    y += this.Font.LineHeight;
                else
                    y -= this.Font.LineHeight;
                Frame itemFrame = new Frame(this.ui, this.X, y, Width, Height, item, BackColor)
                {
                    TextAlignment = this.TextAlignment,
                    Border =  this.BorderLeft,
                    BorderColor = this.BorderColor,
                };
                itemFrame.Click += (s, ev) =>
                {
                    setDropDownElement(itemFrame.Text);
                };
                this.ui.RootFrame.Add(itemFrame);
                this.ui.RootFrame.Click += setDropDownElement;
                dropDownList.Add(itemFrame);
            }
        }

        private void setDropDownElement(object sender, MouseEventArgs e)
        {
            setDropDownElement(this.Text);
        }

        void setDropDownElement(String text)
        {
            this.selectedItem = text;
            this.Text = text;
            this.isDropDown = false;



            foreach (var dropDownElement in dropDownList)
            {
                this.ui.RootFrame.Remove(dropDownElement);
            }
            dropDownList.Clear();
            this.ui.RootFrame.Click -= setDropDownElement;

            Changed(this.Text);
        }
    }
}
