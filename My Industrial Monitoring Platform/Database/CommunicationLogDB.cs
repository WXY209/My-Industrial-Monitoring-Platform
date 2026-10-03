using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信日志的 SQLite 读写操作。</summary>
    internal static class CommunicationLogDB
    {
        public static void AddEntry(DateTime timestamp, string deviceId, string direction,
            string operation, string result, string details)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                INSERT INTO CommunicationLogs
                    (Timestamp, DeviceId, Direction, Operation, Result, Details)
                VALUES
                    (@timestamp, @device, @direction, @operation, @result, @details);", connection))
            {
                command.Parameters.AddWithValue("@timestamp", timestamp.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@device", (object)deviceId ?? DBNull.Value);
                command.Parameters.AddWithValue("@direction", direction ?? string.Empty);
                command.Parameters.AddWithValue("@operation", operation ?? string.Empty);
                command.Parameters.AddWithValue("@result", result ?? string.Empty);
                command.Parameters.AddWithValue("@details", (object)details ?? DBNull.Value);
                command.ExecuteNonQuery();
            }
        }

        public static DataTable GetRecentEntries(int limit)
        {
            var table = new DataTable();
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT Timestamp, DeviceId, Direction, Operation, Result, Details
                FROM CommunicationLogs
                ORDER BY Id DESC
                LIMIT @limit;", connection))
            {
                command.Parameters.AddWithValue("@limit", Math.Max(0, limit));
                using (var adapter = new SQLiteDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }
    }
}
