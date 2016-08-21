using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Media;
using Fusion.Engine.Frames;

namespace City.UIFrames.FrameElement
{
    public class VideoFrame : Frame
    {
        private const int sizeButton = 20;
        private bool IsDrag = false;
        public Vector2? PrevPosition = null;

        public Gis.GeoPoint? geoPoint;

        private Video video;
        private static VideoPlayer videoPlayer;

        public bool isPlaying = false;

        private static Frame frame;

        public VideoFrame(FrameProcessor ui, string filename) : base(ui)
        {
            init(ui, filename);
        }

        public VideoFrame(FrameProcessor ui, int x, int y, int w, int h, string text, Color backColor, string filename) : base(ui, x, y, w, h, text, backColor)
        {
            init(ui, filename);
        }

        private void init(FrameProcessor ui, string filename)
        {
            if (frame != null)
            {
                frame.Parent.Remove(frame);
                frame = null;
            }
            frame = this;
            ImageMode = FrameImageMode.Stretched;
            if (videoPlayer==null)
                videoPlayer = new VideoPlayer();
           
            video = new Video(filename);
            videoPlayer.Play(video);
            setDefaultSize();
            createControlPanel(ui);
        }

        public DynamicTexture getTexture()
        {
            return videoPlayer.GetTexture();
        }


        public void setDefaultSize()
        {
            this.Width = video.Width;
            this.Height = video.Height;
        }

        protected override void Update(GameTime gameTime)
        {
            if (geoPoint == null)
                Image = videoPlayer.GetTexture();
        }


        void createControlPanel(FrameProcessor ui)
        {
            var closeButton = new Button(ui, this.Width - sizeButton, 0, sizeButton, sizeButton, "X", new Color(1,0,0,125))
            {
                TextAlignment = Alignment.MiddleCenter,
            };
            closeButton.Click += (s, a) =>
            {
                this.Parent.Remove(this);
                frame = null;
            };
            this.Add(closeButton);
        }
    }
}
