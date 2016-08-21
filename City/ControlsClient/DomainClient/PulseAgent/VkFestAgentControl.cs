using System;
using System.Collections.Generic;
using System.Linq;
using City.ControlsClient.DomainClient.PulseAgent;
using City.ControlsClient.DomainClient.Train;
using City.Snapshot;
using City.Snapshot.Infection;
using City.Snapshot.Navfield;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using City.UIFrames.Converter;
using City.UIFrames.Converter.Models;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Engine.Common;
using Fusion.Engine.Frames;
using Fusion.Engine.Graphics.GIS;
using Fusion.Engine.Graphics.GIS.DataSystem.MapSources.Projections;
using Fusion.Engine.Graphics.GIS.GlobeMath;
using Fusion.Engine.Input;
using Pulse.Common.Model.Agent;
using Pulse.Common.Utils;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Infection;


namespace City.ControlsClient.DomainClient
{
    public class VkFestAgentControl : PulseMicroModelControl
    {
        protected override void InitializeControl()
        {
            var v1 = new PulseMicroModelDeveloperView(this);
            var v2 = new VkFestGraphics(this);

            Views[1] = v1;
            Views[2] = v2;
        }
    }
}
