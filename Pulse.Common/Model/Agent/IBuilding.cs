using System.Collections.Generic;
using Pulse.Common.Model.Environment.Poi;

namespace Pulse.Common.Model.Agent
{
    public interface IBuilding : IPointOfInterest, IEnterable
    {
        int PoepleAmount { set; get; }
        ICollection<AbstractPulseAgent> Housemates { set; get; } 
    }
}