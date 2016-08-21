using System;

namespace Pulse.MultiagentEngine.ExternalAPI
{
    [Serializable]
    public class TimestampedData<T>
    {
        public double Timestamp { get; set; }
        public T Data { get; set; }

        public TimestampedData(double timestamp, T data)
        {
            Timestamp = timestamp;
            Data = data;
        }
    }

    [Serializable]
    public class GeoTimestampedData<T>
    {
        public DateTime Timestamp { get; set; }
        public T Data { get; set; }

        public GeoTimestampedData(DateTime timestamp, T data)
        {
            Timestamp = timestamp;
            Data = data;
        }
    }
}