namespace Pulse.Common.Behavior.BehaviorTreeFramework.Base
{
    /// <summary>
    /// Base class for all tree nodes
    /// </summary>
    public abstract class AbstractBtNode
    {
        /// <summary>
        /// Update on tick
        /// </summary>
        /// <returns></returns>
        public abstract BehaviorState Update();
    }
}
