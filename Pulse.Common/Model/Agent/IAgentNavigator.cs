using System.Collections.Generic;
using Pulse.Common.Pseudo3D;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{
    public interface IAgentNavigator
    {
        IList<PulseVector2> GeneratePath(PulseVector2 start, PulseVector2 end);
    }

    public interface IPseudo3DAgentNavigator
    {
        IList<ITravelPath> GeneratePath(PulseVector2 start, PulseVector2 end, int levelStart, int levelEnd);
        PulseVector2 Getvelocity(PulseVector2 pos, PulseVector2 dest, int level);
    }

    public enum Navigators
    {
        GRAPH,
        NAVFIELD
    }
}
