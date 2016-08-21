using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using City.UIFrames.FrameElement;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace City.ControlsClient.DomainClient.VKFest.UI
{
    public class NewsFeed : Frame {

        private ListBox listNewsElement;
        public NewsFeed(FrameProcessor ui) : base(ui)
        {
            Init(ui);
        }

        public NewsFeed(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            Init(ui);
        }

        private void Init(FrameProcessor frameProcessor)
        {
            listNewsElement = new ListBox(frameProcessor, 0, this.Height, 0, 0, "", Color.Zero);
            this.Add(listNewsElement);
        }

        public void addNewPost(NewsFeedElement element)
        {
            listNewsElement.addElement(element);
            listNewsElement.Y -= element.Height;
        }
    }
}
