using System.Collections.Generic;
using Pulse.Common.Model.AgentScheduling.Current.Traveling;
using Pulse.Common.Pseudo3D;

namespace Pulse.Plugin.Traffic
{
    public class VehicleActivity : UnitTravelingActivity, ITravelPath
    {
        public Pulse.MultiagentEngine.Map.PulseVector2 VehicleOrigin { set; get; }
        public Pulse.MultiagentEngine.Map.PulseVector2 VehicleDestination { set; get; }
        public Pulse.MultiagentEngine.Map.PulseVector2 TotalDestination { set; get; }

        public VehicleActivity() { }

        public VehicleActivity(object unit)
            : this()
        {
        }

        public bool Finished()
        {
            throw new System.NotImplementedException();
        }
    }
}