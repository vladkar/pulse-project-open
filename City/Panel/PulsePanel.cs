using System.Collections.Generic;
using City.ControlsClient;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;

namespace City.Panel
{
    public interface IPulsePanel
    {
        IList<IPulseView> Views { get; set; }
        RenderLayer PanelLayer { get; }

        void Initialize();
        void Load();
        void Unload();
        void Update(GameTime gameTime);
    }

    public abstract class PulsePanel : IPulsePanel
    {
        public  RenderLayer PanelLayer { get; protected set; }
        public IList<IPulseView> Views { get; set; }
        protected Game GameEngine { get; set; }

        protected PulsePanel(Game e, RenderLayer vl)
        {
            PanelLayer = vl;
            GameEngine = e;
            Views = new List<IPulseView>();
        }

        public abstract void Initialize();
        public abstract void Load();
        public abstract void Unload();
        public abstract void Update(GameTime gameTime);
    }
}
