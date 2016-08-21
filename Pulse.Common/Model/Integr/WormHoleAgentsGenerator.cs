using System;
using System.Collections.Generic;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;

namespace Pulse.Common.Model.Integr
{
    public class WormHoleAgentsGenerator : AbstractExternalPortalAgentGenerator
    {

        protected IList<IGuestAgent> _guests;
        protected IList<IGuestAgent> _travelers;
        protected Func<AbstractPulseAgent, IGuestAgent> _teleportFunction;

        public WormHoleAgentsGenerator(IExternalPortal ep, Func<AbstractPulseAgent, IGuestAgent> teleportFunction) : base(ep)
        {
            _guests = new List<IGuestAgent>();
            _travelers = new List<IGuestAgent>();
            _teleportFunction = teleportFunction;
        }

        public void DropGuest(IGuestAgent guest)
        {
            _guests.Add(guest);
        }

        public override void Update(double timeStep, double time, DateTime geotime)
        {
            _guests = new List<IGuestAgent>();
        }

        public IList<IGuestAgent> GetTravelers()
        {
            var t = _travelers;
            _travelers = new List<IGuestAgent>();
            return t;
        }

        public void TeleportAgent(AbstractPulseAgent agent)
        {
//            _travelers.Add(new SimpleGuestAgent { Id = Int32.Parse(agent.Id), X = agent.Point.X, Y = agent.Point.Y, DestWorld = (byte) (RandomUtil.RandomDouble() > 0.5 ? 1 : 0)});
            _travelers.Add(_teleportFunction(agent));
        }
    }
}