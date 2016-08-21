using System.Collections.Generic;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.AgentScheduling.Current.Traveling
{
    public class SimplePathTravelingActivity : UnitTravelingActivity
    {
        public IList<PulseVector2> SimplePath { set; get; }
        public int LastDoneCheckpoint { set; get; }

        public SimplePathTravelingActivity() { }

        public SimplePathTravelingActivity(SimpleTravelPath unit) : this()
        {
            SimplePath = unit.SimplePath;
        }
    }
}