using System;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;

namespace Pulse.Common.Behavior.Pulse.Bt
{
    public class SimplePathUpdateTast : AbstractActionBtNode<DecisionTreeData>
    {
        public SimplePathUpdateTast(DecisionTreeData data) : base(data)
        {
        }

        public override BehaviorState Update()
        {
            throw new NotImplementedException();
        }
    }
}
