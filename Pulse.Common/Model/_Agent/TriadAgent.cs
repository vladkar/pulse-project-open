using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MultiagentEngine.Pulse.Agents;
using MultiagentEngine.Pulse.Map;
using Pulse.Common.Model.Scheduling;
using Pulse.Common.Pseudo3D;

namespace Pulse.Common.Model.Agent
{
    public abstract class TriadAgent : AgentBase
    {
        public IBrain Brain { set; get; }
        public IPerceptor Perceptor { set; get; }
        public IActuator Actuator { set; get; }

        public virtual Coords Point { set; get; }
        public int Floor { get; set; }

        public CurrentActivity CurrentActivity { get; set; }

        // PluginsContainer PluginsContainer { get; set; }

        protected TriadAgent(string id) : base(id)
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
