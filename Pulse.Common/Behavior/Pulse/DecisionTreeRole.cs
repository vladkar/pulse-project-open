using System;
using System.Collections.Generic;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.BehaviorTreeFramework.Parents;
using Pulse.Common.Behavior.Intention.Current;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.Common.Behavior.Pulse.Bt;
using Pulse.Common.Model.Agent;
using Pulse.Common.Model.AgentScheduling.Abstract;
using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.Common.Model.AgentScheduling.Planned;
using Pulse.Common.Model.Environment.Poi;
using Pulse.Common.Model.Environment.World;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Behavior.Pulse
{
    public abstract class DecisionTreeRole : AbstractAgentRole
    {
        public IAgentGroup Group { set; get; }

        protected DecisionTreeRole(AbstractPulseAgent agent, SimpleScheduleRoleConfig config, string name, int id)
            : base(agent, name, id)
        {
            Config = config; // = new SimpleScheduleRoleConfig { CornerCutOffRadius = 2, PoiInteractionRadius = 0.5, DeadZoneRadius = 0.05 }
        }

        public override void Initialize()
        {
            PrepareTree();
        }

        public override void Update()
        {
            TestBehTree.Update();
        }

        public AbstractBtNode TestBehTree { get; set; }

        public virtual void PrepareTree()
        {
            Data = new TestData();

            var ad = new DecisionTreeData
            {
                World = _agent.World as PulseWorld,
                Agent = _agent,
                Random = new Random()
            };

            //input
            var a1prob = 1d;
            //var poiTypes = _agentGoals;
            var poiTypes = new []{"", ""};

            // current goal
            // todo think where it should be stored
            var goalPoint = new PulseVector2(0, 0);
            var goalLevel = 1;
            // poi selection
            IPointOfInterest poi = null;

            #region behavior

            Data.PlannedIntention = new PoiProbabilityPlannedIntention
            {
                Name = "run",
                PoiTypes = poiTypes,
                Probability = 1d
            };


            // should i do it?
            //            var isActionPerformed = new FuncConditionBehaviorTreeNode(new Func<bool>((() => r.NextDouble() > a1prob)), null);
            var isActionPerformed = new ProbabilityCondition(ad);
            var selectPoi = new PoiSelectTask(ad);


            var isIntentionOk = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                return ad.Agent.Role.Data.CurrentIntention is IMoveCurrentIntention;
            }, ad);

            var isGoalReached = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                return _agent.Point.DistanceTo(goalPoint) < Config.PoiInteractionRadius
                       & _agent.Level == goalLevel;
            }, ad);

            var isPathOk = new FuncConditionBtNode<DecisionTreeData>(() =>
            {
                return (ad.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention)?.Path != null;
            }, ad);

            var calculatePath = new CalculatePathTask(ad);

            var poiBehavior = new SequenceBtNode();
            var setIntention = new SelectorBtNode();
            var navigate = new SelectorBtNode();
            var path = new SelectorBtNode();
            var preMove = new SequenceBtNode();

            poiBehavior.Children.Add(setIntention);
            poiBehavior.Children.Add(navigate);

            setIntention.Children.Add(isIntentionOk);
            setIntention.Children.Add(selectPoi);

            navigate.Children.Add(isGoalReached);
            navigate.Children.Add(preMove);

            preMove.Children.Add(path);

            path.Children.Add(isPathOk);
            path.Children.Add(calculatePath);

            #endregion

            #region move

            var getDesiredPositionSimple = new ActionBtNode<DecisionTreeData>(() =>
            {
                _agent.DesiredPosition = _agent.Point;
            }, ad);

            var isMovI = new FuncConditionBtNode<DecisionTreeData>(() => Data.CurrentIntention is IMoveCurrentIntention, ad);
            var isSimpleSubpath = new FuncConditionBtNode<DecisionTreeData>(() => Data.CurrentIntention is IMoveCurrentIntention, ad);

            var move = new SequenceBtNode();

            var subPath = new SelectorBtNode();
            var validateSubPath = new SequenceBtNode();
            var getSubPath = new SequenceBtNode();

            var subPathsStrategy = new SelectorBtNode();
            var simpleSubpath = new SequenceBtNode();
            var queueSubpath = new SequenceBtNode();

            var simpleSubpathTask = new SimpleGetDesiredPointTask(ad);

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
            //subpaths.Children.Add(queuesubpath);

            simpleSubpath.Children.Add(isSimpleSubpath);
            simpleSubpath.Children.Add(simpleSubpathTask);

            #endregion

            var root = new SequenceBtNode();

            root.Children.Add(getDesiredPositionSimple);
            root.Children.Add(poiBehavior);
            preMove.Children.Add(move);

            TestBehTree = root;
        }

        public override void StartInteract(IPointOfInterest poi)
        {
        }

        public override void StopInteract(IPointOfInterest poi)
        {
        }

        #region deprecated

        [Obsolete]
        public override PlannedDailyAgentSchedule GetPlannedSchedule()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override IList<AbstractScheduleActivity> GetAbstractSchedule()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override void CheckSchedule()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override CurrentActivity GetActivity()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override void CheckActivity()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override void DoActivity()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public override void GetDesiredPosition()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public interface IAgentGroup
    {
        IList<AbstractPulseAgent> Agents { set; get; }
    }

    public class MovingAgentGroup : IAgentGroup
    {
        public IList<AbstractPulseAgent> Agents { set; get; }
        public IFormation Formation { get; set; }

        public MovingAgentGroup(IFormation formation)
        {
            Formation = formation;
        }

        public PulseVector2 AskGoalPoint(AbstractPulseAgent agent)
        {
            return Formation.AskGoalPoint(agent);
        }
    }

    public interface IFormation
    {
        IList<AbstractPulseAgent> Agents { set; get; }
        PulseVector2 AskGoalPoint(AbstractPulseAgent agent);
    }

    public class LeaderFormation : IFormation
    {
        public IList<AbstractPulseAgent> Agents { get; set; }
        public AbstractPulseAgent Leader { set; get; }

        public PulseVector2 AskGoalPoint(AbstractPulseAgent agent)
        {
            // !!!
            if (agent.Id == Leader.Id) return default(PulseVector2);
            return Leader.Point;
        }
    }
}