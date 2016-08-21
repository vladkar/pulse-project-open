using System.Collections.Generic;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Parents
{
    /// <summary>
    /// Simple selector traverse
    /// </summary>
    public class SelectorBtNode : AbstractBtParent
    {
        public IList<AbstractBtNode> Children { set; get; }

        public SelectorBtNode()
        {
            Children = new List<AbstractBtNode>();
        }

        /// <summary>
        /// Simple selector traverse
        /// </summary>
        /// <returns></returns>
        public override BehaviorState Update()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                switch (Children[i].Update())
                {
                    case BehaviorState.Success:
                        return BehaviorState.Success;
                    case BehaviorState.Running:
                        return BehaviorState.Running;
                    default:
                        continue;
                }
            }

            return BehaviorState.Failed;
        }
    }
}