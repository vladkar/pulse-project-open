using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.AgentScheduling.Current.Traveling
{
    public class ComplexTravelingActivity : CurrentActivity, ITravelPath
    {
        public IList<ITravelPath> Path { get; set; }
        //public IPointOfInterest PointOfInterest { get; set; }
        public PulseVector2 Point { get; set; }
        public int Level { get; set; }
        public bool Finished()
        {
            throw new System.NotImplementedException();
        }
    }
}