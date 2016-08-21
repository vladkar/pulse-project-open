using System;
using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.PluginSystem.Interface;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Environment.World
{
    public interface IWorldKnowledge : IPluginable
    {
        IList<PointOfInterestType> PossiBuildingTypes { get; }
        DateTime Clock { get; }
        double MetersPerMapUnit { get; }

        GeoCoords ConvertMapCoords(PulseVector2 coords);
        PulseVector2 ConvertGeoCoords(GeoCoords geoCoords);

        IPointOfInterest GetTypedBuildingInRadius(IList<PointOfInterestType> possibleBuildingTypes, PulseVector2 mapPoint, double radius);
        IEnumerable<IPointOfInterest> GetTypedBuildingsInRadius(IList<PointOfInterestType> possibleBuildingTypes, PulseVector2 mapPoint, double radius);
        IPointOfInterest GetTypedBuildingInRadius(string type, PulseVector2 mapPoint, double d);
        IPointOfInterest GetClosestPointOfInterest(PointOfInterestType poiType, PulseVector2 mapPoint);
        IPointOfInterest GetClosestPointOfInterest(string poiType, PulseVector2 mapPoint);
        IEnumerable<IPointOfInterest> GetTypedBuildings(string type);
    }

//    public interface IPluginableWorldKnowledge : IWorldKnowledge, IPluginable
//    {
//        IPluginableBuilding GetTypedBuildingInRadius(IList<IBuildingType> possibleBuildingTypes, Coords mapPoint, double radius);
//    }
}
