using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Objects;
using Pulse.Common.Utils;

namespace Pulse.Common.Scenery.Loaders
{
    public class SimplePortalBuilder : AbstractExternalPortalBuilder
    {
        private Dictionary<string, PointOfInterestType> _portalTypes;
        private PulseObject _pulseObject;
        private string _simType;
        private int _agentCount;
        private IDictionary<string, Func<ILevelPortal, IExternalPortal>> _portalBuilderFuncs;

        public SimplePortalBuilder(PulseObject pulseObject,
            IDictionary<string, Func<ILevelPortal, IExternalPortal>> portalBuilderFuncs)
        {
            _pulseObject = pulseObject;
            _portalTypes = new Dictionary<string, PointOfInterestType>();
            _portalBuilderFuncs = portalBuilderFuncs;
            Name = "Simple portal manager";
        }

        protected override void LoadData()
        {
            _portalTypes = _portalBuilderFuncs.ToDictionary(
                b => b.Key,
                b => new PointOfInterestType {Id = IdUtil.NextRandomId(), Name = b.Key});


            var portals = new Dictionary<int, List<IExternalPortal>>();

            foreach (var ptype in _portalTypes.Keys)
            {
                foreach (var level in _pulseObject.Levels.Values)
                {
                    var levelPortals = level.Portals.Where(p => p.TargetLink == ptype).Select(p =>
                    {
                        var ep = _portalBuilderFuncs[ptype](p);
                        ep.Types.Add(_portalTypes[ptype]);
                        return ep;
                    }).ToList();

                    if (!portals.ContainsKey(level.LevelFloor)) portals[level.LevelFloor] = new List<IExternalPortal>();
                    portals[level.LevelFloor].AddRange(levelPortals);
                }
            }

            Portals = portals;
        }
    }
}