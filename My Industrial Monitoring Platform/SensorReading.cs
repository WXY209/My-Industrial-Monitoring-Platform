using System;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 表示一台设备在某个时刻产生的一条温度和压力读数。
    /// </summary>
    public sealed class SensorReading
    {
        public string DeviceId { get; private set; }
        public DateTime Timestamp { get; private set; }
        public double Temperature { get; private set; }
        public double Pressure { get; private set; }

        internal SensorReading(string deviceId, DateTime timestamp, double temperature, double pressure)
        {
            DeviceId = deviceId;
            Timestamp = timestamp;
            Temperature = temperature;
            Pressure = pressure;
        }
    }
}
