using System.Collections.Generic;
using Pulse.Common.Model;
using Pulse.Common.Model.Agent;

namespace Pulse.Social.Population
{
    public class PhysicalCapabilityClass : IPhysicalCapabilityClass
    {
        public int Id { set; get; }
        public string Name { get; set; }
        public IList<int> Agility { set; get; }
        public IList<int> Control { set; get; }
        public IList<int> Information { set; get; }
        public IList<int> Marital { set; get; }
        public IList<int> Passability { set; get; }
        public IList<int> Role { set; get; }
        public IList<int> Stamina { set; get; }
        public IList<int> Strength { set; get; }
        public double Speed { set; get; }

        public bool Equals(IPhysicalCapabilityClass other)
        {
            return Name == other.Name;
        }

        public override string ToString()
        {
            return Name;
        }
        
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
