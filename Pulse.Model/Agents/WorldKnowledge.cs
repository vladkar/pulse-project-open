using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Environment;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.PluginSystem.Spawn;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Model.Agents
{
    public class WorldKnowledge : IWorldKnowledge
    {
        private readonly PulseScenery _sb;
        private readonly GeoWorldGeneralInfo _gwi;
        private readonly GeoCartesUtil _gcu;

        public WorldKnowledge(PulseScenery sb, GeoWorldGeneralInfo gwi, GeoCartesUtil gcu)
        {
            _sb = sb;
            _gwi = gwi;
            _gcu = gcu;
        }

        public IList<PointOfInterestType> PossiBuildingTypes
        {
            get { return _sb.TypedPointsOfInterest.Keys.ToList(); }
        }

//        public GeoMapData AskForMapData()
//        {
//            return _md;
//        }

        public DateTime Clock
        {
            get { return _gwi.GeoTime.GeoTime; }
        }

        public double MetersPerMapUnit
        {
            get { return _gwi.MetersPerMapUnit; }
        }

        public GeoCoords ConvertMapCoords(PulseVector2 coords)
        {
            return _gcu.GetGeoCoords(coords);
        }

        public PulseVector2 ConvertGeoCoords(GeoCoords geoCoords)
        {
            return _gcu.GetCoordsTuple(geoCoords);
        }

        public IPointOfInterest GetClosestPointOfInterest(string poiType, PulseVector2 mapPoint)
        {
            return GetClosestPointOfInterest(_sb.GetPoiTypeByName(poiType), mapPoint);
        }

        public IPointOfInterest GetClosestPointOfInterest(PointOfInterestType poiType, PulseVector2 mapPoint)
        {
            var min = _sb.TypedPointsOfInterest[poiType].First(p => p.NavgationBlock != null);
            var minDist = min.TravelPoint.DistanceSquared(mapPoint);
            foreach (var b in _sb.TypedPointsOfInterest[poiType])
            {
                //TODO b.NavgationBlock != null  - hack and bad
                if (b.NavgationBlock == null) continue;

                var currentDist = mapPoint.DistanceSquared(b.TravelPoint);
                
                if (currentDist < minDist)
                {
                    minDist = currentDist;
                    min = b;
                }
            }

            return min;
        }

        public IPointOfInterest GetTypedBuildingInRadius(string type, PulseVector2 mapPoint, double d)
        {
            var ptype = _sb.GetPoiTypeByName(type);
            var pois = _sb.Levels.SelectMany(l => l.Value.TypedPointsOfInterest[ptype]);

            return pois.FirstOrDefault(poi => poi.TravelPoint.DistanceSquared(mapPoint) < d*d);

//            return GetTypedBuildingInRadius(new[] {_sb.GetPoiTypeByName(type)}, mapPoint, d);
        }

        public IEnumerable<IPointOfInterest> GetTypedBuildings(string type)
        {
            return _sb.TypedPointsOfInterest[_sb.GetPoiTypeByName(type)];
        }

        public IPointOfInterest GetTypedBuildingInRadius(IList<PointOfInterestType> possibleBuildingTypes, PulseVector2 mapPoint, double d)
        {
            // 1. Weighted type choise
            var weightedTypes = new Dictionary<PointOfInterestType, int>();
            foreach (var type in possibleBuildingTypes.Where(type => _sb.TypedPointsOfInterest.ContainsKey(type)))
            {
                weightedTypes[type] = _sb.TypedPointsOfInterest[type].Count;
            }


            //var mapDistance = d / MetersPerMapUnit;
            // For all weighted types
            // var iteratedTypes = new List<WikimapiaBuildingType>();
            
            //random choise
            for (var i = 0; i < weightedTypes.Count; i++)
            {
                var selectedType = weightedTypes.ProportionChoise(e => e.Value).Key;

                //TODO something very old and strange
                // Tries count = len/2 -> changed to count = len
                for (var j = 0; j < _sb.TypedPointsOfInterest[selectedType].Count; j++)
                {
                    var tempGoal = _sb.TypedPointsOfInterest[selectedType].RandomChoise();
                    if (tempGoal.TravelPoint.DistanceTo(mapPoint) <= d)
                        return tempGoal;
                }
            }

            // 2. if random == bad then brute
            foreach (var type in weightedTypes.Keys)
                foreach (var b in _sb.TypedPointsOfInterest[type])
                {
                    if (b.TravelPoint.DistanceTo(mapPoint) <= d)
                        return b;
                }

            return null;
            throw new Exception("No typed building in radius");
        }

        public IEnumerable<IPointOfInterest> GetTypedBuildingsInRadius(IList<PointOfInterestType> possibleBuildingTypes, PulseVector2 mapPoint, double d)
        {
            var pois = new List<IPointOfInterest>();

            foreach (var type in possibleBuildingTypes)
                foreach (var b in _sb.TypedPointsOfInterest[type].Where(p => p.NavgationBlock != null))
                    if (b.TravelPoint.DistanceTo(mapPoint) <= d)
                        pois.Add(b);

            return pois;
        }

        public PluginsContainer PluginsContainer { get; set; }
    }
}
