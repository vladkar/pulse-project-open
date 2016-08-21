using Pulse.Common.Model.AgentScheduling.Current;
using Pulse.MultiagentEngine.Agents;
using Pulse.MultiagentEngine.Map;

namespace Pulse.Common.Model.Agent
{
    public abstract class TriadAgent : AgentBase
    {
        public IBrain Brain { set; get; }
        public IPerceptor Perceptor { set; get; }
        public IActuator Actuator { set; get; }

        public virtual PulseVector2 Point { set; get; }
        public int Floor { get; set; }

        public CurrentActivity CurrentActivity { get; set; }

        // PluginsContainer PluginsContainer { get; set; }

        protected TriadAgent(long id) : base(id)
        {
        }
    }

    public interface IActuator
    {
        void DoActivity(CurrentActivity activity);
//        void FinishActivity();
    }

    public interface IPerceptor
    {
    }

    public interface IBrain
    {
        CurrentActivity GetActivity();
    }
}
