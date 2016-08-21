using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using City.Snapshot;
using City.Snapshot.Snapshot;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;

namespace City.ControlsClient
{
    public interface IPulseClient
    {
        void Initialize(Game gameEngine);
        void LoadLevel(ControlInfo serverInfo);
        Frame AddControlsToUI();
        void UnloadLevel();
        ICommandSnapshot Update(GameTime gameTime);
        void FeedSnapshot(ISnapshot snapshot);
        string UserInfo();

        void RegisterViewLayer(RenderLayer vl);
        void UnregisterVieLayer(RenderLayer vl);
        IList<Gis.GisLayer> GetGisLayers();
    }
}