using System;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;
using Pulse.Common.Behavior.Intention.Current;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class CalculatePathTask : AbstractActionBtNode<DecisionTreeData>
    {
        public CalculatePathTask(DecisionTreeData data) : base(data)
        {
        }

        public override BehaviorState Update()
        {
            var currentIntention = Data.Agent.Role.Data.CurrentIntention as IMoveCurrentIntention;

            if (currentIntention == null) return BehaviorState.Failed;

            try
            {
                currentIntention.Path = Data.Agent.CalculatePath(Data.Agent.Point, Data.Agent.Level, currentIntention.GoalPoint, currentIntention.Level);
            }
            catch (Exception)
            {
                //TODO log
                return BehaviorState.Failed;
            }

            return BehaviorState.Success;
        }
    }
}