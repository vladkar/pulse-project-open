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

    public abstract class AbstractPulseAgentClient : AbstractPulseClient
    {
        #region static 

        static AbstractPulseAgentClient()
        {
            Lock = new Object();
        }

        protected static ModelFactoryReflection EngineFactory { get; private set; }

        private static Object Lock;
        private  EnumElement GlobalState;

        #endregion

        protected PulseScenarioConfig ScenarioConfig { get; set; }
        protected ISnapshot CurrentSnapShot { get; set; }
        protected PulseEngine Engine { get; set; }
        protected PulseMapData PulseMap { get; set; }
        protected CoordType CoordSystem { set; get; }
        protected PointsGisLayer AgentsLayer { get; set; }


        protected AbstractPulseAgentClient()
        {
            GlobalState = PulseRunnerState.NotInitialized;
        }


        public override void Initialize(Game gameEngine)
        {
            base.Initialize(gameEngine);
            
            if (GlobalState == PulseRunnerState.NotInitialized)
            {
                GlobalState = PulseRunnerState.Initializing;
                EngineFactory = new ModelFactoryReflection();
                GlobalState = PulseRunnerState.Initialized;
            }
        }

        public override void LoadLevel(ControlInfo controlInfo)
        {
            if (GlobalState == PulseRunnerState.Initialized)
            {
                GlobalState = PulseRunnerState.Loading;

                ScenarioConfig = EngineFactory.GetConfig(controlInfo.Scenario);
                Engine = EngineFactory.GetEngine(ScenarioConfig, SimulationRunMode.StepByStep) as PulseEngine;
                Engine.Start();
                PulseMap = Engine.World.Map.GetMapData() as PulseMapData;
                Configure(Engine.ScenarioConfig);

                GlobalState = PulseRunnerState.Loaded;
            }


            base.LoadLevel(controlInfo);
        }

        private void Configure(PulseScenarioConfig scenarioConfig)
        {
            CoordSystem = scenarioConfig.PreferredCoordinates.Value == "geo" ? CoordType.Geo : CoordType.Map;
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
            CurrentSnapShot = snapshot;
        }

        public override void UnloadLevel()
        {
            
        }

        protected override ICommandSnapshot UpdateControl(GameTime gameTime)
        {
           // TileMap.Update(gameTime);
            return null;
        }

        protected override bool Validate()
        {
            if (CurrentSnapShot != null) return true;
            return false;
        }
    }
}