using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;
using Pulse.Social.Population;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageAgentProbeGenerator : AbstractExternalPortalAgentGenerator
    {
        protected bool _isLoaded = false;
//        protected int _count;

        public VkfestStageAgentProbeGenerator(IExternalPortal externalPortal) : base(externalPortal)
        {
//            _count = count;
        }



        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (_isLoaded) return;

//            for (int i = 0; i < _count; i++)
//            {
                CreateAgent(GetNextPoint());
//            }

            _isLoaded = true;
        }

        private PulseVector2 GetNextPoint()
        {
            return ClipperUtil.GetCentroid(_portal.Polygon);
        }


        protected AbstractPulseAgent CreateAgent(PulseVector2 pos)
        {
            var agent = _pm.CreateAgent();

            agent.PhysicalCapabilityClass = new PhysicalCapabilityClass {Name = "Probe", Speed = 3};

            agent.Home = _portal;
            agent.Level = _portal.Level;
            agent.Move(pos);

            var arrivedShipPasRole = new VkfestStageProbeRole(agent);
            agent.Role = arrivedShipPasRole;

            _map.World.AddNewAgent(agent);
            agent.Role.Initialize();

            return agent;
        }
    }
}