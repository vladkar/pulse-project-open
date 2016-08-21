namespace City.Snapshot.PulseAgent
{
    public struct PulseMapsData : IPulseMapData
    {
        public double Xmin { set; get; }
        public double Ymin { set; get; }
        public double ToMetersMultiplier { set; get; }
        public double ToSecondsMultiplier { set; get; }
    }
}