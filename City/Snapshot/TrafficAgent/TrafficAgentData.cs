namespace City.Snapshot.TrafficAgent
{
    public class TrafficAgentData
    {
        public ulong Id { set; get; }
        public double X { set; get; }
        public double Y { set; get; }
        public bool IsEmergency { set; get; }
        public byte direction { set; get; } 
    }
}
