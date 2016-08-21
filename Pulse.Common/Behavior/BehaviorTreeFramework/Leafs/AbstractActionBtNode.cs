using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.Pulse;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Leafs
{
    public abstract class AbstractActionBtNode<T> : AbstractBtLeaf<T>
    {
        protected AbstractActionBtNode(T data) : base(data)
        {
        }
    }
}