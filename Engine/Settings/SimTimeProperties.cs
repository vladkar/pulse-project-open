namespace Pulse.MultiagentEngine.Settings
{
    public class SimTimeProperties
    {
        /// <summary>
        /// When engine stops execution
        /// in sec
        /// </summary>
        public double SimulationEndsAt { get; set; }

        /// <summary>
        /// When engine starts simulation.
        /// 0.0 is 0 s. after 0h 0m
        /// </summary>
        public double SimulationStartsAt { get; set; }

        /// <summary>
        /// Length of time step in simulation
        /// </summary>
        public double TimeStep { get; set; }

        /// <summary>
        /// Simulation time unit to seconds ration.
        /// For example model time unit can be equal to year.
        /// </summary>
        public double ToSecondsMultiplier { get; set; }

        public SimTimeProperties(double timestep = 0.1, double toSecondsMultiplier = 1.0, double simulationEndsAt = double.MaxValue)
        {
            TimeStep = timestep;
            ToSecondsMultiplier = toSecondsMultiplier;
            SimulationEndsAt = simulationEndsAt;
        }
    }
}