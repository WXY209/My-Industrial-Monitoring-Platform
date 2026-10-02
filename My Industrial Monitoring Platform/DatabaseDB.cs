using System;
using System.Data.SQLite;
using System.IO;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// SQLite 数据库连接和表结构初始化。
    /// </summary>
    internal static class DatabaseDB
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..")),
            "user.db");

        private static readonly string ConnectionString = "Data Source=" + DatabasePath + ";Version=3";

        public static SQLiteConnection OpenConnection()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath));
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        public static void Initialize()
        {
            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS Devices (
                    DeviceId TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    IsDeleted INTEGER NOT NULL DEFAULT 0,
                    DeletedAt TEXT NULL
                );
                CREATE TABLE IF NOT EXISTS Readings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceId TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Temperature REAL NOT NULL,
                    Pressure REAL NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Readings_Device_Time ON Readings(DeviceId, Timestamp);
                CREATE TABLE IF NOT EXISTS Alarms (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceId TEXT NOT NULL,
                    AlarmType TEXT NOT NULL,
                    Temperature REAL NOT NULL,
                    Pressure REAL NOT NULL,
                    TemperatureThreshold REAL NOT NULL,
                    PressureThreshold REAL NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NULL,
                    Status TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Alarms_StartTime ON Alarms(StartTime);
            ", connection))
            {
                command.ExecuteNonQuery();
            }

            int deviceCount;
            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Devices;", connection))
            {
                deviceCount = Convert.ToInt32(command.ExecuteScalar());
            }

            if (deviceCount == 0)
                DeviceDB.AddDevice("DEV-001", "DEV-001");
        }
    }
}
