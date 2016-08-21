using System;
using Pulse.Common;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Map;
using Pulse.Plugin.SimpleInfection.Body;

namespace Pulse.Plugin.SimpleInfection.Infection
{
    public class InfectionContact
    {
        public SimpleInfectionPluginAgent Infector { set; get; }
        public SimpleInfectionPluginAgent Infected { set; get; }
        public PulseVector2 MapPoint { set; get; }
        public GeoCoords GeoPoint { set; get; }
        public DateTime InfectionTime { set; get; }
        public TimeSpan ContactDuration { set; get; }
        public int Level { set; get; }

        //TODO think
        public string ContactType { set; get; }
        public bool Success { set; get; }

    }

    public interface IInfectionContact
    {
        
    }
}