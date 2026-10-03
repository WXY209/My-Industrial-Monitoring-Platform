using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>后台执行设备新增、软删除和恢复操作。</summary>
    public sealed class DeviceManagementService : IDeviceManagementService
    {
        public async Task<string> AddNextDeviceAsync()
        {
            return await DatabaseWriteQueue.ExecuteAsync(() =>
            {
                string deviceId = DeviceDB.GetNextDeviceId();
                return DeviceDB.AddDevice(deviceId, deviceId) ? deviceId : null;
            });
        }

        public Task<int> GetActiveCountAsync()
        {
            return Task.Run(() => DeviceDB.GetActiveCount());
        }

        public Task<DataTable> GetDeletedDevicesAsync()
        {
            return Task.Run(() => DeviceDB.GetDeletedDevices());
        }

        public Task<bool> SetDeletedAsync(string deviceId, bool deleted)
        {
            return DatabaseWriteQueue.ExecuteAsync(() => DeviceDB.SetDeleted(deviceId, deleted));
        }
    }
}
