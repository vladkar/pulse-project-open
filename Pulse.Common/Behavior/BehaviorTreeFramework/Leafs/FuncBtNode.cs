using System;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.Pulse;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Leafs
{
    public class ActionBtNode<T> : AbstractActionBtNode<T>
    {
        protected Action Action { get; set; }

        public ActionBtNode(Action action, T data) : base(data)
        {
            Action = action;
        }

        public override BehaviorState Update()
        {
            try
            {
                Action();
            }
            catch (Exception)
            {
                //TODO log
                return BehaviorState.Failed;
            }

            return BehaviorState.Success;
        }
    }

    public class FuncBtNode<T> : AbstractActionBtNode<T>
    {
        protected Func<BehaviorState> Func { get; set; }

        public FuncBtNode(Func<BehaviorState> func, T data) : base(data)
        {
            Func = func;
        }

        public override BehaviorState Update()
        {
            try
            {
                return Func();
            }
            catch (Exception)
            {
                return BehaviorState.Failed;
            }
        }
    }
}