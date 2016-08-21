using System;
using System.Collections.Generic;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Settings;

namespace Pulse.Common.ConfigSystem
{
    public abstract class PulseScenarioConfig : PulseBaseScenarioConfig
    {
        private SimulationProperties _simulationProperties = null;
        public SimulationProperties SimulationProperties { get { return GetSingleton(_simulationProperties, new Lazy<SimulationProperties>(InitSimulationProperties)); } }

        private GeoMapConfig _mapConfig = null;
        public GeoMapConfig MapConfig { get { return GetSingleton(_mapConfig, new Lazy<GeoMapConfig>(InitMapConfig)); } }

        private GeoWorldGeneralInfo _geoWorldInfo = null;
        public GeoWorldGeneralInfo GeoWorldInfo { get { return GetSingleton(_geoWorldInfo, new Lazy<GeoWorldGeneralInfo>(InitGeoWorldInfo)); } }
        
        public IDictionary<string, string> SfConfig { get; set; } = new Dictionary<string, string>();

        private GeoWorldGeneralInfo InitGeoWorldInfo()
        {
            _geoWorldInfo = new GeoWorldGeneralInfo
            {
                GeoTime = new GeoClockContainer(TimeStart.Value),
                MetersPerMapUnit = MetersPerMapUnit.Value,
                ToSecondsMultiplier = ToSecondsMultiplier.Value
            };

            return _geoWorldInfo;
        }

        private SimulationProperties InitSimulationProperties()
        {
            _simulationProperties = new SimulationProperties
            {
                TimeProperties = new SimTimeProperties
                {
                    TimeStep = TimeStep.Value,
                    ToSecondsMultiplier = ToSecondsMultiplier.Value
                }
            };
            return _simulationProperties;
        }

        private GeoMapConfig InitMapConfig()
        {
            _mapConfig = new GeoMapConfig(MapPointMin.Value, MapPointMax.Value, GeoPointMin.Value, GeoPointMax.Value, MetersPerMapUnit.Value);
            return _mapConfig;
        }
    }
}
