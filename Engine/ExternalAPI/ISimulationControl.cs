using System.Collections.Generic;
using Pulse.MultiagentEngine.Stats;

namespace Pulse.MultiagentEngine.ExternalAPI
{
    public enum SimulationState
    {
        Init, Active, Paused, Stopped, Error, Finished
    }

    public enum CommandResultState
    {
        Ok,
        Error
    }

    public class CommandResult
    {
        public CommandResultState ResultState { get; private set; }
        public string ResultStateComment { get; private set; }
        public object Arg { get; private set; }

        public CommandResult(CommandResultState resultState, string resultStateMessage = null, object arg = null)
        {
            ResultState = resultState;
            Arg = arg;
            ResultStateComment = resultStateMessage;
        }

        public CommandResult(CommandResultState resultState, object arg)
        {
            ResultState = resultState;
            Arg = arg;
        }
    }

    /// <summary>
    /// External interface for simulation contol. For example from visualizer.
    /// </summary>
    public interface ISimulationControl : ITimestampedMapDataProvider, ITimestampedAgentsDataProvider, ITimestampedMapInfoProvider, ITimestampedDeadAgentsDataProvider
    {
        #region Get information from simulation
        /// <summary>
        /// State of the simulation
        /// </summary>
        SimulationState State { get; }

        /// <summary>
        /// Statistics
        /// </summary>
        //AgentsAggregatedStatistics Stats { get; }
        IDictionary<string, string> Stats { get; }

        SimulationInfo Info { get; }
        #endregion
        
        #region Control
        /// <summary>
        /// Generic command interface.
        /// Examples:
        /// - player: stop,start,pause...
        /// - system: exit
        /// - set input params - steering
        /// - domain specific commands: break the bridge
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="arg"></param>
        CommandResult Command(string commandName, object arg);

        /// <summary>
        /// Pause
        /// </summary>
        void Pause();

        /// <summary>
        /// Simulation resume after pause
        /// </summary>
        void Resume();

        /// <summary>
        /// Simulation stop
        /// </summary>
        void Stop();

        /// <summary>
        /// Simulation start
        /// </summary>
        void Start();

        /// <summary>
        /// For "by step" control
        /// </summary>
        void DoStep();

        void Finalization();
        #endregion
    }
    
}
