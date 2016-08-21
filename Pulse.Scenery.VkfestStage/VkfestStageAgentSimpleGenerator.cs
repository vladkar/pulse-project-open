using System;
using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageAgentSimpleGenerator : AbstractExternalPortalAgentGenerator
    {
        protected bool _isLoaded = false;
        protected int _count;

        public VkfestStageAgentSimpleGenerator(IExternalPortal externalPortal, int count) : base(externalPortal)
        {
            _count = count;
        }



        public override void Update(double timeStep, double time, DateTime geotime)
        {
            if (_isLoaded) return;

            for (int i = 0; i < _count; i++)
            {
                CreateAgent(GetNextPoint());
            }

            _isLoaded = true;
        }

        private PulseVector2 GetNextPoint()
        {
            return ClipperUtil.GetRandomPointOnPolygon(_portal.Polygon);
        }


        protected AbstractPulseAgent CreateAgent(PulseVector2 pos)
        {
            var agent = _pm.CreateAgent();

            agent.Home = _portal;
            agent.Level = _portal.Level;
            agent.Move(pos);

            var arrivedShipPasRole = new VkfestStageSimpleRole(agent);
            agent.Role = arrivedShipPasRole;

            _map.World.AddNewAgent(agent);
            agent.Role.Initialize();

            return agent;
        }
    }
}