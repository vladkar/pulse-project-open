using System.Collections.Generic;

namespace VirtualSociety.Model.Tree
{
    public class BuildingClusterQuadrant : PolygonalQuadrant
    {
        public ISet<VsBuilding> Buildings { get; set; }

        public BuildingClusterQuadrant() : base()
        {
            Buildings = new HashSet<VsBuilding>();
        }
    }
}
