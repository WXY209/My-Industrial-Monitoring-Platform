using System;
using System.Threading;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>集中排队执行 SQLite 写操作，避免不同服务并发争用同一个数据库文件。</summary>
    internal static class DatabaseWriteQueue
    {
        private static readonly SemaphoreSlim writeGate = new SemaphoreSlim(1, 1);

        public static async Task<T> ExecuteAsync<T>(Func<T> operation)
        {
            if (operation == null) throw new ArgumentNullException("operation");

            await writeGate.WaitAsync();
            try
            {
                return await Task.Run(operation);
            }
            finally
            {
                writeGate.Release();
            }
        }

        public static async Task ExecuteAsync(Action operation)
        {
            if (operation == null) throw new ArgumentNullException("operation");
            await ExecuteAsync(() =>
            {
                operation();
                return true;
            });
        }
    }
}
