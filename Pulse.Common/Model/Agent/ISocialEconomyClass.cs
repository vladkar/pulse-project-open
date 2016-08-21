using System;
using System.Collections.Generic;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.Agent
{
    public interface ISocialEconomyClass : IEquatable<ISocialEconomyClass>
    {
        int Id { get; set; }
        string Name { set; get; }
        IList<PointOfInterestType> HomeTypes { set; get; }
        int PercentOfPopulation { set; get; }
        IList<AbstractPoiActivity> AbstractClassActivities { set; get; }
        IList<AbstractClassSchedule> AbstractClassSchedules { set; get; }

        string ToString();
        int GetHashCode();
    }
}
