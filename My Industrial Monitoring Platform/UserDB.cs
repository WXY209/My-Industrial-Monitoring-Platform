using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;

namespace My_Industrial_Monitoring_Platform
{
    internal class UserDB
    {
        /// <summary>
        /// 创建用户表
        /// </summary>
        public static void Initizlize()
        {
            using (var connection = DatabaseDB.OpenConnection())
            {
                using (var command = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Users (
                        Username TEXT PRIMARY KEY,
                        Password TEXT NOT NULL,
                        Role TEXT NOT NULL DEFAULT '管理员'
                    );", connection))
                {
                    command.ExecuteNonQuery();
                }


                bool hasRoleColumn = false;

                using (var command = new SQLiteCommand("PRAGMA table_info(Users);", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == "Role")
                        {
                            hasRoleColumn = true;
                            break;
                        }
                    }
                }


                if (!hasRoleColumn)
                {
                    using (var command = new SQLiteCommand(
                        "ALTER TABLE Users ADD COLUMN Role TEXT NOT NULL DEFAULT '用户';",
                        connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                //在默认账号不存在时创建它
                using (var command = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Users (Username, Password,Role)
                    VALUES ('admin','admin123','管理员');", connection))
                {
                    command.ExecuteNonQuery();
                }
                using (var command = new SQLiteCommand("UPDATE Users SET Role='管理员' WHERE Username='admin';", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

        }
        /// <summary>
        /// 验证账号密码
        /// </summary>
        /// <returns></returns>
        public static bool DataLogin(string username, string password)
        {
            using (var connection = DatabaseDB.OpenConnection())
            {
                using (var command = new SQLiteCommand(@"
                    SELECT COUNT(*)
                    FROM Users
                    WHERE Username = @username AND Password = @password;", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        /// <summary>
        /// 用户注册，保存到数据库中
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool RegisterUser(string username, string password)
        {
            return CreateUser(username, password, "用户");
        }

        public static DataTable GetUsers()
        {
            var table = new DataTable();
            using (var connection = DatabaseDB.OpenConnection())
            {
                using (var command = new SQLiteCommand(@"
                    SELECT Username,Role,'正常' AS Status FROM Users
                    ORDER BY CASE WHEN Username='admin' THEN 0 ELSE 1 END,Username;", connection))
                using (var adapter = new SQLiteDataAdapter(command))
                {
                    adapter.Fill(table);
                }
            }
            return table;
        }

        public static bool CreateUser(string username, string password, string role)
        {
            using (var connection = DatabaseDB.OpenConnection())
            {
                using (var command = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Users (Username, Password, Role)
                    VALUES (@username, @password, @role);", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.AddWithValue("@role", role);

                    return command.ExecuteNonQuery() == 1;
                }
            }
        }
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool DeleteUser(string username)
        {
            using (var connection = DatabaseDB.OpenConnection())
            {
                using (var command = new SQLiteCommand(@"
            DELETE FROM Users
            WHERE Username = @username
              AND LOWER(Username) <> 'admin';", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return command.ExecuteNonQuery() == 1;
                }
            }
        }
    }
}
