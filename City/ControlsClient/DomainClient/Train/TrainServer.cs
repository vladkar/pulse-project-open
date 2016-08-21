using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using City.ControlsServer;
using City.Snapshot;
using City.Snapshot.PulseAgent;
using City.Snapshot.Snapshot;
using Fusion;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Engine;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.Model;
using Pulse.Model.Environment;
using Pulse.MultiagentEngine.Engine;
using Pulse.Scenery.HarsiddhiTemple;

namespace City.ControlsClient.DomainClient.Train
{
    public class TrainServer : PulseServer
    {
        public TrainServer() : base()
        {
        }

        public TrainServer(int id) : base(id)
        {
        }

        protected override void UpdateCache()
        {
            base.UpdateCache();
        }
    }
}