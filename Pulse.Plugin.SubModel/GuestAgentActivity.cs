using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;

namespace Pulse.Plugin.SubModel
{
    public class GuestAgentActivity : UnitTravelingActivity
    {
        public string OriginWorldName { set; get; }
        public long OriginAgentId { set; get; }
    }
}
