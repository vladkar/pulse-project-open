using System.Collections.Generic;
using NavigField;

namespace Pulse.Common.Scenery.Objects
{
    public class PulseObject : AbstractPulseObject
    {
        public IDictionary<int, Level> Levels { set; get; }
        public string Version { set; get; }
        public IDictionary<string, INavfieldCalc> Navfields { set; get; }
    }
}
