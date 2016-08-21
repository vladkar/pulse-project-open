using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Legend;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Pseudo3D.Graph;
using Pulse.Common.Scenery.Loaders;

namespace Pulse.Common.Model.Environment
{
    public class PulseScenery : AbstractDataBroker, ILegendable
    {
        public IDictionary<int, PulseLevel> Levels { set; get; }
        public RoadGraphPseudo3D<VertexDataPseudo3D, EdgeDataPseudo3D> Graph { set; get; }
        public Navigators Navigator { set; get; }

        private IDictionary<PointOfInterestType, List<IPointOfInterest>> _typedPointsOfInterest = null;
        //private object _lock = new object();
        public IDictionary<PointOfInterestType, List<IPointOfInterest>> TypedPointsOfInterest
        {
            get
            {
                if (_typedPointsOfInterest == null)
                    _typedPointsOfInterest = GetTypedPointsOfInterest();

                return _typedPointsOfInterest;
            }
        }

        private IDictionary<PointOfInterestType, List<IPointOfInterest>> GetTypedPointsOfInterest()
        {
            var typedPois = new Dictionary<PointOfInterestType, List<IPointOfInterest>>();
            foreach (var level in Levels)
            {
                foreach (var typedLevelPois in level.Value.TypedPointsOfInterest)
                {
                    if (!typedPois.ContainsKey(typedLevelPois.Key))
                        typedPois.Add(typedLevelPois.Key, new List<IPointOfInterest>());
                    typedPois[typedLevelPois.Key].AddRange(typedLevelPois.Value);
                }
            }
            return typedPois;
        }

        public PointOfInterestType GetPoiTypeByName(string name)
        {
            return Levels.Values.Select(level => level.TypedPointsOfInterest.Keys.FirstOrDefault(x => x.Name == name)).FirstOrDefault(poit => poit != null);
        }

        public virtual IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>>
            {
                {
                    "PointOfInterestTypes", TypedPointsOfInterest.Select(t => new LegendElement
                    {
                        NiceName = t.Key.Name,
                        Id = t.Key.Id,
                        Name = t.Key.Name
                    }).ToList()
                }
            };
        }

        protected override void LoadData()
        {
        }
    }
}