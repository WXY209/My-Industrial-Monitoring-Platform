using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页采样数据持久化接口。</summary>
    public interface IReadingPersistenceService
    {
        Task SaveAsync(SensorReading reading);
    }
}
