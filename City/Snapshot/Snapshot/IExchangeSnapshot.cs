namespace City.Snapshot.Snapshot
{
    public interface IExchangeSnapshot : ISnapshot
    {
        byte OriginServer { set; get; }
        byte DestinationServer { set; get; }
    }
}