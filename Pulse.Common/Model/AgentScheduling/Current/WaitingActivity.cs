using System;

namespace Pulse.Common.Model.AgentScheduling.Current
{
    public class WaitingActivity : CurrentActivity
    {
        public DateTime PlannedEndTime { get; set; }
    }

    public class HomeActivity : WaitingActivity
    {
    }
}