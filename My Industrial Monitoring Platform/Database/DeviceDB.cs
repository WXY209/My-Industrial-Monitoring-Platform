using System;
using System.Data;
using System.Data.SQLite;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 设备信息的数据库操作。
    /// </summary>
    internal static class DeviceDB
    {
        public static DataTable GetActiveDevices()
        {
            return GetDevices(false);
        }

        public static DataTable GetDeletedDevices()
        {
            return GetDevices(true);
        }

        private static DataTable GetDevices(bool deleted)
        {
            var table = new DataTable();
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                SELECT DeviceId, Name, CreatedAt, DeletedAt
                FROM Devices
                WHERE IsDeleted = @deleted
                ORDER BY DeviceId;", connection))
            {
                command.Parameters.AddWithValue("@deleted", deleted ? 1 : 0);
                using (var adapter = new SQLiteDataAdapter(command))
                    adapter.Fill(table);
            }
            return table;
        }

        public static string GetNextDeviceId()
        {
            int largestNumber = 0;
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand("SELECT DeviceId FROM Devices;", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string id = reader["DeviceId"].ToString();
                    int number;
                    if (id.StartsWith("DEV-", StringComparison.OrdinalIgnoreCase)
                        && int.TryParse(id.Substring(4), out number)
                        && number > largestNumber)
                    {
                        largestNumber = number;
                    }
                }
            }
            return "DEV-" + (largestNumber + 1).ToString("D3");
        }

        public static bool AddDevice(string deviceId, string name)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                INSERT INTO Devices (DeviceId, Name, CreatedAt, IsDeleted)
                VALUES (@id, @name, @created, 0);", connection))
            {
                command.Parameters.AddWithValue("@id", deviceId);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@created", DateTime.Now.ToString("o"));
                return command.ExecuteNonQuery() == 1;
            }
        }

        public static bool SetDeleted(string deviceId, bool deleted)
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand(@"
                UPDATE Devices
                SET IsDeleted = @deleted, DeletedAt = @deletedAt
                WHERE DeviceId = @id AND IsDeleted <> @deleted;", connection))
            {
                command.Parameters.AddWithValue("@deleted", deleted ? 1 : 0);
                command.Parameters.AddWithValue("@deletedAt", deleted ? (object)DateTime.Now.ToString("o") : DBNull.Value);
                command.Parameters.AddWithValue("@id", deviceId);
                return command.ExecuteNonQuery() == 1;
            }
        }

        public static int GetActiveCount()
        {
            using (var connection = DatabaseDB.OpenConnection())
            using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Devices WHERE IsDeleted = 0;", connection))
                return Convert.ToInt32(command.ExecuteScalar());
        }
    }
}
