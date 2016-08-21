using System.Collections.Generic;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Pseudo3D
{
    public class SimpleTravelPath : ITravelPath
    {
        public IList<PulseVector2> SimplePath { set; get; }
        public int LastDoneCheckpoint { set; get; }


        public bool Finished()
        {
            return LastDoneCheckpoint >= SimplePath.Count;
        }
    }

    public class NavFieldTravelPath : ITravelPath
    {
        public bool Finished()
        {
            return false;
        }
    }
}