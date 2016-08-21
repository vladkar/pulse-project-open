using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Scenery.Loaders
{
    public abstract class AbstractExternalPortalBuilder : AbstractDataBroker
    {
        public IDictionary<int, List<IExternalPortal>> Portals { get; set; }

        public void ApplyEnvironment(IPopulationManager pm, IPulseMap map)
        {
            Portals.Values.SelectMany(p => p).Where(p => p.AgentGenerator != null).ToList().ForEach(p => p.AgentGenerator.ApplyEnvironment(pm, map));
        }

        protected abstract override void LoadData();

        protected IExternalPortal MakeEmptyPortal(ILevelPortal p)
        {
            return new ExternalPortal
            {
                ObjectId = p.ObjectId,
                Name = p.Name,
                Point = p.Point,
                Polygon = p.Polygon,
                Level = p.Level,
                Enterable = p.Enterable,
                Exitable = p.Exitable,
                Zone = p.Zone
            };
        }
    }
}
