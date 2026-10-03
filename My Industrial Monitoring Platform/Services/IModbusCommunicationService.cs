using System;
using System.Threading.Tasks;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>网络通讯页面依赖的 Modbus 连接接口。</summary>
    public interface IModbusCommunicationService : IDisposable
    {
        Task ConnectAsync(CommunicationSettings connectionSettings);
        Task<SensorReading> ReadReadingAsync();
        Task DisconnectAsync();
    }
}
