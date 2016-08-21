using System;
using System.Collections.Generic;
using System.Linq;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.BehaviorTreeFramework.Parents;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Behavior.Pulse;
using Pulse.Common.Behavior.Pulse.Bt;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.Environment.Map;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.Common.Utils;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Scenery.VkfestStage
{
    public class VkfestStageProbeRole : DecisionTreeRole
    {
        public VkfestStageProbeRole(AbstractPulseAgent agent) : base
            (
                agent, 
                new SimpleScheduleRoleConfig {CornerCutOffRadius = 0.2, PoiInteractionRadius = 0.5, DeadZoneRadius = 0.3}, 
                "VkFest Probe Agent Role", 
                140
            )
        {
            //
        }




        public override void PrepareTree()
        {
            Data = new TestData();

            var ad = new DecisionTreeData
            {
                World = _agent.World as PulseWorld,
                Agent = _agent,
                Random = new Random()
            };

            if (_agent.MovementSystem == null)
                _agent.SetMovementSystem(null);
            //            _agent.MovementSystem.SetAgentMaxSpeed(_agent, _agent.PhysicalCapabilityClass.Speed / 50);
            (_agent.MovementSystem as ISFMovementSystem).SetAgentForce(_agent, 200);

            //input
            var a1prob = 1d;

            // current goal
            // todo think where it should be stored
            var goalPoint = new PulseVector2(0, 0);
            var goalLevel = 1;
            // poi selection
            IPointOfInterest poi = null;

            #region behavior

            // should i do it?
            //            var isActionPerformed = new FuncConditionBehaviorTreeNode(new Func<bool>((() => r.NextDouble() > a1prob)), null);
            var isActionPerformed = new ProbabilityCondition(ad);

//            var selectPoi = new PoiSelectTask(ad);
            var selectPoi = new FuncBtNode<DecisionTreeData>(() =>
            {
                var plannedIntention = _agent.Role.Data.PlannedIntention as IPoiPlannedIntension;
                if (plannedIntention == null) return BehaviorState.Failed;

                var type = (ad.World.Map as IPulseMap).Scenery.GetPoiTypeByName(plannedIntention.PoiTypes.First());
                var typedPois = (ad.World.Map as IPulseMap).Scenery.TypedPointsOfInterest[type].Where(p => p.NavgationBlock != null);

                var targetPoi = typedPois.First(tp => _agent.Home.Point.Y < tp.Polygon.Max(p => p.Y) & _agent.Home.Point.Y > tp.Polygon.Min(p => p.Y));

                var currentIntention = new PoiCurrentIntention(plannedIntention);
                currentIntention.Poi = targetPoi;
                currentIntention.GoalPoint = currentIntention.Poi.GetTravelPoint(ad.Agent, ad.Random);
                currentIntention.Level = currentIntention.Poi.Level;
                ad.Agent.Role.Data.CurrentIntention = currentIntention;

                _agent.DesiredPosition = ClipperUtil.GetCentroid(targetPoi.Polygon);

                return BehaviorState.Success;

            }, ad);


            var isIntentionOk = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                return ad.Agent.Role.Data.CurrentIntention is IMoveCurrentIntention;
            }, ad);

            var isGoalReached = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                var mi = _agent.Role.Data.CurrentIntention as IMoveCurrentIntention;

                return _agent.Point.DistanceTo(mi.GoalPoint) < Config.PoiInteractionRadius
                       & _agent.Level == mi.Level;
            }, ad);

            var isPathOk = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                return (ad.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention)?.Path != null;
            }, ad);

            var calculatePath = new CalculatePathTask(ad);
            //            var calculatePath = new CalculatePathParallelExperimentalTask(ad);

            var simpleSubpathTask = new SimpleGetDesiredPointTask(ad);

            var simplePoiInteract = new ActionBtNode<DecisionTreeData>(() =>
            {
                var mi = _agent.Role.Data.CurrentIntention as IMoveCurrentIntention;

                _agent.Role.Data.CurrentIntention = null;
                _agent.Role.Data.PlannedIntention = null;
                simpleSubpathTask.State = BehaviorState.Ready;

                if (mi.PlannedIntention.Name == "exit")
                    _agent.Kill("exit");

            }, ad);

            var die = new ActionBtNode<DecisionTreeData>(() => { _agent.Kill("tree"); }, ad);

            var isSpecialPoiMoverequired = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                var plannedIntention = _agent.Role.Data.CurrentIntention as IPoiCurrentIntention;
                if (plannedIntention == null) return false;

                return ClipperUtil.IsPointInPolygon(_agent.Point, plannedIntention.Poi.Polygon);
            }, ad);

            var simplePoiMoveAction = new ActionBtNode<DecisionTreeData>(() =>
            {
                var plannedIntention = _agent.Role.Data.CurrentIntention as IMoveCurrentIntention;
                if (plannedIntention == null) return;

                _agent.DesiredPosition = plannedIntention.GoalPoint;
            }, ad);

            var poiBehavior = new SequenceBtNode();
            var setIntention = new SelectorBtNode();
            var navigate = new SelectorBtNode();
            var path = new SelectorBtNode();
            var preMove = new SequenceBtNode();
            var specialPoiMove = new SequenceBtNode();

            poiBehavior.Children.Add(setIntention);
            poiBehavior.Children.Add(navigate);
            poiBehavior.Children.Add(simplePoiInteract);

            setIntention.Children.Add(isIntentionOk);
            setIntention.Children.Add(selectPoi);

            navigate.Children.Add(isGoalReached);
            navigate.Children.Add(specialPoiMove);
            navigate.Children.Add(preMove);

            specialPoiMove.Children.Add(isSpecialPoiMoverequired);
            specialPoiMove.Children.Add(simplePoiMoveAction);

            preMove.Children.Add(path);

            path.Children.Add(isPathOk);
            path.Children.Add(calculatePath);

            #endregion

            #region move

            var getDesiredPositionSimple = new ActionBtNode<DecisionTreeData>(() =>
            {
                _agent.DesiredPosition = _agent.Point;
            }, ad);

            var isMovI = new FuncConditionBtNode<DecisionTreeData>(
                () => Data.CurrentIntention is IMoveCurrentIntention, ad);
            var isSimpleSubpath =
                new FuncConditionBtNode<DecisionTreeData>(() => Data.CurrentIntention is IMoveCurrentIntention, ad);

            var move = new SequenceBtNode();

            var subPath = new SelectorBtNode();
            var validateSubPath = new SequenceBtNode();
            var getSubPath = new SequenceBtNode();

            var subPathsStrategy = new SelectorBtNode();
            var simpleSubpath = new SequenceBtNode();
            var queueSubpath = new SequenceBtNode();

            //subpath
            var isSpOk = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                var mi = Data.CurrentIntention as IMoveCurrentIntention;
                var sp = mi.CurrentSubPath;
                return mi.Path.Count > sp && mi.Path[sp] != null && !mi.Path[sp].Finished();
            }, ad);

            var hasMoreSp = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                var mi = Data.CurrentIntention as IMoveCurrentIntention;
                return mi.Path.Count > mi.CurrentSubPath + 1;
            }, ad);

            var simpleGetSp = new ActionBtNode<DecisionTreeData>(() =>
            {
                var mi = Data.CurrentIntention as IMoveCurrentIntention;
                mi.CurrentSubPath += 1;
            }, ad);

            move.Children.Add(isMovI);
            move.Children.Add(subPath);

            subPath.Children.Add(validateSubPath);
            subPath.Children.Add(getSubPath);

            validateSubPath.Children.Add(isSpOk);
            validateSubPath.Children.Add(subPathsStrategy);

            getSubPath.Children.Add(hasMoreSp);
            getSubPath.Children.Add(simpleGetSp);
            getSubPath.Children.Add(subPathsStrategy);

            subPathsStrategy.Children.Add(simpleSubpath);
            subPathsStrategy.Children.Add(new NavFieldGetDesiredPointTask(ad));
            //subpaths.Children.Add(queuesubpath);

            simpleSubpath.Children.Add(isSimpleSubpath);
            simpleSubpath.Children.Add(simpleSubpathTask);

            #endregion
            

            #region planning

            var plan = new Queue<IPlannedIntention>();

