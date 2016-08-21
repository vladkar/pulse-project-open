namespace Pulse.MultiagentEngine.Events
{
    public class SimulationEvent
    {
        public string Type { get; private set; }
        public string Comment { get; set; }
        public object Arg { get; set; }

        public SimulationEvent(string type)
        {
            Type = type;
        }

        public SimulationEvent(string type, object arg, string comment = "")
        {
            Comment = comment;
            Arg = arg;
            Type = type;
        }
    }


    public class SimulationEventArgs
    {
        public SimulationEvent Event { get; set;}

        public SimulationEventArgs(SimulationEvent @event)
        {
            Event = @event;
        }
    }

    public class EventEngine
    {
        // Declare the delegate (if using non-generic pattern).
        public delegate void SimulationEventHandler(object sender, SimulationEventArgs e);

        // Declare the event.
        public event SimulationEventHandler SimulationEvent;

        // Wrap the event in a protected virtual method
        // to enable derived classes to raise the event.
        public void PushEvent(SimulationEvent ev)
        {
            // Raise the event by using the () operator.
            if (SimulationEvent != null)
                SimulationEvent(this, new SimulationEventArgs(ev));
        }        

    }
}
