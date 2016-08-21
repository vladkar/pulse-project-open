using System;
using Pulse.Common.PluginSystem.Base;

namespace Pulse.Plugin.SimpleInfection.Body
{
    public class SimpleInfectionPluginBuilding : PluginBaseBuilding
    {
        public bool IsInfected { set; get; }
        public DateTime LastInfectionContact { set; get; }
        public SimpleInfectionPluginAgent LastInfector { set; get; }

        public SimpleInfectionPluginBuilding()
        {
            Name = GlobalStrings.PluginName;
        }
    }
}
