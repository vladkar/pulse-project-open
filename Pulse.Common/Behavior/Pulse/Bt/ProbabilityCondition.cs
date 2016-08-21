using System.Collections;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Planned;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class ProbabilityCondition : AbstractConditionBtNode<DecisionTreeData>
    {
        public ProbabilityCondition(DecisionTreeData data) : base(data)
        {
        }

        public override BehaviorState Update()
        {
            var plannedIntention = Data.Agent.Role.Data.PlannedIntention as IProbabilityPlannedIntension;
            if (plannedIntention == null) return BehaviorState.Failed;

            return Data.Random.NextDouble() < plannedIntention.Probability ? BehaviorState.Success : BehaviorState.Failed;
        }
    }

    // TODO real shit inside: refactor typed pois etc.

    //todo make not static!!! this is test version
}