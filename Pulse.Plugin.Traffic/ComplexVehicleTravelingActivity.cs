using Pulse.Common.Model.AgentScheduling.Current.Traveling;

namespace Pulse.Plugin.Traffic
{
    public class ComplexVehicleTravelingActivity : ComplexTravelingActivity
    {
        public Pulse.MultiagentEngine.Map.PulseVector2 VehiclePosition { set; get; }
    }
}