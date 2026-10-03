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
        private readonly IModbusCommunicationService modbusService;
        private readonly Timer samplingTimer = new Timer();
        private readonly Timer reconnectTimer = new Timer();
        private CommunicationSettings activeSettings;
        private string activeDeviceId;
        private bool pollInProgress;
        private bool isReconnecting;
        private bool reconnectAttemptInProgress;
        private bool reconnectPromptShown;
        private int reconnectAttempts;
        private DateTime reconnectDeadline;
        private DateTime nextReconnectAttempt;
        private int connectionVersion;

        private sealed class OperationResult<T>
        {
            public T Value;
            public Exception Error;
        }

        /// <summary>成功采样后发出，供首页保存采样并更新曲线和报警。</summary>
        public event Action<SensorReading> ReadingProduced;
        public event Action<string> CommunicationStopped;

        public NetworkCommunicationPage() : this(new ModbusCommunicationService(), new CommunicationLogService())
        {
        }

        public NetworkCommunicationPage(IModbusCommunicationService modbusService)
            : this(modbusService, new CommunicationLogService())
        {
        }

        public NetworkCommunicationPage(IModbusCommunicationService modbusService, ICommunicationLogService communicationLogService)
        {
            if (modbusService == null) throw new ArgumentNullException("modbusService");
            if (communicationLogService == null) throw new ArgumentNullException("communicationLogService");
            this.modbusService = modbusService;
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
            logControl = new CommunicationLogControl(communicationLogService) { Dock = DockStyle.Fill, Margin = new Padding(6, 6, 0, 0) };

            layout.Controls.Add(configControl, 0, 0);
            layout.Controls.Add(statusControl, 1, 0);
            layout.Controls.Add(mappingControl, 0, 1);
            layout.Controls.Add(logControl, 1, 1);
            Controls.Add(layout);

            statusControl.ConnectRequested += ConnectCommunication;
            statusControl.DisconnectRequested += DisconnectCommunication;
            statusControl.TestReadRequested += TestRead;
            samplingTimer.Tick += SamplingTimer_Tick;
            reconnectTimer.Interval = 1000;
            reconnectTimer.Tick += ReconnectTimer_Tick;
            Disposed += async (sender, e) =>
            {
                samplingTimer.Stop();
                reconnectTimer.Stop();
                try { await modbusService.DisconnectAsync(); }
                catch { }
            };
        }

        private async void ConnectCommunication(object sender, EventArgs e)
        {
            int version = -1;
            try
            {
                await StopCurrentCommunication(false);
                version = connectionVersion;
                configControl.RefreshDevices();
                activeSettings = configControl.GetSettings(mappingControl);
                activeDeviceId = activeSettings.DeviceId;
                samplingTimer.Interval = activeSettings.SamplingInterval;

                if (activeSettings.Mode == CommunicationMode.Simulation)
                {
                    statusControl.SetRunning("模拟", activeDeviceId);
                    logControl.AddEntry("系统", "启动模拟数据", "成功", "设备 " + activeDeviceId
                        + "，采样间隔 " + samplingTimer.Interval + " ms", activeDeviceId);
                    SensorReading firstReading = simulator.NextReading(activeDeviceId);
                    PublishReading(firstReading, "模拟数据");
                    samplingTimer.Start();
                    return;
                }

                string modeName = activeSettings.Mode == CommunicationMode.ModbusRtu ? "Modbus RTU" : "Modbus TCP";
                statusControl.SetConnecting(modeName);
                logControl.AddEntry("系统", "连接 " + modeName, "进行中", DescribeConnection(activeSettings), activeDeviceId);

                CommunicationSettings settingsToConnect = activeSettings;
                OperationResult<bool> connectResult = await ExecuteSafelyAsync(async delegate
                {
                    await modbusService.ConnectAsync(settingsToConnect);
                    return true;
                });
                if (version != connectionVersion || IsDisposed)
                {
                    await DisconnectServiceAsync();
                    return;
                }

                if (connectResult.Error != null)
                {
                    statusControl.RecordFailure();
                    logControl.AddEntry("系统", "连接 " + modeName, "失败", connectResult.Error.Message, activeDeviceId);
                    await StopCurrentCommunication(false);
                    MessageBox.Show(this.FindForm(), "连接 " + modeName + " 失败：\r\n" + connectResult.Error.Message,
                        "通信连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                statusControl.SetRunning(modeName, activeDeviceId);
                logControl.AddEntry("系统", "连接 " + modeName, "成功", DescribeConnection(activeSettings), activeDeviceId);
                samplingTimer.Start();
                await PollModbusOnce(version);
            }
            catch (Exception ex)
            {
                if (version >= 0 && version != connectionVersion)
                    return;

                statusControl.RecordFailure();
                logControl.AddEntry("系统", "连接/采样", "失败", ex.Message, activeDeviceId);
                await StopCurrentCommunication(false);
                if (!IsDisposed)
                    MessageBox.Show(this.FindForm(), "连接或启动采样失败：\r\n" + ex.Message,
                        "通信失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                logControl.AddEntry("系统", "断开连接", "失败", ex.Message, activeDeviceId);
            }
        }

        private async void TestRead(object sender, EventArgs e)
        {
            if (isReconnecting)
                return;

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
                        logControl.AddEntry("模拟器", "单次读取", "失败", ex.Message, activeDeviceId);
                    }
                    return;
                }

                await PollModbusOnce(connectionVersion);
                return;
            }

            string oneOffDeviceId = null;
            try
            {
                configControl.RefreshDevices();
                CommunicationSettings settings = configControl.GetSettings(mappingControl);
                oneOffDeviceId = settings.DeviceId;
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
                logControl.AddEntry("系统", "单次读取", "失败", ex.Message, oneOffDeviceId);
            }
        }

        private async void SamplingTimer_Tick(object sender, EventArgs e)
        {
            if (activeSettings == null || pollInProgress || isReconnecting)
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
                    logControl.AddEntry("模拟器", "周期采样", "失败", ex.Message, activeDeviceId);
                    await StopCurrentCommunication(false);
                }
                return;
            }

            await PollModbusOnce(connectionVersion);
        }

        private async Task PollModbusOnce(int version)
        {
            if (pollInProgress || isReconnecting || activeSettings == null
                || activeSettings.Mode == CommunicationMode.Simulation)
                return;

            pollInProgress = true;
            try
            {
                OperationResult<SensorReading> readResult = await ExecuteSafelyAsync(
                    () => modbusService.ReadReadingAsync());
                if (version != connectionVersion || activeSettings == null || IsDisposed)
                    return;

                if (readResult.Error != null)
                {
                    statusControl.RecordFailure();
                    await BeginReconnect(readResult.Error);
                    return;
                }

                SensorReading reading = readResult.Value;
                PublishReading(reading, activeSettings.Mode == CommunicationMode.ModbusRtu ? "Modbus RTU" : "Modbus TCP");
            }
            catch (Exception ex)
            {
                if (version == connectionVersion && !IsDisposed)
                {
                    statusControl.RecordFailure();
                    await BeginReconnect(ex);
                }
            }
            finally
            {
                pollInProgress = false;
            }
        }

        private async Task BeginReconnect(Exception error)
        {
            if (activeSettings == null || activeSettings.Mode == CommunicationMode.Simulation || isReconnecting)
                return;

            samplingTimer.Stop();
            isReconnecting = true;
            reconnectPromptShown = false;
            reconnectAttempts = 0;
            reconnectDeadline = DateTime.Now.AddSeconds(30);
            nextReconnectAttempt = DateTime.Now.AddSeconds(5);
            int version = ++connectionVersion;

            statusControl.SetReconnecting(activeDeviceId, 0, 30);
            logControl.AddEntry("系统", "通信中断", "失败", "设备 " + activeDeviceId + " | " + error.Message, activeDeviceId);

            await DisconnectServiceAsync();
            if (version == connectionVersion && isReconnecting && activeSettings != null && !IsDisposed)
                reconnectTimer.Start();
        }

        private async void ReconnectTimer_Tick(object sender, EventArgs e)
        {
            if (!isReconnecting || activeSettings == null || IsDisposed)
                return;

            DateTime now = DateTime.Now;
            if (now >= reconnectDeadline)
            {
                await FinishReconnectTimeout();
                return;
            }

            if (reconnectAttemptInProgress || now < nextReconnectAttempt)
                return;

            int remainingMilliseconds = (int)(reconnectDeadline - now).TotalMilliseconds;
            if (remainingMilliseconds < 300)
            {
                await FinishReconnectTimeout();
                return;
            }

            reconnectAttemptInProgress = true;
            int version = connectionVersion;
            int attempt = ++reconnectAttempts;
            int attemptTimeout = System.Math.Max(100,
                System.Math.Min(activeSettings.Timeout, remainingMilliseconds / 3));
            CommunicationSettings retrySettings = CopySettings(activeSettings, attemptTimeout);
            statusControl.SetReconnecting(activeDeviceId, attempt,
                System.Math.Max(0, (int)System.Math.Ceiling((reconnectDeadline - now).TotalSeconds)));

            try
            {
                OperationResult<SensorReading> result = await ExecuteSafelyAsync(async delegate
                {
                    await modbusService.ConnectAsync(retrySettings);
                    return await modbusService.ReadReadingAsync();
                });

                if (version != connectionVersion || !isReconnecting || IsDisposed)
                {
                    await DisconnectServiceAsync();
                    return;
                }

                if (result.Error != null)
                {
                    await DisconnectServiceAsync();
                    if (version != connectionVersion || !isReconnecting || IsDisposed)
                        return;

                    statusControl.RecordFailure();
                    logControl.AddEntry("系统", "重连尝试 " + attempt, "失败", result.Error.Message, activeDeviceId);
                    if (DateTime.Now >= reconnectDeadline)
                    {
                        await FinishReconnectTimeout();
                        return;
                    }

                    nextReconnectAttempt = DateTime.Now.AddSeconds(5);
                    statusControl.SetReconnecting(activeDeviceId, attempt,
                        System.Math.Max(0, (int)System.Math.Ceiling((reconnectDeadline - DateTime.Now).TotalSeconds)));
                    return;
                }

                if (DateTime.Now >= reconnectDeadline)
                {
                    await DisconnectServiceAsync();
                    await FinishReconnectTimeout();
                    return;
                }

                reconnectTimer.Stop();
                isReconnecting = false;
                string modeName = activeSettings.Mode == CommunicationMode.ModbusRtu ? "Modbus RTU" : "Modbus TCP";
                statusControl.SetRunning(modeName, activeDeviceId);
                logControl.AddEntry("系统", "通信恢复", "成功", "设备 " + activeDeviceId + " | " + modeName, activeDeviceId);
                PublishReading(result.Value, modeName);
                samplingTimer.Start();
            }
            catch (Exception ex)
            {
                if (version == connectionVersion && isReconnecting && !IsDisposed)
                {
                    await DisconnectServiceAsync();
                    statusControl.RecordFailure();
                    logControl.AddEntry("系统", "重连尝试 " + attempt, "失败", ex.Message, activeDeviceId);
                    if (DateTime.Now >= reconnectDeadline)
                        await FinishReconnectTimeout();
                    else
                    {
                        nextReconnectAttempt = DateTime.Now.AddSeconds(5);
                        statusControl.SetReconnecting(activeDeviceId, attempt,
                            System.Math.Max(0, (int)System.Math.Ceiling((reconnectDeadline - DateTime.Now).TotalSeconds)));
                    }
                }
            }
            finally
            {
                reconnectAttemptInProgress = false;
            }
        }

        private async Task FinishReconnectTimeout()
        {
            if (!isReconnecting || activeSettings == null || reconnectPromptShown)
                return;

            reconnectPromptShown = true;
            string timedOutDevice = activeDeviceId;
            reconnectTimer.Stop();
            isReconnecting = false;
            logControl.AddEntry("系统", "重连超时", "失败", "设备 " + timedOutDevice + " 超过 30 秒未恢复，已停止采样并断开连接。", timedOutDevice);
            await StopCurrentCommunication(false, "重连超时，已停止采样并断开连接。");

            if (!IsDisposed)
                MessageBox.Show(this.FindForm(), "设备 " + timedOutDevice
                    + " 在 30 秒内未恢复通信，程序已停止采样并断开连接。",
                    "通信重连超时", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private async Task DisconnectServiceAsync()
        {
            await ExecuteSafelyAsync(async delegate
            {
                await modbusService.DisconnectAsync();
                return true;
            });
        }

        private static async Task<OperationResult<T>> ExecuteSafelyAsync<T>(Func<Task<T>> operation)
        {
            var result = new OperationResult<T>();
            try { result.Value = await operation(); }
            catch (Exception ex) { result.Error = ex; }
            return result;
        }

        private static CommunicationSettings CopySettings(CommunicationSettings source, int timeout)
        {
            return new CommunicationSettings
            {
                Mode = source.Mode,
                DeviceId = source.DeviceId,
                SamplingInterval = source.SamplingInterval,
                SerialPortName = source.SerialPortName,
                BaudRate = source.BaudRate,
                DataBits = source.DataBits,
                Parity = source.Parity,
                StopBits = source.StopBits,
                IpAddress = source.IpAddress,
                TcpPort = source.TcpPort,
                UnitId = source.UnitId,
                Timeout = timeout,
                Function = source.Function,
                TemperatureAddress = source.TemperatureAddress,
                PressureAddress = source.PressureAddress,
                TemperatureScale = source.TemperatureScale,
                PressureScale = source.PressureScale,
                SignedValues = source.SignedValues
            };
        }

        private void PublishReading(SensorReading reading, string source)
        {
            Action<SensorReading> handler = ReadingProduced;
            if (handler != null)
                handler(reading);

            statusControl.RecordSuccess(reading.Timestamp);
            logControl.AddEntry("接收", "读取温度/压力", "成功",
                source + " | " + reading.DeviceId + " | 温度 " + reading.Temperature.ToString("F2")
                + " °C，压力 " + reading.Pressure.ToString("F2") + " MPa", reading.DeviceId);
        }

        private async Task StopCurrentCommunication(bool writeLog, string stoppedMessage = "当前未连接")
        {
            samplingTimer.Stop();
            reconnectTimer.Stop();
            isReconnecting = false;
            connectionVersion++;
            string stoppedDevice = activeDeviceId;
            CommunicationSettings stoppedSettings = activeSettings;
            activeDeviceId = null;
            activeSettings = null;

            if (stoppedSettings != null && stoppedSettings.Mode != CommunicationMode.Simulation)
                await DisconnectServiceAsync();

            if (!string.IsNullOrWhiteSpace(stoppedDevice))
            {
                if (writeLog)
                    logControl.AddEntry("系统", "断开连接", "成功", "设备 " + stoppedDevice, stoppedDevice);
                NotifyCommunicationStopped(stoppedDevice);
            }
            statusControl.SetStopped(stoppedMessage);
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
