using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信配置、实际 Modbus RTU/TCP 连接及项目模拟模式。</summary>
    public sealed class NetworkCommunicationPage : UserControl
    {
        private readonly CommunicationConfigControl configControl;
        private readonly CommunicationStatusControl statusControl;
        private readonly RegisterMappingControl mappingControl;
        private readonly CommunicationLogControl logControl;
        private readonly MonitoringSimulator simulator = new MonitoringSimulator();
        private readonly ModbusCommunicationService modbusService = new ModbusCommunicationService();
        private readonly Timer samplingTimer = new Timer();
        private CommunicationSettings activeSettings;
        private string activeDeviceId;
        private bool pollInProgress;
        private int connectionVersion;

        /// <summary>成功采样后发出，供首页保存采样并更新曲线和报警。</summary>
        public event Action<SensorReading> ReadingProduced;
        public event Action<string> CommunicationStopped;

        public NetworkCommunicationPage()
        {
            BackColor = Color.FromArgb(241, 245, 249);
            Padding = new Padding(10);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = BackColor,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 47F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 53F));

            configControl = new CommunicationConfigControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 6) };
            statusControl = new CommunicationStatusControl { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 6) };
            mappingControl = new RegisterMappingControl { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 6, 0) };
            logControl = new CommunicationLogControl { Dock = DockStyle.Fill, Margin = new Padding(6, 6, 0, 0) };

            layout.Controls.Add(configControl, 0, 0);
            layout.Controls.Add(statusControl, 1, 0);
            layout.Controls.Add(mappingControl, 0, 1);
            layout.Controls.Add(logControl, 1, 1);
            Controls.Add(layout);

            statusControl.ConnectRequested += ConnectCommunication;
            statusControl.DisconnectRequested += DisconnectCommunication;
            statusControl.TestReadRequested += TestRead;
            samplingTimer.Tick += SamplingTimer_Tick;
            Disposed += (sender, e) =>
            {
                samplingTimer.Stop();
                modbusService.Dispose();
            };
        }

        private async void ConnectCommunication(object sender, EventArgs e)
        {
            try
            {
                await StopCurrentCommunication(false);
                configControl.RefreshDevices();
                activeSettings = configControl.GetSettings(mappingControl);
                activeDeviceId = activeSettings.DeviceId;
                samplingTimer.Interval = activeSettings.SamplingInterval;

                if (activeSettings.Mode == CommunicationMode.Simulation)
                {
                    statusControl.SetRunning("模拟", activeDeviceId);
                    logControl.AddEntry("系统", "启动模拟数据", "成功", "设备 " + activeDeviceId
                        + "，采样间隔 " + samplingTimer.Interval + " ms");
                    SensorReading firstReading = simulator.NextReading(activeDeviceId);
                    PublishReading(firstReading, "模拟数据");
                    samplingTimer.Start();
                    return;
                }

                int version = connectionVersion;
                string modeName = activeSettings.Mode == CommunicationMode.ModbusRtu ? "Modbus RTU" : "Modbus TCP";
                statusControl.SetConnecting(modeName);
                logControl.AddEntry("系统", "连接 " + modeName, "进行中", DescribeConnection(activeSettings));

                CommunicationSettings settingsToConnect = activeSettings;
                await Task.Run(() => modbusService.Connect(settingsToConnect));
                if (version != connectionVersion || IsDisposed)
                {
                    await Task.Run(() => modbusService.Disconnect());
                    return;
                }

                statusControl.SetRunning(modeName, activeDeviceId);
                logControl.AddEntry("系统", "连接 " + modeName, "成功", DescribeConnection(activeSettings));
                samplingTimer.Start();
                await PollModbusOnce(version);
            }
            catch (Exception ex)
            {
                statusControl.RecordFailure();
                logControl.AddEntry("系统", "连接/采样", "失败", ex.Message);
                await StopCurrentCommunication(false);
            }
        }

        private async void DisconnectCommunication(object sender, EventArgs e)
        {
            try
            {
                await StopCurrentCommunication(true);
            }
            catch (Exception ex)
            {
                statusControl.RecordFailure();
                logControl.AddEntry("系统", "断开连接", "失败", ex.Message);
            }
        }

        private async void TestRead(object sender, EventArgs e)
        {
            if (activeSettings != null)
            {
                if (activeSettings.Mode == CommunicationMode.Simulation)
                {
                    try
                    {
                        SensorReading reading = simulator.NextReading(activeDeviceId);
                        PublishReading(reading, "模拟数据");
                    }
                    catch (Exception ex)
                    {
                        statusControl.RecordFailure();
                        logControl.AddEntry("模拟器", "单次读取", "失败", ex.Message);
                    }
                    return;
                }

                await PollModbusOnce(connectionVersion);
                return;
            }

            try
            {
                configControl.RefreshDevices();
                CommunicationSettings settings = configControl.GetSettings(mappingControl);
                if (settings.Mode != CommunicationMode.Simulation)
                    throw new InvalidOperationException("请先点击“连接”建立 RTU/TCP 连接，再进行单次读取。");

                try
                {
                    SensorReading reading = simulator.NextReading(settings.DeviceId);
                    PublishReading(reading, "模拟数据");
                }
                finally
                {
                    NotifyCommunicationStopped(settings.DeviceId);
                }
            }
            catch (Exception ex)
            {
                statusControl.RecordFailure();
                logControl.AddEntry("系统", "单次读取", "失败", ex.Message);
            }
        }

        private async void SamplingTimer_Tick(object sender, EventArgs e)
        {
            if (activeSettings == null || pollInProgress)
                return;

            if (activeSettings.Mode == CommunicationMode.Simulation)
            {
                try
                {
                    SensorReading reading = simulator.NextReading(activeDeviceId);
                    PublishReading(reading, "模拟数据");
                }
                catch (Exception ex)
                {
                    statusControl.RecordFailure();
                    logControl.AddEntry("模拟器", "周期采样", "失败", ex.Message);
                    await StopCurrentCommunication(false);
                }
                return;
            }

            await PollModbusOnce(connectionVersion);
        }

        private async Task PollModbusOnce(int version)
        {
            if (pollInProgress || activeSettings == null || activeSettings.Mode == CommunicationMode.Simulation)
                return;

            pollInProgress = true;
            try
            {
                SensorReading reading = await Task.Run(() => modbusService.ReadReading());
                if (version != connectionVersion || activeSettings == null || IsDisposed)
                    return;
                PublishReading(reading, activeSettings.Mode == CommunicationMode.ModbusRtu ? "Modbus RTU" : "Modbus TCP");
            }
            catch (Exception ex)
            {
                if (version == connectionVersion && !IsDisposed)
                {
                    statusControl.RecordFailure();
                    logControl.AddEntry("接收", "读取温度/压力寄存器", "失败", ex.Message);
                    await StopCurrentCommunication(false);
                }
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private void PublishReading(SensorReading reading, string source)
        {
            Action<SensorReading> handler = ReadingProduced;
            if (handler != null)
                handler(reading);

            statusControl.RecordSuccess(reading.Timestamp);
            logControl.AddEntry("接收", "读取温度/压力", "成功",
                source + " | " + reading.DeviceId + " | 温度 " + reading.Temperature.ToString("F2")
                + " °C，压力 " + reading.Pressure.ToString("F2") + " MPa");
        }

        private async Task StopCurrentCommunication(bool writeLog)
        {
            samplingTimer.Stop();
            connectionVersion++;
            string stoppedDevice = activeDeviceId;
            CommunicationSettings stoppedSettings = activeSettings;
            activeDeviceId = null;
            activeSettings = null;

            if (stoppedSettings != null && stoppedSettings.Mode != CommunicationMode.Simulation)
                await Task.Run(() => modbusService.Disconnect());

            if (!string.IsNullOrWhiteSpace(stoppedDevice))
            {
                if (writeLog)
                    logControl.AddEntry("系统", "断开连接", "成功", "设备 " + stoppedDevice);
                NotifyCommunicationStopped(stoppedDevice);
            }
            statusControl.SetStopped();
        }

        private void NotifyCommunicationStopped(string deviceId)
        {
            Action<string> handler = CommunicationStopped;
            if (handler != null)
                handler(deviceId);
        }

        private static string DescribeConnection(CommunicationSettings settings)
        {
            if (settings.Mode == CommunicationMode.ModbusRtu)
                return settings.DeviceId + " | " + settings.SerialPortName + " | " + settings.BaudRate
                    + ", " + settings.DataBits + ", " + settings.Parity + ", " + settings.StopBits
                    + " | 从站 " + settings.UnitId;
            return settings.DeviceId + " | " + settings.IpAddress + ":" + settings.TcpPort
                + " | Unit ID " + settings.UnitId;
        }
    }
}
