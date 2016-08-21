using Pulse.Common.Behavior.BehaviorTreeFramework.Base;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Decorators
{
    public class LooperBtDecorator : AbstracBtDecorator
    {
        protected LooperBtDecorator(AbstractBtNode carrier) : base(carrier)
        {
        }

        public override BehaviorState Update()
        {
            switch (Carrier.Update())
            {
                case BehaviorState.Failed:
                    return BehaviorState.Failed;
                default:
                    return BehaviorState.Running;
            }
        }
    }
}