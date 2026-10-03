using System;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>在后台串行保存首页产生的报警，避免 SQLite 操作阻塞界面。</summary>
    public sealed class AlarmPersistenceService : IAlarmPersistenceService
    {
        public async Task<int> AddAsync(SensorReading reading, string alarmType, double temperatureThreshold, double pressureThreshold)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            return await DatabaseWriteQueue.ExecuteAsync(
                () => AlarmDB.AddAlarm(reading, alarmType, temperatureThreshold, pressureThreshold));
        }

        public async Task CloseAsync(int id, DateTime endTime)
        {
            await DatabaseWriteQueue.ExecuteAsync(() => AlarmDB.CloseAlarm(id, endTime));
        }
    }
}
