using System;
using System.Collections.Generic;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.BehaviorTreeFramework.Parents;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Behavior.Pulse;
using Pulse.Common.Behavior.Pulse.Bt;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageSimpleRole : DecisionTreeRole
    {
        public VkfestStageSimpleRole(AbstractPulseAgent agent) : base
            (
                agent, 
                new SimpleScheduleRoleConfig {CornerCutOffRadius = 0.2, PoiInteractionRadius = 0.5, DeadZoneRadius = 0.3}, 
                "VkFest Dummy Agent Role", 
                141
            )
        {
            //
        }


        public override void PrepareTree()
        {
            if (_agent.MovementSystem == null)
                _agent.SetMovementSystem(null);
//            _agent.MovementSystem.SetAgentMaxSpeed(_agent, _agent.PhysicalCapabilityClass.Speed/50);


            Data = new TestData();

            var ad = new DecisionTreeData
            {
                World = _agent.World as PulseWorld,
                Agent = _agent,
                Random = new Random()
            };
          

            var getDesiredPositionRandomConstrained = new ActionBtNode<DecisionTreeData>(() =>
            {
                var d = 5;
                
                if (_agent.Point.DistanceTo(_agent.DesiredPosition) < Config.PoiInteractionRadius || _agent.DesiredPosition.X == 0)
                {
                    PulseVector2 p = new PulseVector2();
                    var f = true;
                    while (f)
                    {
                        p = ClipperUtil.GetRandomPointOnPolygon(_agent.Home.Polygon);
                        if (p.DistanceTo(_agent.Point) <= d)
                            f = false;
                    }

                    _agent.DesiredPosition = p;
                }

            }, ad);

            var getDesiredPositionStay = new ActionBtNode<DecisionTreeData>(() =>
            {
                    _agent.DesiredPosition = _agent.Point;

            }, ad);

            var root = new SequenceBtNode();
            root.Children.Add(getDesiredPositionStay);
            TestBehTree = root;
        }

        public Queue<IPlannedIntention> Plan { get; set; }
    }
}