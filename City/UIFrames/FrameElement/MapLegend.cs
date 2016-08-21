using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;

namespace City.UIFrames.Impl
{
    public class MapLegend : Frame
    {
        private FrameProcessor ui;

        private int ListElementHeight = 20;

        private ListBox ListLegendElement;

        private Frame Next;
        private Frame Prev;
        private List<Frame> AddedFrames;

        private int CountElementOnPage;
        private int CurrentPosition;

        public MapLegend(FrameProcessor ui) : base(ui)
        {
            Init(ui);
        }

        public MapLegend(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init(ui);
        }

        private void Init(FrameProcessor ui)
        {
            this.ui = ui;
            ListLegendElement = new ListBox(ui);
            AddedFrames = new List<Frame>();
            CountElementOnPage = this.Height/ListElementHeight;
            CurrentPosition = 0;
            this.Add(ListLegendElement);
        }

        public void addNewElement(Texture texture, string description)
        {
            var newElement = new ImageWithText(ui, 0, 0, this.Width, ListElementHeight, description, Color.Zero)
            {
                Image = texture,
                sizePicture = 15,
                PaddingLeft = 5,
            };
            AddedFrames.Add(newElement);
            if (CountElementOnPage  < AddedFrames.Count)
            {
                if (Next == null)
                {
                    createExpandButton();
                }
            }
            else
            {
                this.ListLegendElement.addElement(newElement);
            }
        }

        private void createExpandButton()
        {
            Next = FrameHelper.createButton(ui, this.Width - 10, 0, 10, 10, "+", NextPage);
            Prev = FrameHelper.createButton(ui, this.Width - 10, this.Height-10, 10, 10, "-", PrevPage);
            this.Add(Next);
            this.Add(Prev);
        }



        private void NextPage()
        {
            if (CurrentPosition + CountElementOnPage >= AddedFrames.Count())
                return;
            switchElements(CurrentPosition, CurrentPosition + CountElementOnPage, false);
            CurrentPosition += CountElementOnPage;
            var to = CurrentPosition + CountElementOnPage > AddedFrames.Count
                ? AddedFrames.Count
                : CurrentPosition + CountElementOnPage;
            switchElements(CurrentPosition, to, true);
        }

        private void PrevPage()
        {
            if (CurrentPosition - CountElementOnPage < 0)
                return;
            var to = CurrentPosition + CountElementOnPage > AddedFrames.Count
                ? AddedFrames.Count
                : CurrentPosition + CountElementOnPage;
            switchElements(CurrentPosition, to, false);
            CurrentPosition -= CountElementOnPage;
            switchElements(CurrentPosition, CurrentPosition + CountElementOnPage, true);
        }

        private void switchElements(int from, int to, bool IsAdd)
        {
            for (int i = from; i < to; i++)
            {
                if(IsAdd)
                    this.ListLegendElement.addElement(AddedFrames[i]);
                else
                    this.ListLegendElement.RemoveElement(AddedFrames[i]);
            }
        }
    }
}
