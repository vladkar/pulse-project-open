using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.Pulse;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Leafs
{
    public abstract class AbstractConditionBtNode<T> : AbstractBtLeaf<T>
    {
        protected AbstractConditionBtNode(T data) : base(data)
        {
        }
    }
}