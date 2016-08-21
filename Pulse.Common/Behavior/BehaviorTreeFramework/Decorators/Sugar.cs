using System;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.BehaviorTreeFramework.Leafs;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Decorators
{
    public class BranchBtNode : AbstractBtParent
    {
        private Func<bool> _cond;
        private AbstractBtNode _success;
        private AbstractBtNode _fail;

        public BranchBtNode(Func<bool> cond, AbstractBtNode success, AbstractBtNode fail)
        {
            _cond = cond;
            _success = success;
            _fail = fail;
        }

        public override BehaviorState Update()
        {
            return _cond() ? _success.Update() : _fail.Update();
        }
    }

    public class BranchConditionDecoratorBtNode<T> : AbstractBtParent
    {
        private AbstractConditionBtNode<T> _cond;
        private AbstractBtNode _success;
        private AbstractBtNode _fail;

        public BranchConditionDecoratorBtNode(AbstractConditionBtNode<T> cond, AbstractBtNode success, AbstractBtNode fail)
        {
            _cond = cond;
            _success = success;
            _fail = fail;
        }

        //todo running bug
        public override BehaviorState Update()
        {
            var res = _cond.Update();
            switch (res)
            {
                case BehaviorState.Success:
                    _success.Update();
                    return BehaviorState.Success;
                case BehaviorState.Failed:
                    _fail.Update();
                    return BehaviorState.Success;
                default:
                    return res;
            }
        }
    }
}
