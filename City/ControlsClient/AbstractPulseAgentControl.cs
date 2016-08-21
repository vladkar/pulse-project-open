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
    public abstract class AbstractPulseAgentControl : AbstractPulseControl
    {
        protected ModelFactoryReflection EngineFactory { get; private set; }

        protected ISnapshot CurrentSnapShot { get; set; }

        protected PulseScenarioConfig ScenarioConfig { get; set; }
        protected PulseEngine Engine { get; set; }
        protected PulseMapData PulseMap { get; set; }
        protected CoordType CoordSystem { set; get; }

        protected virtual void InitializeControl(Game gameEngine)
        {
            EngineFactory = new ModelFactoryReflection();
        }

        protected override void LoadControl(ControlInfo controlInfo)
        {
            EngineFactory = new ModelFactoryReflection();
            ScenarioConfig = EngineFactory.GetConfig(controlInfo.Scenario);
            Engine = EngineFactory.GetEngine(ScenarioConfig, SimulationRunMode.StepByStep) as PulseEngine;
            Engine.Start();
            PulseMap = Engine.World.Map.GetMapData() as PulseMapData;
            Configure(Engine.ScenarioConfig);
        }

        private void Configure(PulseScenarioConfig scenarioConfig)
        {
            CoordSystem = scenarioConfig.PreferredCoordinates.Value == "geo" ? CoordType.Geo : CoordType.Map;
        }

        public override void FeedSnapshot(ISnapshot snapshot)
        {
            CurrentSnapShot = snapshot;
        }

        protected override bool ValidateSnapshot()
        {
            if (CurrentSnapShot != null) return true;
            return false;
        }
    }
}