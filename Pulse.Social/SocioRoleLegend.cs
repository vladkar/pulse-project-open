using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Legend;

namespace Pulse.Social
{
    public class SocioRoleLegend : ILegendable
    {
        private ISet<ISocialEconomyClass> _socioClasses;

        public SocioRoleLegend(ISet<ISocialEconomyClass> socioClasses)
        {
            _socioClasses = socioClasses;
        }

        public IDictionary<string, IList<LegendElement>> GetLegend()
        {
            return new Dictionary<string, IList<LegendElement>> { {"Roles", GetRoles() } };
        }

        private IList<LegendElement> GetRoles()
        {
            return _socioClasses.Select(cl => new LegendElement
            {
                NiceName = cl.Name,
                Id = cl.Id,
                Name = cl.Name
            }).ToList();
        }
    }
}
