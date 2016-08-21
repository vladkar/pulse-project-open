namespace Pulse.MultiagentEngine.ExternalAPI
{
    public interface ICommandExecutor
    {
        CommandResult Command(string commandName, object arg);
    }
}
