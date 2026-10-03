using System;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>将采样数据按调用顺序串行写入 SQLite，避免阻塞界面并发访问数据库。</summary>
    public sealed class ReadingPersistenceService : IReadingPersistenceService
    {
        public async Task SaveAsync(SensorReading reading)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            await DatabaseWriteQueue.ExecuteAsync(() => ReadingDB.AddReading(reading));
        }
    }
}
