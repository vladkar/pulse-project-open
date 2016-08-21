using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.AgentScheduling.Abstract
{
    public class AbstractPoiActivity : AbstractAgentActivity
    {
        public IList<PointOfInterestType> PossiblePoiTypes { set; get; }
    }
}