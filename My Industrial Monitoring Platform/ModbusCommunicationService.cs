using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Modbus.Device;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>使用 NModbus4 连接 Modbus RTU/TCP 并读取温度、压力寄存器。</summary>
    public sealed class ModbusCommunicationService : IModbusCommunicationService
    {
        private readonly object syncRoot = new object();
        private IModbusMaster master;
        private TcpClient tcpClient;
        private SerialPort serialPort;
        private CommunicationSettings settings;

        public Task ConnectAsync(CommunicationSettings connectionSettings)
        {
            return Task.Run(() => ConnectCore(connectionSettings));
        }

        private void ConnectCore(CommunicationSettings connectionSettings)
        {
            if (connectionSettings == null)
                throw new ArgumentNullException("connectionSettings");
            if (connectionSettings.Mode == CommunicationMode.Simulation)
                throw new ArgumentException("模拟模式不需要创建 Modbus 连接。", "connectionSettings");

            Disconnect();
            lock (syncRoot)
            {
                settings = connectionSettings;
                try
                {
                    if (settings.Mode == CommunicationMode.ModbusTcp)
                        ConnectTcp();
                    else
                        ConnectRtu();

                    master.Transport.ReadTimeout = settings.Timeout;
                    master.Transport.WriteTimeout = settings.Timeout;
                }
                catch
                {
                    DisconnectResources();
                    throw;
                }
            }
        }

        public Task<SensorReading> ReadReadingAsync()
        {
            return Task.Run(() => ReadReadingCore());
        }

        private SensorReading ReadReadingCore()
        {
            lock (syncRoot)
            {
                if (master == null || settings == null)
                    throw new InvalidOperationException("Modbus 尚未连接。");

                ushort temperatureRaw = ReadRegister(settings.TemperatureAddress);
                ushort pressureRaw = ReadRegister(settings.PressureAddress);
                double temperature = ConvertRaw(temperatureRaw, settings.SignedValues) * settings.TemperatureScale;
                double pressure = ConvertRaw(pressureRaw, settings.SignedValues) * settings.PressureScale;

                return new SensorReading(settings.DeviceId, DateTime.Now, Math.Round(temperature, 3), Math.Round(pressure, 3));
            }
        }

        public Task DisconnectAsync()
        {
            return Task.Run(() => Disconnect());
        }

        public void Disconnect()
        {
            lock (syncRoot)
                DisconnectResources();
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void ConnectTcp()
        {
            IPAddress address;
            if (!IPAddress.TryParse(settings.IpAddress, out address))
                throw new ArgumentException("设备 IP 地址格式不正确。");

            tcpClient = new TcpClient();
            IAsyncResult connectResult = tcpClient.BeginConnect(address, settings.TcpPort, null, null);
            try
            {
                if (!connectResult.AsyncWaitHandle.WaitOne(settings.Timeout))
                    throw new TimeoutException("连接 Modbus TCP 设备超时。");
                tcpClient.EndConnect(connectResult);
            }
            finally
            {
                connectResult.AsyncWaitHandle.Close();
            }

            tcpClient.ReceiveTimeout = settings.Timeout;
            tcpClient.SendTimeout = settings.Timeout;
            master = ModbusIpMaster.CreateIp(tcpClient);
        }

        private void ConnectRtu()
        {
            serialPort = new SerialPort(
                settings.SerialPortName,
                settings.BaudRate,
                settings.Parity,
                settings.DataBits,
                settings.StopBits)
            {
                ReadTimeout = settings.Timeout,
                WriteTimeout = settings.Timeout,
                Handshake = Handshake.None
            };
            serialPort.Open();
            master = ModbusSerialMaster.CreateRtu(serialPort);
        }

        private ushort ReadRegister(ushort address)
        {
            ushort[] result = settings.Function == RegisterFunction.InputRegister
                ? master.ReadInputRegisters(settings.UnitId, address, 1)
                : master.ReadHoldingRegisters(settings.UnitId, address, 1);
            if (result == null || result.Length != 1)
                throw new InvalidOperationException("设备未返回寄存器数据。");
            return result[0];
        }

        private static double ConvertRaw(ushort raw, bool signed)
        {
            return signed ? (double)unchecked((short)raw) : raw;
        }

        private void DisconnectResources()
        {
            IModbusMaster oldMaster = master;
            master = null;
            if (oldMaster != null)
            {
                try { oldMaster.Dispose(); }
                catch { }
            }

            TcpClient oldTcpClient = tcpClient;
            tcpClient = null;
            if (oldTcpClient != null)
            {
                try { oldTcpClient.Close(); }
                catch { }
            }

            SerialPort oldSerialPort = serialPort;
            serialPort = null;
            if (oldSerialPort != null)
            {
                try
                {
                    if (oldSerialPort.IsOpen)
                        oldSerialPort.Close();
                    oldSerialPort.Dispose();
                }
                catch { }
            }

            settings = null;
        }
    }
}
