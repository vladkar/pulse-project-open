using System.Collections.Generic;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Behavior.Intention.Current
{
    public class PoiCurrentIntention : CurrentIntention, IPoiCurrentIntention, IMoveCurrentIntention
    {
        public IPointOfInterest Poi { set; get; }

        public PulseVector2 GoalPoint { set; get; }
        public int Level { get; set; }
        public IList<ITravelPath> Path { get; set; }
        public int CurrentSubPath { get; set; }

        public PoiCurrentIntention(IPoiPlannedIntension plannedIntention) : base(plannedIntention)
        {
        }
    }
}