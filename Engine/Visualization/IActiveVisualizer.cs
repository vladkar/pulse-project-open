using Pulse.MultiagentEngine.ExternalAPI;

namespace Pulse.MultiagentEngine.Visualization
{
    public enum VisualizationConnectionType
    {
        /// <summary>
        /// Each visualization update cycle the simulation method DoStep will be called.
        /// In this case visualization FPS will dictate simulation iteration delay.
        /// </summary>
        ActiveSyncronizedUpdate,

        /// <summary>
        /// Each visualization step data from simulation will be transferred to visualization.
        /// Simulation in this case may be threaded and ISimulationControl should provide the thread safety.
        /// </summary>
        AsyncronizedUpdate,

        WarmUp
    }

    /// <summary>
    /// Active visualizer calls simulation to get data and to control it.
    /// </summary>
    public interface IActiveVisualizer
    {
        void ConnectToSimulation(ISimulationControl simulationControl, VisualizationConnectionType connectionType);
    }
}
