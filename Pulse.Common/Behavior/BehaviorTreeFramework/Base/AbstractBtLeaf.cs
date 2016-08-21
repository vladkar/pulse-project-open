using Pulse.Common.Behavior.Pulse;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Base
{
    /// <summary>
    /// Base class for tree leafs
    /// </summary>
    public abstract class AbstractBtLeaf<T> : AbstractBtNode
    {
        /// <summary>
        /// Leaf data
        /// </summary>
        protected T Data { get; set; }

        protected AbstractBtLeaf(T data)
        {
            Data = data;
        }
    }
}