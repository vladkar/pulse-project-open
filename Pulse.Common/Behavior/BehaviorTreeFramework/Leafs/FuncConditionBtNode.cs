using System;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.Pulse;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Leafs
{
    public class FuncConditionBtNode<T> : AbstractConditionBtNode<T>
    {
        protected Func<bool> Condition { get; set; }

        public FuncConditionBtNode(Func<bool> condition, T data) : base(data)
        {
            Condition = condition;
        }

        public override BehaviorState Update()
        {
            return Condition() ? BehaviorState.Success : BehaviorState.Failed;
        }
    }
}