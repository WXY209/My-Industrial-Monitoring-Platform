using System;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>历史采样与报警页面使用的查询接口。</summary>
    public interface IRecordService
    {
        Task<DataTable> GetActiveDevicesAsync();
        Task<int> GetHistoryCountAsync(DateTime start, DateTime endExclusive, string deviceId);
        Task<DataTable> GetHistoryPageAsync(DateTime start, DateTime endExclusive, string deviceId, int page, int pageSize);
        Task<int> GetAlarmCountAsync(string alarmType);
        Task<DataTable> GetAlarmPageAsync(string alarmType, int page, int pageSize);
    }
}
