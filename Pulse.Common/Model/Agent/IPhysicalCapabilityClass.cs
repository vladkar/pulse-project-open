using System;
using System.Collections.Generic;

namespace Pulse.Common.Model.Agent
{
    public interface IPhysicalCapabilityClass : IEquatable<IPhysicalCapabilityClass>
    {
        int Id { set; get; }
        string Name { set; get; }
        IList<int> Agility { set; get; }
        IList<int> Control { set; get; }
        IList<int> Information { set; get; }
        IList<int> Marital { set; get; }
        IList<int> Passability { set; get; }
        IList<int> Role { set; get; }
        IList<int> Stamina { set; get; }
        IList<int> Strength { set; get; }
        double Speed { set; get; }

        string ToString();
        int GetHashCode();
    }
}
