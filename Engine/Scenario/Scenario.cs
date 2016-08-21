using System.Collections.Generic;
using Pulse.MultiagentEngine.Engine;

namespace Pulse.MultiagentEngine.Scenario
{
    public interface IScenario
    {
        void Apply(SimulationEngine engine);
    }

    /// <summary>
    /// with condition
    /// </summary>
    public abstract class ScenarioBase : IScenario
    {
        protected IScenarioCondition Condition;

        protected ScenarioBase(IScenarioCondition condition)
        {
            Condition = condition;
        }

        protected abstract void Do(SimulationEngine engine);

        public void Apply(SimulationEngine engine)
        {
            if(Condition.IsReady(engine))
            {
                Do(engine);
            }
        }
    }

    /// <summary>
    /// container
    /// </summary>
    public class GlobalScenario: IScenario
    {
        private List<IScenario> scenarios = new List<IScenario>();

        public void Add(IScenario item)
        {
            scenarios.Add(item);
        }

        public void Apply(SimulationEngine engine)
        {
            foreach (var scenario in scenarios)
            {
                scenario.Apply(engine);
            }
        }
    }

}
