using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace My_Industrial_Monitoring_Platform
{
    internal class UserDB
    {
        //数据库放在当前用户的本地目录
        private static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MyIndustrialMonitoringPlatform","user.db");

        private static readonly string ConnectionString = "Data Source="+DatabasePath+";Version=3";
        /// <summary>
        /// 创建用户表
        /// </summary>
        public static void Initizlize() 
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath));
            using (var connection=new SQLiteConnection(ConnectionString)) 
            {
                connection.Open();
                using (var command = new SQLiteCommand(@"
                    CREATE TABLE IF NOT EXISTS Users (
                        Username TEXT PRIMARY KEY,
                        Password TEXT NOT NULL
                    );", connection))
                {
                    command.ExecuteNonQuery();
                }
                //在默认账号不存在时创建它
                using (var command = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Users (Username, Password)
                    VALUES (@username, @password);", connection))
                {
                    command.Parameters.AddWithValue("@username", "admin");
                    command.Parameters.AddWithValue("@password", "admin123");
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
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

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
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(@"
            INSERT OR IGNORE INTO Users (Username, Password)
            VALUES (@username, @password);", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    //成功返回 true；用户名已存在则返回 false
                    return command.ExecuteNonQuery() == 1;
                }
            }
        }
    }
}
