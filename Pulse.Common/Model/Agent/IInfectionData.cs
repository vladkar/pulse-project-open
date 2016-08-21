using System;
using System.Collections.Generic;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.ExternalAPI;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{
    public interface IInfectionData : IPackableData
    {

    }

    public class InfectionData : IInfectionData
    {
        public IList<InfectionContactData> Contacts { set; get; }

    }

    public struct InfectionContactData
    {
        public long Infector { set; get; }
        public long Infected { set; get; }
        public PulseVector2 Point { set; get; }
        public GeoCoords GeoPoint { set; get; }
        public DateTime InfectionTime { set; get; }
        public double ContactDurationSeconds { set; get; }
        public string ContactType { set; get; }
        public bool Success { set; get; }
        public int Level { get; set; }
    }
}
