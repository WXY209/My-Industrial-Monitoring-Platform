using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>用户管理服务：将同步 SQLite 操作放到线程池执行。</summary>
    public sealed class UserManagementService : IUserManagementService
    {
        public Task<bool> ValidateLoginAsync(string username, string password)
        {
            return Task.Run(() => UserDB.DataLogin(username, password));
        }

        public Task<bool> RegisterUserAsync(string username, string password)
        {
            return DatabaseWriteQueue.ExecuteAsync(() => UserDB.RegisterUser(username, password));
        }

        public Task<DataTable> GetUsersAsync()
        {
            return Task.Run(() => UserDB.GetUsers());
        }

        public Task<bool> CreateUserAsync(string username, string password, string role)
        {
            return DatabaseWriteQueue.ExecuteAsync(() => UserDB.CreateUser(username, password, role));
        }

        public Task<bool> DeleteUserAsync(string username)
        {
            return DatabaseWriteQueue.ExecuteAsync(() => UserDB.DeleteUser(username));
        }
    }
}
