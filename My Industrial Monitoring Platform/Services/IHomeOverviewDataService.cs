using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页启动时加载设备、曲线缓存和报警记录的数据接口。</summary>
    public interface IHomeOverviewDataService
    {
        Task<IList<string>> GetActiveDeviceIdsAsync();
        Task<List<SensorReading>> GetRecentReadingsAsync(string deviceId, int limit);
        Task<DataTable> GetLatestAlarmsAsync(int limit);
    }
}
