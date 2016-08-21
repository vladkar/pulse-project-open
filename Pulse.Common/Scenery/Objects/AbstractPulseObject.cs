using System.Collections.Generic;
using Pulse.Common.Model;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Scenery.Objects
{
    public abstract class AbstractPulseObject : IUniqueObject
    {
        public string ObjectId { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }
    }

    public abstract class AbstractPolygonPulseObject : AbstractPulseObject
    {
        public IList<PulseVector2> Polygon { set; get; }
    }
}
