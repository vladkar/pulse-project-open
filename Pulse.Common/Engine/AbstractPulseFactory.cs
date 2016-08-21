using System;
using Pulse.Common.ConfigSystem;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.Common.Engine
{
    public abstract class AbstractPulseFactory
    {
        public static AbstractPulseFactory GetInstance()
        {
            var instance = (AbstractPulseFactory)Activator.CreateInstance("Pulse.Model", "Pulse.Model.ModelFactory").Unwrap();
            return instance;
        }

        public abstract PulseScenarioConfig GetConfig(string scenario);
        public abstract PulseEngine GetEngine(PulseScenarioConfig config, SimulationRunMode simulationRunMode);
    }
}
