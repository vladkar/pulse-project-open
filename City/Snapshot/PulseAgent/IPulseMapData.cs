namespace City.Snapshot.PulseAgent
{
    public interface IPulseMapData
    {
        double Xmin { set; get; }
        double Ymin { set; get; }
        double ToMetersMultiplier { set; get; }
        double ToSecondsMultiplier { set; get; }
    }
}