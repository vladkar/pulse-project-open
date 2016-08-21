using System.Collections.Generic;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Behavior.Intention.Current
{
    public interface IMoveCurrentIntention : ICurrentIntention
    {
        PulseVector2 GoalPoint { set; get; }
        int Level { set; get; }
        IList<ITravelPath> Path { set; get; }
        int CurrentSubPath { set; get; }
    }
}