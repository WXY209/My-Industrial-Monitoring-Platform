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
            FindSolutionDirectory(),
            "user.db");

        private static readonly string ConnectionString = "Data Source=" + DatabasePath + ";Version=3";

        private static string FindSolutionDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (directory != null)
            {
                if (Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
                    return directory.FullName;
                directory = directory.Parent;
            }

            // 发布后找不到解决方案文件时，把数据库放在程序所在目录。
            return AppDomain.CurrentDomain.BaseDirectory;
        }

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
                CREATE TABLE IF NOT EXISTS CommunicationLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp TEXT NOT NULL,
                    DeviceId TEXT NULL,
                    Direction TEXT NOT NULL,
                    Operation TEXT NOT NULL,
                    Result TEXT NOT NULL,
                    Details TEXT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_CommunicationLogs_Timestamp
                    ON CommunicationLogs(Timestamp DESC);
                CREATE INDEX IF NOT EXISTS IX_CommunicationLogs_Device_Time
                    ON CommunicationLogs(DeviceId, Timestamp DESC);
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
