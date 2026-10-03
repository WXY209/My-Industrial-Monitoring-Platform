using System;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>记录查询服务：在线程池中执行现有同步 SQLite 查询。</summary>
    public sealed class RecordService : IRecordService
    {
        public Task<DataTable> GetActiveDevicesAsync()
        {
            return Task.Run(() => DeviceDB.GetActiveDevices());
        }

        public Task<int> GetHistoryCountAsync(DateTime start, DateTime endExclusive, string deviceId)
        {
            return Task.Run(() => ReadingDB.GetHistoryCount(start, endExclusive, deviceId));
        }

        public Task<DataTable> GetHistoryPageAsync(DateTime start, DateTime endExclusive, string deviceId, int page, int pageSize)
        {
            return Task.Run(() => ReadingDB.GetHistoryPage(start, endExclusive, deviceId, page, pageSize));
        }

        public Task<int> GetAlarmCountAsync(string alarmType)
        {
            return Task.Run(() => AlarmDB.GetAlarmCount(alarmType));
        }

        public Task<DataTable> GetAlarmPageAsync(string alarmType, int page, int pageSize)
        {
            return Task.Run(() => AlarmDB.GetAlarmPage(alarmType, page, pageSize));
        }
    }
}
