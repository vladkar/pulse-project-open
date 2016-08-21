using System;
using Pulse.MultiagentEngine.Containers;

namespace Pulse.MultiagentEngine.Agents
{

    public enum TerminationReason
    {
        None,
        Finishing,
        Migration
    }


    public abstract class AgentBase : IIdentifiedItem
    {
        #region Parents
        public SimWorld World { get; set; }
        #endregion

        public long Id { get; set; }

        #region Engine service params
        public bool IsAlive { get; set; }
        public TerminationReason TerminationReason = TerminationReason.None;
        public string TerminationReasonInfo { set; get; }
        #endregion

        #region Constructor
        protected AgentBase(long id)
        {
            Id = id;
            IsAlive = true;
        }
        #endregion
        
        #region Abstracts
        public abstract void StepAct(double timeStep, double time); //note I think It would be better to add timestep parameter here
        #endregion

        #region Agent API
        /// <summary>
        /// Mark agent as dead. It will be cleaned up by engine.
        /// </summary>
        public virtual void Terminate(TerminationReason reason)
        {
            IsAlive = false;
            TerminationReason = reason;
        }
        #endregion

        public virtual void InitializeSimulation(double simTime)
        {
            
        }

        public virtual string PrettyPrint()
        {
            string str = "";
            str += String.Format("== Agent #{0} ==\n", Id);
            str += String.Format("IsAlive: {0}\n", IsAlive);
            return str;
        }
    }

    public interface IIdentifiedItem
    {
        long Id { set; get; }
    }
}
