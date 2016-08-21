using System;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.MultiagentEngine.Scenario
{
    public interface IScenarioCondition
    {
        bool IsReady(SimulationEngine engine);
    }

    public class DoOnceAt : IScenarioCondition
    {
        private bool AlreadyDone = false;
        protected double When;

        public DoOnceAt(double when)
        {
            When = when;
        }

        public bool IsReady(SimulationEngine engine)
        {
            if (AlreadyDone)
                return false;

            var ret = Math.Abs(engine.World.Time - When) <= engine.Properties.TimeProperties.TimeStep;

            if (ret)
                AlreadyDone = true;

            return ret;
        }
    }
}