using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页初始数据服务：在后台执行 SQLite 读取和默认设备检查。</summary>
    public sealed class HomeOverviewDataService : IHomeOverviewDataService
    {
        public Task<IList<string>> GetActiveDeviceIdsAsync()
        {
            return Task.Run<IList<string>>(() =>
            {
                DataTable table = DeviceDB.GetActiveDevices();
                var ids = new List<string>();
                foreach (DataRow row in table.Rows) ids.Add(row["DeviceId"].ToString());
                if (ids.Count == 0)
                {
                    string id = DeviceDB.GetNextDeviceId();
                    DeviceDB.AddDevice(id, id);
                    ids.Add(id);
                }
                return ids;
            });
        }

        public Task<List<SensorReading>> GetRecentReadingsAsync(string deviceId, int limit)
        {
            return Task.Run(() => ReadingDB.GetRecentReadings(deviceId, limit));
        }

        public Task<DataTable> GetLatestAlarmsAsync(int limit)
        {
            return Task.Run(() => AlarmDB.GetLatestAlarms(limit));
        }
    }
}
