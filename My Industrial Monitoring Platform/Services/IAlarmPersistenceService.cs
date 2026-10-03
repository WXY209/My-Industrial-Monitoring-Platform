using System;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页报警记录的异步持久化接口。</summary>
    public interface IAlarmPersistenceService
    {
        Task<int> AddAsync(SensorReading reading, string alarmType, double temperatureThreshold, double pressureThreshold);
        Task CloseAsync(int id, DateTime endTime);
    }
}
