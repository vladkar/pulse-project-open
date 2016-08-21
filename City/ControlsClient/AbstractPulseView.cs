using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using City.ControlsServer;
using City.Panel;
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
    public abstract class AbstractPulseView<T> : IPulseControlledView<T> where T : IPulseControl
    {
        protected IDictionary<string, DiscTexture> Textures { get; set; }
        protected Game Game { get; private set; }

        public EnumElement ControlsState { get; protected set; } = PulseRunnerState.NotInitialized;
        public ControlInfo ControlConfig { get; protected set; }

        
        public T Control { get; set; }
        public RenderLayer ViewLayer	{ get; set; }
		public IPulsePanel Panel		{ get; set; }
		public IList<ILayerWrapper> Layers { get; set; }

        public virtual void Initialize(Game gameEngine)
        {
            if (ControlsState == PulseRunnerState.NotInitialized)
            {
                ControlsState = PulseRunnerState.Initializing;
                Game = gameEngine;
                Layers = new List<ILayerWrapper>();
                InitializeView();
                ControlsState = PulseRunnerState.Initialized;
            }
        }

        public virtual void Load(ControlInfo controlInfo)
        {
            if (ControlsState == PulseRunnerState.Initialized)
            {
                ControlConfig = controlInfo;
                ControlsState = PulseRunnerState.Loading;
                LoadView(controlInfo);
                ControlsState = PulseRunnerState.Loaded;
            }
        }

        public virtual void Unload()
        {
            ControlsState = PulseRunnerState.Unloading;
            UnloadView();
            ControlsState = PulseRunnerState.Finished;
        }

        public ICommandSnapshot Update(GameTime gameTime)
        {
            return UpdateView(gameTime);
        }

        public abstract Frame AddControlsToUI();

        public IList<ILayerWrapper> GetLayers()
        {
            return Layers;
        }


        protected abstract void InitializeView();
        protected abstract void LoadView(ControlInfo controlInfo);
        protected abstract ICommandSnapshot UpdateView(GameTime gameTime);
        protected abstract void UnloadView();
        
//        public abstract string UserInfo();
//
//        public IList<Gis.GisLayer> GetGisLayers()
//        {
//            return GisLayers;
//        }
    }
}