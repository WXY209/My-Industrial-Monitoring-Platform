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

            // 查询结果按最新优先排列；倒序加入后，曲线缓存按时间从旧到新排列。
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

        public static DataTable GetHistoryPage(DateTime startInclusive, DateTime endExclusive, string deviceId, int page, int pageSize)
        {
            var table = new DataTable();
            int offset = Math.Max(0, page - 1) * pageSize;
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT Id AS [编号], DeviceId AS [设备], Timestamp AS [采样时间],
                       Temperature AS [温度], Pressure AS [压力]
                FROM Readings
                WHERE Timestamp >= @start AND Timestamp < @end
                  AND (@device = '' OR DeviceId = @device)
                ORDER BY Id DESC LIMIT @limit OFFSET @offset;", connection))
            {
                command.Parameters.AddWithValue("@start", startInclusive.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@end", endExclusive.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@device", deviceId ?? string.Empty);
                command.Parameters.AddWithValue("@limit", pageSize);
                command.Parameters.AddWithValue("@offset", offset);
                using (var adapter = new SQLiteDataAdapter(command)) adapter.Fill(table);
            }
            return table;
        }

        public static int GetHistoryCount(DateTime startInclusive, DateTime endExclusive, string deviceId)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT COUNT(*) FROM Readings
                WHERE Timestamp >= @start AND Timestamp < @end
                  AND (@device = '' OR DeviceId = @device);", connection))
            {
                command.Parameters.AddWithValue("@start", startInclusive.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@end", endExclusive.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@device", deviceId ?? string.Empty);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
