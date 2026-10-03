using System;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信日志控件使用的异步读写接口。</summary>
    public interface ICommunicationLogService
    {
        Task AddEntryAsync(DateTime timestamp, string deviceId, string direction, string operation, string result, string details);
        Task<DataTable> GetRecentEntriesAsync(int limit);
    }
}
