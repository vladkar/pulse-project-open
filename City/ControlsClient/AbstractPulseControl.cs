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
    public abstract class AbstractPulseControl : IPulseControl
    {
        public EnumElement ControlsState { get; protected set; } = PulseRunnerState.NotInitialized;
        public ControlInfo ControlConfig { get; protected set; }
        public Game Game { get; protected set; }

        public IDictionary<int, IPulseView> Views { set; get; } = new Dictionary<int, IPulseView>();


        public virtual void Initialize(Game gameEngine)
        {
            if (ControlsState == PulseRunnerState.NotInitialized)
            {
                this.Game = gameEngine;
                ControlsState = PulseRunnerState.Initializing;
                InitializeControl();
                foreach (var view in Views)
                {
                    view.Value.Initialize(gameEngine);
                }
//                Views.AsParallel().ForAll(v => v.Value.Initialize(gameEngine));
                ControlsState = PulseRunnerState.Initialized;
            }
        }


        public virtual void Load(ControlInfo controlInfo)
        {
            if (ControlsState == PulseRunnerState.Initialized)
            {
                ControlConfig = controlInfo;
                ControlsState = PulseRunnerState.Loading;
                LoadControl(controlInfo);
//                Views.AsParallel().ForAll(v => v.Value.Load(controlInfo));

                foreach (var view in Views)
                {
                    view.Value.Load(controlInfo);
                }

                ControlsState = PulseRunnerState.Loaded;
            }
        }

        public virtual void Unload()
        {
            ControlsState = PulseRunnerState.Unloading;
            UnloadControl();
            Views.AsParallel().ForAll(v => v.Value.Unload());
            ControlsState = PulseRunnerState.Finished;
        }

        //TODO think about state check & test it
        public virtual ICommandSnapshot[] Update(GameTime gameTime)
        {
            if (ValidateSnapshot())
            {
                UpdateControl(gameTime);

                var cmds = new List<ICommandSnapshot>();
                foreach (var view in Views)
                {
                    cmds.Add(view.Value.Update(gameTime));
                }

//                Views.AsParallel().ForAll(v => v.Value.Update(gameTime));

                return cmds.ToArray();
            }
            else return null;
        }

        protected abstract void InitializeControl();
        protected abstract void LoadControl(ControlInfo controlInfo);
        protected abstract void UpdateControl(GameTime gameTime);
        protected abstract bool ValidateSnapshot();

        public abstract void FeedSnapshot(ISnapshot snapshot);
        public abstract void UnloadControl();
        public abstract string UserInfo();
    }
}