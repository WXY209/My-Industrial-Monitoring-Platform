using System;
using System.Collections.Generic;

namespace My_Industrial_Monitoring_Platform
{
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
                    Temperature = 39 + random.NextDouble() * 2,
                    Pressure = 1.15 + random.NextDouble() * 0.1
                };
                states.Add(deviceId, state);

                // 首次启动时直接返回低位初值，不在首条读数上叠加异常峰值。
                return new SensorReading(
                    deviceId,
                    DateTime.Now,
                    Math.Round(state.Temperature, 2),
                    Math.Round(state.Pressure, 2));
            }

            // 正常值围绕 40°C、1.2 MPa 缓慢回归并小幅波动。
            state.Temperature = 40.0
                + (state.Temperature - 40.0) * 0.65
                + (random.NextDouble() - 0.5) * 1.2;
            state.Pressure = 1.2
                + (state.Pressure - 1.2) * 0.65
                + (random.NextDouble() - 0.5) * 0.08;

            // 偶尔制造短暂的异常峰值，用于观察报警效果。
            if (random.NextDouble() < 0.02)
                state.Temperature += 21.0 + random.NextDouble() * 4.0;
            if (random.NextDouble() < 0.02)
                state.Pressure += 0.7 + random.NextDouble() * 0.3;

            state.Temperature = Clamp(state.Temperature, 30.0, 70.0);
            state.Pressure = Clamp(state.Pressure, 0.8, 2.2);

            return new SensorReading(
                deviceId,
                DateTime.Now,
                Math.Round(state.Temperature, 2),
                Math.Round(state.Pressure, 2));
        }

        public void RemoveDevice(string deviceId)
        {
            if (!string.IsNullOrWhiteSpace(deviceId))
                states.Remove(deviceId);
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}
