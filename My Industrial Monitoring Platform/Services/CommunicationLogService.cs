using System;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信日志服务：后台执行 SQLite 操作，并顺序写入日志。</summary>
    public sealed class CommunicationLogService : ICommunicationLogService
    {
        public async Task AddEntryAsync(DateTime timestamp, string deviceId, string direction, string operation, string result, string details)
        {
            await DatabaseWriteQueue.ExecuteAsync(
                () => CommunicationLogDB.AddEntry(timestamp, deviceId, direction, operation, result, details));
        }

        public Task<DataTable> GetRecentEntriesAsync(int limit)
        {
            return Task.Run(() => CommunicationLogDB.GetRecentEntries(limit));
        }
    }
}
