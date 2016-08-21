using Pulse.Common.Model;
using Pulse.Common.Model.Environment.Map;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Objects
{
    public class StaffPoint : AbstractPulseObject, IZonable
    {
        public PulseVector2 Point { set; get; }
        public int Level { set; get; }
        public Zone Zone { get; set; }
    }
}