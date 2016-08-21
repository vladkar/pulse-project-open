using System;

namespace Pulse.Common.Model.Environment.Poi
{
    public class PointOfInterestType
    {
        public string Name { set; get; }
        public int Id { set; get; }

        public override string ToString()
        {
            return String.Format("{0}: {1}", Id, Name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PointOfInterestType);
        }

        public bool Equals(PointOfInterestType obj)
        {
            return obj != null && obj.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
