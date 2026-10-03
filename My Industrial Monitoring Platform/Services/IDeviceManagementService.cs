using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页设备列表的数据库操作接口。</summary>
    public interface IDeviceManagementService
    {
        Task<string> AddNextDeviceAsync();
        Task<int> GetActiveCountAsync();
        Task<DataTable> GetDeletedDevicesAsync();
        Task<bool> SetDeletedAsync(string deviceId, bool deleted);
    }
}
