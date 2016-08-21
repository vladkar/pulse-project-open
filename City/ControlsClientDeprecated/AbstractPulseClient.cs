using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using City.ControlsServer;
using City.Snapshot;
using City.Snapshot.Snapshot;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Utils;
using Pulse.Model;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Engine;

namespace City.ControlsClient
{

    public abstract class AbstractPulseClient : IPulseClient
    {
        #region static 

        static AbstractPulseClient()
        {
            GlobalState = PulseRunnerState.NotInitialized;
            _lock = new Object();
        }

        protected static IDictionary<string, DiscTexture> Textures { get; set; }
        protected static Game Game { get; private set; }
        private static EnumElement GlobalState { get; set; }

        private static Object _lock;

        #endregion

        public EnumElement ControlsState { get; protected set; } = PulseRunnerState.NotInitialized;
        public ControlInfo ControlConfig { get; protected set; }
        protected IList<Gis.GisLayer> GisLayers { get; set; }
        protected IList<RenderLayer> ViewLayers { get; set; }

        public virtual void Initialize(Game gameEngine)
        {
            #region static

            lock (_lock)
            if (GlobalState == PulseRunnerState.NotInitialized)
                {
                    GlobalState = PulseRunnerState.Initializing;

                    Game = gameEngine;
                    Textures = new ConcurrentDictionary<string, DiscTexture>();

                    GlobalState = PulseRunnerState.Initialized;
                }

            #endregion


            #region nonstatic

            if (ControlsState == PulseRunnerState.NotInitialized)
            {
                ControlsState = PulseRunnerState.Initializing;

                GisLayers = new List<Gis.GisLayer>();
                ViewLayers = new List<RenderLayer>();
                InitializeControl();

                ControlsState = PulseRunnerState.Initialized;
            }

            #endregion
        }

        public virtual void LoadLevel(ControlInfo controlInfo)
        {
            #region static

            lock (_lock)
                if (GlobalState == PulseRunnerState.Initialized)
                {
                    GlobalState = PulseRunnerState.Loading;

                    GlobalState = PulseRunnerState.Loaded;
                }

            #endregion


            #region nonstatic

            if (ControlsState == PulseRunnerState.Initialized)
            {
                ControlConfig = controlInfo;
                ControlsState = PulseRunnerState.Loading;

                LoadControl(controlInfo);

                ControlsState = PulseRunnerState.Loaded;
            }

            #endregion
        }

        public abstract Frame AddControlsToUI();

        protected abstract void InitializeControl();
        protected abstract void LoadControl(ControlInfo controlInfo);
        protected abstract ICommandSnapshot UpdateControl(GameTime gameTime);

        public virtual void FeedSnapshot(ISnapshot snapshot)
        {
            //            CurrentSnapShot = snapshot;
        }

        public virtual void UnloadLevel()
        {

        }

        public virtual ICommandSnapshot Update(GameTime gameTime)
        {
            if (Validate())     return UpdateControl(gameTime);
            else                return null;
        }

        protected virtual bool Validate()
        {
            return true;
        }

        public abstract string UserInfo();

        public void RegisterViewLayer(RenderLayer vl)
        {
            ViewLayers.Add(vl);
        }

        public void UnregisterVieLayer(RenderLayer vl)
        {
            ViewLayers.Remove(vl);
        }

        public IList<Gis.GisLayer> GetGisLayers()
        {
            return GisLayers;
        }
    }
}