//            plan.Enqueue(
//                new PoiProbabilityPlannedIntention
//                {
//                    Name = "drop shoes",
//                    PoiTypes = new[] {"B_k"},
//                    Probability = 1d
//                });

            plan.Enqueue(
                new PoiProbabilityPlannedIntention
                {
                    Name = "probe",
                    PoiTypes = new[] { "probe_end" },
                    Probability = 1d
                });
            
            Plan = plan;

            var isNeedToPlanning =
                new FuncConditionBtNode<DecisionTreeData>(() => { return _agent.Role.Data.PlannedIntention == null; }, ad);
            var hasMorePlans = new FuncConditionBtNode<DecisionTreeData>(() => { return Plan.Count > 0; }, ad);

            var toPlan = new ActionBtNode<DecisionTreeData>(() =>
            {
                if (Plan.Count != null && Plan.Count > 0)
                    _agent.Role.Data.PlannedIntention = Plan.Dequeue();
                
            }, ad);

            var planning = new SequenceBtNode();
            planning.Children.Add(isNeedToPlanning);
            planning.Children.Add(toPlan);

            #endregion


            var root = new SequenceBtNode();
            var actionRoot = new SelectorBtNode();

            root.Children.Add(getDesiredPositionSimple);
            root.Children.Add(actionRoot);
            actionRoot.Children.Add(planning);
            actionRoot.Children.Add(poiBehavior);

            preMove.Children.Add(move);

            TestBehTree = root;
        }

        public Queue<IPlannedIntention> Plan { get; set; }
    }
}