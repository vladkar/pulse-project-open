using System.Collections.Generic;
using Pulse.Common.Model.Agent;

namespace Pulse.Common.Model.Environment.Poi
{
    public interface IInteractable
    {
        IList<AbstractPulseAgent> Agents { set; get; }

        event OnAgentExitDelegate OnAgentExit;
        event OnAgentEnterDelegate OnAgentEnter;

        void Enter(AbstractPulseAgent agent);
        void Exit(AbstractPulseAgent agent);
    }

    public interface IEnterable : IInteractable { }
    public interface IActivePoi : IInteractable { }

    public delegate void OnAgentExitDelegate(AbstractPulseAgent agent);
    public delegate void OnAgentEnterDelegate(AbstractPulseAgent agent);
}