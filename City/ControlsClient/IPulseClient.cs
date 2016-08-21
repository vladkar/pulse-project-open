using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using City.Panel;
using City.Snapshot;
using City.Snapshot.Snapshot;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Pulse.Common.Utils;

namespace City.ControlsClient
{
    public interface IPulseControl
    {
        IDictionary<int, IPulseView> Views { set; get; }

        void Initialize(Game gameEngine);
        void Load(ControlInfo serverInfo);
        void Unload();
        ICommandSnapshot[] Update(GameTime gameTime);
        void FeedSnapshot(ISnapshot snapshot);
        string UserInfo();

        EnumElement ControlsState { get; }
    }

    public interface IPulseView
    {
        RenderLayer ViewLayer	{ get; set; }
		IPulsePanel Panel		{ get; set; }

        void Initialize(Game gameEngine);
        void Load(ControlInfo serverInfo);
        void Unload();
        ICommandSnapshot Update(GameTime gameTime);

        Frame AddControlsToUI();
        IList<ILayerWrapper> GetLayers();
    }

    public interface IPulseControlledView<T> : IPulseView where T : IPulseControl
    {
        T Control { set; get; }
    }

    public interface ILayerWrapper
    {
        object GetLayer();
    }

    public class GisLayerWrapper : ILayerWrapper
    {
        public Gis.GisLayer Layer { set; get; }

        public GisLayerWrapper(Gis.GisLayer layer)
        {
            Layer = layer;
        }

        public object GetLayer()
        {
            return Layer;
        }
    }

    public class SpriteLayerWrapper : ILayerWrapper
    {
        public SpriteLayer Layer { set; get; }

        public SpriteLayerWrapper(SpriteLayer textSpriteLayer)
        {
            Layer = textSpriteLayer;
        }

        public object GetLayer()
        {
            return Layer;
        }
    }
}