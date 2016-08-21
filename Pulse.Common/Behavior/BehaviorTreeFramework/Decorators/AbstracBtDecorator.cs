using Pulse.Common.Behavior.BehaviorTreeFramework.Base;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Decorators
{
    public abstract class AbstracBtDecorator : AbstractBtNode
    {
        public AbstractBtNode Carrier { get; protected set; }

        protected AbstracBtDecorator(AbstractBtNode carrier)
        {
            Carrier = carrier;
        }
    }
}