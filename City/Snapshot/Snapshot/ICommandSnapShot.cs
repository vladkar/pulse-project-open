namespace City.Snapshot.Snapshot
{
    public interface ICommandSnapshot : ISnapshot
    {
        string Command { set; get; }
        string[] Args { set; get; }
    }
}