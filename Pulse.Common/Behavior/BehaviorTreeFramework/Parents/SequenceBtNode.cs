using System.Collections.Generic;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Parents
{
    /// <summary>
    /// Simple sequence traverse
    /// </summary>
    public class SequenceBtNode : AbstractBtParent
    {
        public IList<AbstractBtNode> Children { set; get; }

        public SequenceBtNode()
        {
            Children = new List<AbstractBtNode>();
        }
        
        /// <summary>
        /// Simple sequence traverse
        /// </summary>
        /// <returns></returns>
        public override BehaviorState Update()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                switch (Children[i].Update())
                {
                    case BehaviorState.Failed:
                        return BehaviorState.Failed;
                    case BehaviorState.Running:
                        return BehaviorState.Running;
                    default:
                        continue;
                }
            }

            return BehaviorState.Success;
        }
    }
}
