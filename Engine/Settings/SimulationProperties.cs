namespace Pulse.MultiagentEngine.Settings
{
    public class SimulationProperties
    {
        public SimTimeProperties TimeProperties { get; set; }
        
        private bool _useNormalStopCondition = true;
        public bool UseNormalStopCondition
        {
            get { return _useNormalStopCondition; }
            set { _useNormalStopCondition = value; }
        }
    }
}
