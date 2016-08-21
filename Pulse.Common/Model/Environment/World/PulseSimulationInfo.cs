using Pulse.MultiagentEngine.Map;
using Pulse.MultiagentEngine.Stats;

namespace Pulse.Common.Model.Environment.World
{
    public class PulseSimulationInfo : SimulationInfo
    {
        public double ConfigurationToMetersMultiplier;
        public PulseVector2 ConfigurationDL;
        public PulseVector2 ConfigurationUR;
        public GeoCoords ConfidurationGeoDL;
    }
}
