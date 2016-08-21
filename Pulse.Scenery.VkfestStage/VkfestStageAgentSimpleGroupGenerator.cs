using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Behavior.Pulse;
using Pulse.Common.Model.Agent;
using Pulse.Common.Pseudo3D;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageAgentSimpleGroupGenerator : SimpleIterationProbabilityAgentsGroupGenerator
    {

        public VkfestStageAgentSimpleGroupGenerator(IExternalPortal externalPortal, double p, double count = -1) : base(externalPortal, 2, 2, p, count)
        {
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

        
        

        protected override void CreateGroup(int grsize)
        {
            var formation = new LeaderFormation { Agents = new List<AbstractPulseAgent>() };
            var group = new MovingAgentGroup(formation) { Agents = new List<AbstractPulseAgent>() };

            for (int i = 0; i < grsize; i++)
            {
                var agent = _pm.CreateAgent();

                agent.Home = _portal;
                agent.Level = _portal.Level;
//                agent.Move(ClipperUtil.GetRandomPointOnPolygon(_portal.Polygon));
                agent.Move(GetNextPoint());

                var arrivedShipPasRole = new VkfestStageGroupProbeRole(agent);

                arrivedShipPasRole.Group = group;
                if (i == 0)
                {
                    formation.Leader = agent;
                    formation.Leader.OnKill += reason => { formation.Agents.Where(a => a != formation.Leader).ToList().ForEach(a => a.Kill("dead leader")); };
                }
                group.Agents.Add(agent);
                formation.Agents.Add(agent);

                agent.Role = arrivedShipPasRole;

                _map.World.AddNewAgent(agent);
                agent.Role.Initialize();
            }

            var minGroupSpeed = group.Agents.Min(a => a.PhysicalCapabilityClass.Speed);

            foreach (var agent in group.Agents)
            {
                agent.PhysicalCapabilityClass.Speed = minGroupSpeed;
            }
        }
    }
}