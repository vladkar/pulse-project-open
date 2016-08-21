using System.Collections.Generic;
using System.Linq;

namespace Pulse.Common.Model.Legend
{
    public class LegendAggregator : ILegendable
    {
        public IList<ILegendable> Legendables { set; get; }

        public LegendAggregator()
        {
            Legendables = new List<ILegendable>();
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return Legendables.SelectMany(l => l.GetLegend()).ToDictionary(p => p.Key, p => p.Value);
        }
    }
}
