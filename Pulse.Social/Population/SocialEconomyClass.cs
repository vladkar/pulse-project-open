using System.Collections.Generic;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Social.Population
{
    public class SocialEconomyClass : ISocialEconomyClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PercentOfPopulation { set; get; }
        public IList<PointOfInterestType> HomeTypes { set; get; }
        public IList<AbstractPoiActivity> AbstractClassActivities { set; get; }
        public IList<AbstractClassSchedule> AbstractClassSchedules { set; get; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(ISocialEconomyClass other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
