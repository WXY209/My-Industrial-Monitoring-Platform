using System;
using System.Collections.Generic;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 一台模拟设备在某个时刻产生的温度和压力读数。
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

    /// <summary>
    /// 为每台设备生成连续、平滑波动的模拟读数。
    /// </summary>
    public sealed class MonitoringSimulator
    {
        private sealed class DeviceState
        {
            public double Temperature;
            public double Pressure;
        }

        private readonly Random random = new Random();
        private readonly Dictionary<string, DeviceState> states =
            new Dictionary<string, DeviceState>(StringComparer.OrdinalIgnoreCase);

        public SensorReading NextReading(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("设备编号不能为空。", "deviceId");

            DeviceState state;
            if (!states.TryGetValue(deviceId, out state))
            {
                state = new DeviceState
                {
                    Temperature = 40 + random.NextDouble() * 4,
                    Pressure = 1.2 + random.NextDouble() * 0.2
                };
                states.Add(deviceId, state);
            }

            // 正常值围绕较低的工况缓慢回归并小幅波动。
            state.Temperature = 55.0
                + (state.Temperature - 55.0) * 0.65
                + (random.NextDouble() - 0.5) * 2.4;
            state.Pressure = 1.5
                + (state.Pressure - 1.5) * 0.65
                + (random.NextDouble() - 0.5) * 0.12;

            // 偶尔制造短暂的异常峰值，用于观察报警效果。
            if (random.NextDouble() < 0.025)
                state.Temperature += 10.0 + random.NextDouble() * 4.0;
            if (random.NextDouble() < 0.025)
                state.Pressure += 0.35 + random.NextDouble() * 0.2;

            state.Temperature = Clamp(state.Temperature, 55.0, 92.0);
            state.Pressure = Clamp(state.Pressure, 1.5, 2.7);

            return new SensorReading(
                deviceId,
                DateTime.Now,
                Math.Round(state.Temperature, 2),
                Math.Round(state.Pressure, 2));
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}
