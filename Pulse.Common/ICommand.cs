namespace Pulse.Common
{
    public interface ICommand
    {
        string Cmd { set; get; }
        string[] Args { set; get; }
    }
}
