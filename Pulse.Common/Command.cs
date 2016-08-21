namespace Pulse.Common
{
    public class Command : ICommand
    {
        public string Cmd { get; set; }
        public string[] Args { get; set; }
    }
}
