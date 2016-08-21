using System;
using System.Collections.Concurrent;
using Pulse.Common.Behavior.BehaviorTreeFramework.Base;
using Pulse.Common.Behavior.Pulse.Bt;

namespace Pulse.Common.Behavior.BehaviorTreeFramework.Decorators
{
    public class ParallelWorkerBtDecorator : AbstracBtDecorator
    {
        protected ParallelWorkerBtDecorator(AbstractBtNode carrier) : base(carrier)
        {
        }

        public override BehaviorState Update()
        {
            throw new NotImplementedException();
        }
    }
}