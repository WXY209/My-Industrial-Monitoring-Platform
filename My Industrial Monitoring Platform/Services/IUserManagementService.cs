using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>用户管理页面使用的用户操作接口。</summary>
    public interface IUserManagementService
    {
        Task<bool> ValidateLoginAsync(string username, string password);
        Task<bool> RegisterUserAsync(string username, string password);
        Task<DataTable> GetUsersAsync();
        Task<bool> CreateUserAsync(string username, string password, string role);
        Task<bool> DeleteUserAsync(string username);
    }
}
