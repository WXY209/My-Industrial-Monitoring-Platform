using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 温度和压力采样数据的数据库操作。
    /// </summary>
    internal static class ReadingDB
    {
        public static void AddReading(SensorReading reading)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                INSERT INTO Readings (DeviceId, Timestamp, Temperature, Pressure)
                VALUES (@device, @time, @temperature, @pressure);", connection))
            {
                command.Parameters.AddWithValue("@device", reading.DeviceId);
                command.Parameters.AddWithValue("@time", reading.Timestamp.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@temperature", reading.Temperature);
                command.Parameters.AddWithValue("@pressure", reading.Pressure);
                command.ExecuteNonQuery();
            }
        }

        public static List<SensorReading> GetRecentReadings(string deviceId, int limit)
        {
            var readings = new List<SensorReading>();
            var table = new DataTable();
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT DeviceId, Timestamp, Temperature, Pressure
                FROM Readings
                WHERE DeviceId = @device
                ORDER BY Id DESC
                LIMIT @limit;", connection))
            {
                command.Parameters.AddWithValue("@device", deviceId);
                command.Parameters.AddWithValue("@limit", limit);
                using (var adapter = new SQLiteDataAdapter(command))
                    adapter.Fill(table);
            }

            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow row = table.Rows[i];
                DateTime timestamp = DateTime.Parse(row["Timestamp"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                readings.Add(new SensorReading(
                    row["DeviceId"].ToString(),
                    timestamp,
                    Convert.ToDouble(row["Temperature"], CultureInfo.InvariantCulture),
                    Convert.ToDouble(row["Pressure"], CultureInfo.InvariantCulture)));
            }
            return readings;
        }
    }
}
