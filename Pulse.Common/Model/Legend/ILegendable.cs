using System.Collections.Generic;

namespace Pulse.Common.Model.Legend
{
    public interface ILegendable
    {
        IDictionary<string, IList<LegendElement>> GetLegend();
    }
    
    public class LegendElement
    {
        public string Name { set; get; }
        public string NiceName { set; get; }
        public int Id { set; get; }
    }
}