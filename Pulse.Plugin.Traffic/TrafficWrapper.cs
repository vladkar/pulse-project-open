using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EmbeddableTraffic;
using Pulse.Common;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Loaders;

namespace Pulse.Plugin.Traffic
{
    public class TrafficWrapper : AbstractDataBroker, IComplexUpdatable
    {
        private EmbeddableTrafficEngine _engine;

        protected override void LoadData()
        {
            // General configuration parameters
            var config = new EmbeddableTrafficEngineConfig
            {
                RoadGraphFilePath = "zsd.XML",
                SimulationStartsAt = 0,
                TimeStep = 1
            };

            // Constuctor
            _engine = new EmbeddableTrafficEngine();

            // Initialize with parameters
            _engine.InitializeEngine(config);
        }

        public void Update(double timeStep, double time, DateTime geotime)
        {
            _engine.Step();
        }
    }
}
