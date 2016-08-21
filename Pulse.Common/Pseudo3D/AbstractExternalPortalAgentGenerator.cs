using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;

namespace Pulse.Common.Pseudo3D
{
    public abstract class AbstractExternalPortalAgentGenerator : IComplexUpdatable
    {
        protected IPulseMap _map;
        protected IPopulationManager _pm;
        protected IExternalPortal _portal;

        protected AbstractExternalPortalAgentGenerator(IExternalPortal p)
        {
            _portal = p;
        }

        public void ApplyEnvironment(IPopulationManager pm, IPulseMap map)
        {
            _pm = pm;
            _map = map;
        }

        public abstract void Update(double timeStep, double time, DateTime geotime);
    }
}