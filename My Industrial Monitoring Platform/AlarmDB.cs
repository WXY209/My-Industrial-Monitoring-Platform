using System;
using System.Data;
using System.Data.SQLite;
using System.Globalization;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 报警记录的数据库操作。
    /// </summary>
    internal static class AlarmDB
    {
        public static int AddAlarm(SensorReading reading, string alarmType, double temperatureThreshold, double pressureThreshold)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                using (var command = new SQLiteCommand(@"
                    INSERT INTO Alarms
                        (DeviceId, AlarmType, Temperature, Pressure, TemperatureThreshold, PressureThreshold, StartTime, Status)
                    VALUES
                        (@device, @type, @temperature, @pressure, @temperatureLimit, @pressureLimit, @start, '处理中');", connection, transaction))
                {
                    command.Parameters.AddWithValue("@device", reading.DeviceId);
                    command.Parameters.AddWithValue("@type", alarmType);
                    command.Parameters.AddWithValue("@temperature", reading.Temperature);
                    command.Parameters.AddWithValue("@pressure", reading.Pressure);
                    command.Parameters.AddWithValue("@temperatureLimit", temperatureThreshold);
                    command.Parameters.AddWithValue("@pressureLimit", pressureThreshold);
                    command.Parameters.AddWithValue("@start", reading.Timestamp.ToString("o", CultureInfo.InvariantCulture));
                    command.ExecuteNonQuery();
                    int id;
                    using (var idCommand = new SQLiteCommand("SELECT last_insert_rowid();", connection, transaction))
                        id = Convert.ToInt32(idCommand.ExecuteScalar());
                    using (var trim = new SQLiteCommand(@"
                        DELETE FROM Alarms
                        WHERE Id NOT IN (SELECT Id FROM Alarms ORDER BY Id DESC LIMIT 75);", connection, transaction))
                    {
                        trim.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return id;
                }
            }
        }

        public static void CloseAlarm(int id, DateTime endTime)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                UPDATE Alarms SET EndTime = @end, Status = '已解除' WHERE Id = @id;", connection))
            {
                command.Parameters.AddWithValue("@end", endTime.ToString("o", CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        public static DataTable GetLatestAlarms(int limit)
        {
            var table = new DataTable();
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT Id, DeviceId, AlarmType, Temperature, Pressure,
                       TemperatureThreshold, PressureThreshold, StartTime, EndTime, Status
                FROM Alarms
                ORDER BY Id DESC
                LIMIT @limit;", connection))
            {
                command.Parameters.AddWithValue("@limit", limit);
                using (var adapter = new SQLiteDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }

        public static string FormatTime(object value)
        {
            return value == null || value == DBNull.Value
                ? string.Empty
                : DateTime.Parse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    .ToString("MM-dd HH:mm:ss");
        }
    }
}
