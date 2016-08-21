using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common;
using Pulse.Common.ConfigSystem;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Model.Environment;
using Pulse.Common.Scenery.Loaders;
using Pulse.Common.Utils;
using Pulse.Model.Agents;
using Pulse.Plugin.SubModel;
using Pulse.Plugin.Traffic;
using Pulse.Scenery.StPetersburg.Net;

namespace Pulse.Model.MovementSystems
{
    public class MovementSystemFactory : AbstractDataBroker
    {
        private IDictionary<MovementSystemTypes, List<Common.Model.Agent.MovementSystem>> _availableSystems;
        private PulseScenarioConfig _config;
        private PulseScenery _sb;

        public MovementSystemFactory(PulseScenarioConfig config, PulseScenery sb)
        {
            _availableSystems = new Dictionary<MovementSystemTypes, List<Common.Model.Agent.MovementSystem>>();

            _config = config;
            _sb = sb;
        }

        public Common.Model.Agent.MovementSystem GetMovementSystem(MovementSystemTypes type, int subgroupId = 0)
        {
            return _availableSystems[type][subgroupId];
        }
        // тип агента
        public static MovementSystemTypes GetTypeByName(string name)
        {
            switch (name.ToLower())
            {
                case "pedestrian":
                    return MovementSystemTypes.PEDESTRIAN;
                case "vehicle":
                    return MovementSystemTypes.VEHICLE;
                case "sub_model":
                    return MovementSystemTypes.SUB_MODEL;
                default:
                    throw new Exception("Unknown movement system");
            }
        }

        public Common.Model.Agent.MovementSystem GetImplementationByName(string name)
        {
            var gcu = new GeoCartesUtil(_config.MapConfig.MinGeo, _config.MapConfig.MetersPerMapUnit);
            switch (name.ToLower())
            {
                case "default":
                    return new SimpleMovementSystem(_config, _sb, gcu);
                case "simple":
                    return new SimpleMovementSystem(_config, _sb, gcu);
                case "simplevelocity":
                    return new SimpleVelocityMovementSystem(_config, _sb, gcu);
                case "rvo":
                    return new RvoMovementSystem(_config, _sb);
                case "sf":
                    return new SfMovementSystem(_config, _sb);
                case "lsf":
                    return new SfMultilevelMovementSystem(_config, _sb);
                case "traffic_model":
                    return new TrafficMovementSystem2D(_config, _sb);
                case "sub_model":
                    return new SubModelMovementSystem2D(_config, _sb);
                case "abstract":
                    return new AbstractNetMovementSystem(_config, _sb);
                default:
                    throw new Exception("Unknown movement system");
            }
        }

        public Common.Model.Agent.MovementSystem RegisterAgent(PulseAgent ag, UnitTravelingActivity unit)
        {
            if (ag == null) throw new Exception("Can't register null agent");

            Common.Model.Agent.MovementSystem ms;

            if (unit is VehicleActivity)
                ms = GetMovementSystem(MovementSystemTypes.VEHICLE);
            else if (unit is GuestAgentActivity)
                ms = GetMovementSystem(MovementSystemTypes.SUB_MODEL);
            else
                ms = GetMovementSystem(MovementSystemTypes.PEDESTRIAN);

            ms.AddAgent(ag, unit);

            return ms;
        }

        public void Update(double timeStep, double time)
        {
            foreach (var ms in _availableSystems.Values.SelectMany(ms => ms))
            {
                ms.Update(timeStep, time);
            }
        }

        public void SetCommand(ICommand command)
        {
            foreach(var ms in _availableSystems.Values.SelectMany(ms => ms))
                if (ms is SfMultilevelMovementSystem)
                {
                    ms.SetCommand(command);
                    break;
                }
        }

        protected override void LoadData()
        {
            foreach (var msRaw in _config.MovementSystems.Value)
            {
                var ms = GetImplementationByName(msRaw.Value);
                var msType = GetTypeByName(msRaw.Key);

                ms.Initialize();

                if (!_availableSystems.ContainsKey(msType))
                    _availableSystems[msType] = new List<Common.Model.Agent.MovementSystem>();

                _availableSystems[msType].Add(ms);
            }
        }
    }
}