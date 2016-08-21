using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Scenery.Objects;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Loaders
{
    public class SimpleLevelsBuilder : AbstractDataBroker
    {
        public Dictionary<int, PulseLevel> Levels { get; set; }

        private PulseObject _pulseObject;
        private IDictionary<int, List<ILevelPortal>> _lPortals;
        private IDictionary<int, List<IExternalPortal>> _ePortals;

        public SimpleLevelsBuilder(PulseObject pulseObject, IDictionary<int, List<IExternalPortal>> ePortals, IDictionary<int, List<ILevelPortal>> lPortals)
        {
            _pulseObject = pulseObject;
            _ePortals = ePortals;
            _lPortals = lPortals;
        }
        
        protected override void LoadData()
        {
            var levels = new Dictionary<int, PulseLevel>();

            foreach (var level in _pulseObject.Levels)
            {
                var pulseLevel = new PulseLevel();
                
                pulseLevel.Floor = level.Key;

                // if last point != first point then polygon will be closed
                if (level.Value.Obstacles != null)
                    pulseLevel.Obstacles = level.Value.Obstacles.Select(o =>
                        o.Polygon.First().Equals(o.Polygon.Last())
                            ? o.Polygon.ToArray()
                            : o.Polygon.Concat(new[] {o.Polygon.First()}).ToArray()
                        ).ToList();
                else
                    pulseLevel.Obstacles = new List<PulseVector2[]>();

                pulseLevel.PointsOfInterest = level.Value.PointsOfInterest?.ToList() ?? new List<IPointOfInterest>();
                pulseLevel.Zones = level.Value.Zones?.ToList() ?? new List<Zone>();

                pulseLevel.ExternalPortals = _ePortals != null ? _ePortals[level.Key] : new List<IExternalPortal>();
                pulseLevel.LevelPortals = _lPortals != null ? _lPortals[level.Key] : new List<ILevelPortal>();

                pulseLevel.TypedPointsOfInterest = GetTypedPois(pulseLevel.PointsOfInterest, pulseLevel.ExternalPortals);

                levels.Add(pulseLevel.Floor, pulseLevel);
            }

            Levels = levels;
        }

        private IDictionary<PointOfInterestType, List<IPointOfInterest>> GetTypedPois(IEnumerable<IPointOfInterest> pois, IEnumerable<IExternalPortal> portals)
        {
            var typedPois = new Dictionary<PointOfInterestType, List<IPointOfInterest>>();

            foreach (var poi in pois)
            {
                foreach (var t in poi.Types)
                {
                    if (!typedPois.ContainsKey(t))
                        typedPois[t] = new List<IPointOfInterest>();
                    typedPois[t].Add(poi);
                }
            }

            foreach (var portal in portals)
            {
                foreach (var t in portal.Types)
                {
                    if (!typedPois.ContainsKey(t))
                        typedPois[t] = new List<IPointOfInterest>();
                    typedPois[t].Add(portal);
                }
            }

            return typedPois;
        }
    }
}