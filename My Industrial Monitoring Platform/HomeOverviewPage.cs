using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 首页总览 / 实时监控界面，使用模拟设备数据动态更新曲线。
    /// </summary>
    public sealed class HomeOverviewPage : UserControl
    {
        private sealed class AlarmRecord
        {
            public int Id;
            public SensorReading Reading;
            public string Reason;
            public string Peak;
            public DateTime StartTime;
            public DateTime? EndTime;
            public Task<int> PersistenceTask;
        }

        private const int MaximumAlarmRecords = 75;
        private readonly Color pageBackground = Color.FromArgb(241, 245, 249);
        private readonly MonitoringSimulator simulator = new MonitoringSimulator();
        private readonly IReadingPersistenceService readingPersistenceService;
        private readonly IAlarmPersistenceService alarmPersistenceService;
        private readonly IHomeOverviewDataService homeDataService;
        private readonly IDeviceManagementService deviceManagementService;
        private readonly Timer simulationTimer = new Timer { Interval = 1000 };
        private readonly Dictionary<string, List<SensorReading>> readingsByDevice =
            new Dictionary<string, List<SensorReading>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> loadedReadingDevices =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Task> readingLoadsInProgress =
            new Dictionary<string, Task>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> runningDevices =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> communicationDevices =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, bool> alarmStateByDevice =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AlarmRecord> activeAlarmsByDevice =
            new Dictionary<string, AlarmRecord>(StringComparer.OrdinalIgnoreCase);
        private readonly List<AlarmRecord> alarmRecords = new List<AlarmRecord>();
        private readonly List<string> devices = new List<string> { "DEV-001" };
        private OverviewChartControl overviewChartControl;
        private OverviewAlarmControl overviewAlarmControl;
        private OverviewToolbarControl overviewToolbarControl;
        private string selectedDevice = "DEV-001";
        private bool resetAlarmPagePending;

        public HomeOverviewPage() : this(new ReadingPersistenceService(), new AlarmPersistenceService(), new HomeOverviewDataService(), new DeviceManagementService())
        {
        }

        public HomeOverviewPage(IReadingPersistenceService readingPersistenceService)
            : this(readingPersistenceService, new AlarmPersistenceService(), new HomeOverviewDataService())
        {
        }

        public HomeOverviewPage(IReadingPersistenceService readingPersistenceService, IHomeOverviewDataService homeDataService)
            : this(readingPersistenceService, new AlarmPersistenceService(), homeDataService, new DeviceManagementService())
        {
        }

        public HomeOverviewPage(IReadingPersistenceService readingPersistenceService, IAlarmPersistenceService alarmPersistenceService, IHomeOverviewDataService homeDataService)
            : this(readingPersistenceService, alarmPersistenceService, homeDataService, new DeviceManagementService())
        {
        }

        public HomeOverviewPage(IReadingPersistenceService readingPersistenceService, IAlarmPersistenceService alarmPersistenceService, IHomeOverviewDataService homeDataService, IDeviceManagementService deviceManagementService)
        {
            if (readingPersistenceService == null) throw new ArgumentNullException("readingPersistenceService");
            if (alarmPersistenceService == null) throw new ArgumentNullException("alarmPersistenceService");
            if (homeDataService == null) throw new ArgumentNullException("homeDataService");
            if (deviceManagementService == null) throw new ArgumentNullException("deviceManagementService");
            this.readingPersistenceService = readingPersistenceService;
            this.alarmPersistenceService = alarmPersistenceService;
            this.homeDataService = homeDataService;
            this.deviceManagementService = deviceManagementService;
            BackColor = pageBackground;
            Padding = new Padding(10);
            BuildLayout();
            overviewToolbarControl.StartClicked += StartMonitoring_Click;
            overviewToolbarControl.StopClicked += StopMonitoring_Click;
            overviewToolbarControl.AddDeviceClicked += AddDevice_Click;
            overviewToolbarControl.DeleteDeviceClicked += DeleteDevice_Click;
            overviewToolbarControl.RestoreDeviceClicked += RestoreDevice_Click;
            overviewToolbarControl.DeviceSelected += SelectDevice;
            Load += HomeOverviewPage_Load;
            simulationTimer.Tick += SimulationTimer_Tick;
            Disposed += (sender, e) => simulationTimer.Dispose();
        }

        private async void HomeOverviewPage_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
                return;

            try { await LoadSavedDataAsync(); }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show("加载首页数据失败：" + ex.Message, "首页总览", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsDesignTime()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode)
                return true;

            for (Control control = this; control != null; control = control.Parent)
            {
                if (control.Site != null && control.Site.DesignMode)
                    return true;
            }

            string processName = Process.GetCurrentProcess().ProcessName;
            return processName.Equals("devenv", StringComparison.OrdinalIgnoreCase)
                || processName.Equals("DesignToolsServer", StringComparison.OrdinalIgnoreCase)
                || processName.Equals("XDesProc", StringComparison.OrdinalIgnoreCase);
        }

        private async Task LoadSavedDataAsync()
        {
            IList<string> activeDeviceIds = await homeDataService.GetActiveDeviceIdsAsync();
            if (IsDisposed || Disposing) return;
            devices.Clear();
            devices.AddRange(activeDeviceIds);

            selectedDevice = devices[0];
            foreach (string deviceId in devices)
            {
                List<SensorReading> savedReadings = await homeDataService.GetRecentReadingsAsync(deviceId, 11);
                if (IsDisposed || Disposing) return;
                readingsByDevice[deviceId] = savedReadings;
                loadedReadingDevices.Add(deviceId);
            }

            overviewToolbarControl.SetDevices(devices, selectedDevice);
            DataTable savedAlarms = await homeDataService.GetLatestAlarmsAsync(MaximumAlarmRecords);
            if (IsDisposed || Disposing) return;
            LoadSavedAlarms(savedAlarms);
            RenderDeviceHistory(selectedDevice);
            ShowCachedDeviceSummary(selectedDevice);
            UpdateMonitoringControls();
        }

        private async Task EnsureReadingsLoadedAsync(string deviceId)
        {
            if (loadedReadingDevices.Contains(deviceId)) return;

            Task loadTask;
            if (!readingLoadsInProgress.TryGetValue(deviceId, out loadTask))
            {
                loadTask = LoadReadingsIntoCacheAsync(deviceId);
                readingLoadsInProgress[deviceId] = loadTask;
                if (loadTask.IsCompleted)
                    readingLoadsInProgress.Remove(deviceId);
            }

            await loadTask;
        }

        private async Task LoadReadingsIntoCacheAsync(string deviceId)
        {
            try
            {
                List<SensorReading> savedReadings = await homeDataService.GetRecentReadingsAsync(deviceId, 11);
                if (IsDisposed || Disposing) return;

                List<SensorReading> currentReadings;
                if (!readingsByDevice.TryGetValue(deviceId, out currentReadings))
                    currentReadings = new List<SensorReading>();

                // 读取期间可能已经收到新采样；合并并按时间保留最新 11 条，避免覆盖新数据。
                var combined = savedReadings.Concat(currentReadings)
                    .GroupBy(reading => reading.Timestamp)
                    .Select(group => group.Last())
                    .OrderBy(reading => reading.Timestamp)
                    .ToList();
                if (combined.Count > 11)
                    combined = combined.Skip(combined.Count - 11).ToList();

                readingsByDevice[deviceId] = combined;
                loadedReadingDevices.Add(deviceId);
            }
            finally
            {
                readingLoadsInProgress.Remove(deviceId);
            }
        }

        private async void EnsureReadingsLoadedInBackground(string deviceId)
        {
            try
            {
                await EnsureReadingsLoadedAsync(deviceId);
                if (!IsDisposed && !Disposing
                    && string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                {
                    RenderDeviceHistory(deviceId);
                    ShowCachedDeviceSummary(deviceId);
                }
            }
            catch (Exception ex)
            {
                ShowDatabaseError("设备 " + deviceId + " 的历史采样读取失败。", ex);
            }
        }

        private void LoadSavedAlarms(DataTable table)
        {
            alarmRecords.Clear();
            activeAlarmsByDevice.Clear();
            alarmStateByDevice.Clear();
            foreach (DataRow row in table.Rows)
            {
                DateTime startTime = DateTime.Parse(row["StartTime"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                double temperature = Convert.ToDouble(row["Temperature"], CultureInfo.InvariantCulture);
                double pressure = Convert.ToDouble(row["Pressure"], CultureInfo.InvariantCulture);
                var record = new AlarmRecord
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Reading = new SensorReading(row["DeviceId"].ToString(), startTime, temperature, pressure),
                    Reason = row["AlarmType"].ToString(),
                    Peak = row["AlarmType"].ToString().Contains("温度")
                        ? temperature.ToString("F2")
                        : pressure.ToString("F2"),
                    StartTime = startTime,
                    EndTime = row["EndTime"] == DBNull.Value
                        ? (DateTime?)null
                        : DateTime.Parse(row["EndTime"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                };
                alarmRecords.Add(record);
                string deviceId = record.Reading.DeviceId;
                if (record.EndTime == null && row["Status"].ToString() == "处理中"
                    && !activeAlarmsByDevice.ContainsKey(deviceId))
                {
                    activeAlarmsByDevice.Add(deviceId, record);
                    alarmStateByDevice[deviceId] = true;
                }
            }

            resetAlarmPagePending = true;
            RenderAlarmPage();
        }

        private void BuildLayout()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = pageBackground,
                Margin = Padding.Empty
            };
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 102F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            Controls.Add(page);

            overviewToolbarControl = new OverviewToolbarControl { Dock = DockStyle.Fill, Margin = Padding.Empty };
            page.Controls.Add(overviewToolbarControl, 0, 0);
            overviewChartControl = new OverviewChartControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 6) };
            overviewAlarmControl = new OverviewAlarmControl { Dock = DockStyle.Fill };
            page.Controls.Add(overviewChartControl, 0, 1);
            page.Controls.Add(overviewAlarmControl, 0, 2);
        }

        private void StartMonitoring_Click(object sender, System.EventArgs e)
        {
            if (communicationDevices.Contains(selectedDevice))
            {
                MessageBox.Show("该设备正在网络通讯模拟采样中，请先在“网络通讯”页面停止模拟。",
                    "设备正在采样", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!runningDevices.Add(selectedDevice))
                return;

            simulationTimer.Start();
            UpdateMonitoringControls();
            TryUpdateReading(selectedDevice);
        }

        private void StopMonitoring_Click(object sender, System.EventArgs e)
        {
            if (!runningDevices.Remove(selectedDevice))
                return;

            CloseActiveAlarm(selectedDevice, DateTime.Now);
            alarmStateByDevice[selectedDevice] = false;
            if (runningDevices.Count == 0)
                simulationTimer.Stop();

            UpdateMonitoringControls();
            ShowCachedDeviceSummary(selectedDevice);
        }

        private async void SelectDevice(string deviceId)
        {
            if (string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                return;

            selectedDevice = deviceId;
            overviewToolbarControl.SetSelectedDevice(selectedDevice);

            if (!loadedReadingDevices.Contains(selectedDevice))
            {
                overviewChartControl.ShowReadings(null);
                overviewToolbarControl.SetValues(null, null);
                overviewToolbarControl.SetBanner("正在加载 " + selectedDevice + " 的历史数据…", false);
            }
            UpdateMonitoringControls();

            try
            {
                await EnsureReadingsLoadedAsync(selectedDevice);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                    MessageBox.Show("加载设备 " + selectedDevice + " 的历史采样失败。\r\n" + ex.Message,
                        "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsDisposed || Disposing
                || !string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                return;

            RenderDeviceHistory(selectedDevice);
            ShowCachedDeviceSummary(selectedDevice);
        }

        private void UpdateMonitoringControls()
        {
            overviewToolbarControl.SetCounts(runningDevices.Count + communicationDevices.Count, devices.Count);
            overviewToolbarControl.SetMonitoringButtons(!runningDevices.Contains(selectedDevice)
                && !communicationDevices.Contains(selectedDevice), runningDevices.Contains(selectedDevice));
        }

        private async void AddDevice_Click(object sender, EventArgs e)
        {
            string newDeviceId;
            try
            {
                newDeviceId = await deviceManagementService.AddNextDeviceAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加设备失败。\r\n" + ex.Message, "添加失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(newDeviceId))
            {
                MessageBox.Show("设备编号已存在，请重试。", "添加失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            devices.Add(newDeviceId);
            overviewToolbarControl.SetDevices(devices, selectedDevice);
            SelectDevice(newDeviceId);
        }

        private async void DeleteDevice_Click(object sender, EventArgs e)
        {
            if (devices.Count <= 1)
            {
                MessageBox.Show("至少需要保留一台设备。", "无法删除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "确定删除当前设备 " + selectedDevice + " 吗？",
                "确认删除设备",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            string removedDevice = selectedDevice;
            int activeCount;
            try
            {
                activeCount = await deviceManagementService.GetActiveCountAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("检查设备数量失败。\r\n" + ex.Message, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (activeCount <= 1)
            {
                MessageBox.Show("至少需要保留一台正常设备。", "无法删除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            bool deleted;
            try
            {
                deleted = await deviceManagementService.SetDeletedAsync(removedDevice, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除设备失败。\r\n" + ex.Message, "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (IsDisposed || Disposing) return;
            if (!deleted)
            {
                MessageBox.Show("数据库没有更新该设备，请重试。", "删除失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            runningDevices.Remove(removedDevice);
            if (runningDevices.Count == 0)
                simulationTimer.Stop();
            CloseActiveAlarm(removedDevice, DateTime.Now);
            alarmStateByDevice.Remove(removedDevice);
            devices.Remove(removedDevice);
            readingsByDevice.Remove(removedDevice);
            simulator.RemoveDevice(removedDevice);

            bool selectionWasDeleted = string.Equals(selectedDevice, removedDevice, StringComparison.OrdinalIgnoreCase);
            if (selectionWasDeleted)
                selectedDevice = string.Empty;
            string nextSelection = selectionWasDeleted ? devices[0] : selectedDevice;
            overviewToolbarControl.SetDevices(devices, nextSelection);
            if (selectionWasDeleted)
                SelectDevice(nextSelection);
            else
                UpdateMonitoringControls();
        }

        private async void RestoreDevice_Click(object sender, EventArgs e)
        {
            DataTable deletedDevices;
            try
            {
                deletedDevices = await deviceManagementService.GetDeletedDevicesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取已删除设备失败。\r\n" + ex.Message, "设备恢复", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (IsDisposed || Disposing) return;
            if (deletedDevices.Rows.Count == 0)
            {
                MessageBox.Show(FindForm(), "目前没有已删除的设备。", "设备恢复", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new Form
            {
                Text = "恢复已删除设备",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(520, 340),
                Font = new Font("微软雅黑", 9F)
            })
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 2,
                    Padding = new Padding(10)
                };
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

                var grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White
                };
                grid.Columns.Add("DeviceId", "设备编号");
                grid.Columns.Add("Name", "设备名称");
                grid.Columns.Add("DeletedAt", "删除时间");

                foreach (DataRow row in deletedDevices.Rows)
                {
                    string deletedAt = string.IsNullOrWhiteSpace(row["DeletedAt"].ToString())
                        ? string.Empty
                        : DateTime.Parse(row["DeletedAt"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                            .ToString("yyyy-MM-dd HH:mm:ss");
                    grid.Rows.Add(row["DeviceId"], row["Name"], deletedAt);
                }
                layout.Controls.Add(grid, 0, 0);

                var footer = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false
                };
                var closeButton = new Button { Text = "关闭", AutoSize = true };
                var restoreButton = new Button { Text = "恢复所选设备", AutoSize = true };
                restoreButton.Click += async (restoreSender, restoreArgs) =>
                {
                    if (grid.CurrentRow == null)
                    {
                        MessageBox.Show(dialog, "请先选择一台已删除的设备。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string deviceId = Convert.ToString(grid.CurrentRow.Cells[0].Value);
                    restoreButton.Enabled = false;
                    closeButton.Enabled = false;
                    bool restored;
                    try
                    {
                        restored = await deviceManagementService.SetDeletedAsync(deviceId, false);
                    }
                    catch (Exception ex)
                    {
                        restoreButton.Enabled = true;
                        closeButton.Enabled = true;
                        MessageBox.Show(dialog, "恢复设备失败。\r\n" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (dialog.IsDisposed) return;
                    if (!restored)
                    {
                        restoreButton.Enabled = true;
                        closeButton.Enabled = true;
                        MessageBox.Show(dialog, "恢复失败，请重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    devices.Add(deviceId);
                    overviewToolbarControl.SetDevices(devices, selectedDevice);
                    SelectDevice(deviceId);
                    dialog.Close();
                };
                closeButton.Click += (closeSender, closeArgs) => dialog.Close();
                footer.Controls.Add(closeButton);
                footer.Controls.Add(restoreButton);
                layout.Controls.Add(footer, 0, 1);
                dialog.Controls.Add(layout);

                dialog.ShowDialog(FindForm());
            }
        }

        private void SimulationTimer_Tick(object sender, System.EventArgs e)
        {
            if (runningDevices.Count == 0)
            {
                simulationTimer.Stop();
                UpdateMonitoringControls();
                return;
            }

            foreach (string deviceId in new List<string>(runningDevices))
                TryUpdateReading(deviceId);
        }

        private void TryUpdateReading(string deviceId)
        {
            try
            {
                UpdateReading(deviceId);
            }
            catch (Exception ex)
            {
                runningDevices.Remove(deviceId);
                if (runningDevices.Count == 0)
                    simulationTimer.Stop();
                UpdateMonitoringControls();

                if (string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                    ShowCachedDeviceSummary(deviceId);

                MessageBox.Show("设备 " + deviceId + " 保存监控数据失败，该设备监控已停止。\r\n" + ex.Message,
                    "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateReading(string deviceId)
        {
            ProcessReading(simulator.NextReading(deviceId), false);
        }

        /// <summary>接收网络通讯页面产生的模拟读数，沿用首页的数据库、曲线和报警处理。</summary>
        public void AcceptCommunicationReading(SensorReading reading)
        {
            if (reading == null)
                throw new ArgumentNullException("reading");

            if (runningDevices.Contains(reading.DeviceId))
                throw new InvalidOperationException("设备 " + reading.DeviceId
                    + " 已在首页总览中监控，请先停止首页监控再启动模拟通信。");

            communicationDevices.Add(reading.DeviceId);
            ProcessReading(reading, true);
        }

        /// <summary>模拟通信停止时，刷新首页状态文字但保留已采样数据。</summary>
        public void CommunicationStopped(string deviceId)
        {
            communicationDevices.Remove(deviceId);
            if (string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
            {
                ShowCachedDeviceSummary(deviceId);
                UpdateMonitoringControls();
            }
        }

        private void ProcessReading(SensorReading reading, bool fromCommunication)
        {
            string deviceId = reading.DeviceId;
            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(deviceId, out deviceReadings))
            {
                deviceReadings = new List<SensorReading>();
                readingsByDevice[deviceId] = deviceReadings;
                EnsureReadingsLoadedInBackground(deviceId);
            }

            SaveReadingInBackground(reading);

            deviceReadings.Add(reading);
            if (deviceReadings.Count > 11)
                deviceReadings.RemoveAt(0);

            bool isSelectedDevice = string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase);
            if (isSelectedDevice)
            {
                overviewToolbarControl.SetValues(reading.Temperature, reading.Pressure);
                RenderDeviceHistory(deviceId);
            }

            bool temperatureAlarm = reading.Temperature >= 60.0;
            bool pressureAlarm = reading.Pressure >= 1.8;
            bool alarm = temperatureAlarm || pressureAlarm;
            bool wasAlarm;
            alarmStateByDevice.TryGetValue(deviceId, out wasAlarm);

            if (alarm)
            {
                string reason = temperatureAlarm && pressureAlarm
                    ? "温度+压力超限"
                    : (temperatureAlarm ? "温度超限" : "压力超限");
                if (isSelectedDevice)
                {
                    overviewToolbarControl.SetBanner("⚠  " + deviceId + " " + reason, true);
                }

                if (!wasAlarm)
                    AddAlarmRow(reading, reason);
            }
            else
            {
                if (isSelectedDevice)
                {
                    overviewToolbarControl.SetBanner(fromCommunication
                        ? "●  正在接收 " + deviceId + " 的通信数据"
                        : "●  正在监控 " + deviceId + "，系统正常运行", false);
                }

                if (wasAlarm)
                    CloseActiveAlarm(deviceId, reading.Timestamp);
            }

            alarmStateByDevice[deviceId] = alarm;
        }

        private async void SaveReadingInBackground(SensorReading reading)
        {
            try
            {
                await readingPersistenceService.SaveAsync(reading);
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing) return;
                Action showError = () => MessageBox.Show(
                    "设备 " + reading.DeviceId + " 的采样数据保存失败。\r\n" + ex.Message,
                    "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (InvokeRequired)
                {
                    try { BeginInvoke(showError); }
                    catch (InvalidOperationException) { }
                }
                else showError();
            }
        }

        private void RenderDeviceHistory(string deviceId)
        {
            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(deviceId, out deviceReadings))
            {
                overviewChartControl.ShowReadings(null);
                return;
            }
            overviewChartControl.ShowReadings(deviceReadings);
        }

        private void ShowCachedDeviceSummary(string deviceId)
        {
            bool isCommunicationRunning = communicationDevices.Contains(deviceId);
            bool isRunning = runningDevices.Contains(deviceId) || isCommunicationRunning;
            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(deviceId, out deviceReadings) || deviceReadings.Count == 0)
            {
                overviewToolbarControl.SetValues(null, null);
                overviewToolbarControl.SetBanner(isRunning
                    ? "●  正在监控 " + deviceId + "，等待首条数据"
                    : "●  " + deviceId + " 尚无数据（监控已停止）", false);
                return;
            }

            SensorReading latest = deviceReadings[deviceReadings.Count - 1];
            overviewToolbarControl.SetValues(latest.Temperature, latest.Pressure);

            bool alarm = latest.Temperature >= 60.0 || latest.Pressure >= 1.8;
            if (activeAlarmsByDevice.ContainsKey(deviceId))
            {
                overviewToolbarControl.SetBanner("⚠  " + deviceId + " 存在处理中报警", true);
            }
            else if (isRunning)
            {
                overviewToolbarControl.SetBanner(isCommunicationRunning
                    ? "●  正在接收 " + deviceId + " 的通信数据"
                    : "●  正在监控 " + deviceId + "，系统正常运行", false);
            }
            else
            {
                overviewToolbarControl.SetBanner(alarm
                    ? "⚠  " + deviceId + " 上次读数超限（监控已停止）"
                    : "●  " + deviceId + " 已显示保留数据（监控已停止）", alarm);
            }
        }

        private void CloseActiveAlarm(string deviceId, DateTime endTime)
        {
            AlarmRecord activeAlarm;
            if (activeAlarmsByDevice.TryGetValue(deviceId, out activeAlarm))
            {
                activeAlarm.EndTime = endTime;
                CloseAlarmInBackground(activeAlarm, endTime);
                activeAlarmsByDevice.Remove(deviceId);
            }

            alarmStateByDevice[deviceId] = false;
            RenderAlarmPage();
        }

        private void AddAlarmRow(SensorReading reading, string reason)
        {
            string peak = reason.Contains("温度")
                ? reading.Temperature.ToString("F2")
                : reading.Pressure.ToString("F2");

            var alarm = new AlarmRecord
            {
                Reading = reading,
                Reason = reason,
                Peak = peak,
                StartTime = reading.Timestamp
            };
            alarm.PersistenceTask = alarmPersistenceService.AddAsync(reading, reason, 60.0, 1.8);
            PersistAlarmInBackground(alarm);
            alarmRecords.Insert(0, alarm);
            activeAlarmsByDevice[reading.DeviceId] = alarm;
            alarmStateByDevice[reading.DeviceId] = true;

            if (alarmRecords.Count > MaximumAlarmRecords)
                alarmRecords.RemoveAt(alarmRecords.Count - 1);

            // 新报警加入列表顶部，切回第一页让它立即可见。
            resetAlarmPagePending = true;
            RenderAlarmPage();
        }

        private async void PersistAlarmInBackground(AlarmRecord alarm)
        {
            try
            {
                alarm.Id = await alarm.PersistenceTask;
            }
            catch (Exception ex)
            {
                ShowAlarmPersistenceError(alarm.Reading.DeviceId, "新增报警", ex);
            }
        }

        private async void CloseAlarmInBackground(AlarmRecord alarm, DateTime endTime)
        {
            int alarmId;
            try
            {
                alarmId = alarm.PersistenceTask != null
                    ? await alarm.PersistenceTask
                    : alarm.Id;
            }
            catch (Exception ex)
            {
                // 新增失败已由 PersistAlarmInBackground 报告；没有数据库记录可更新。
                System.Diagnostics.Debug.WriteLine(ex);
                return;
            }

            if (alarmId <= 0) return;
            alarm.Id = alarmId;
            try
            {
                await alarmPersistenceService.CloseAsync(alarmId, endTime);
            }
            catch (Exception ex)
            {
                ShowAlarmPersistenceError(alarm.Reading.DeviceId, "更新报警解除时间", ex);
            }
        }

        private void ShowAlarmPersistenceError(string deviceId, string action, Exception exception)
        {
            ShowDatabaseError("设备 " + deviceId + " 的" + action + "保存失败。", exception);
        }

        private void ShowDatabaseError(string message, Exception exception)
        {
            if (IsDisposed || Disposing) return;
            Action showError = () => MessageBox.Show(
                message + "\r\n" + exception.Message,
                "数据库错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (InvokeRequired)
            {
                try { BeginInvoke(showError); }
                catch (InvalidOperationException) { }
            }
            else showError();
        }

        private void RenderAlarmPage()
        {
            var items = new List<OverviewAlarmItem>();
            foreach (AlarmRecord alarm in alarmRecords)
            {
                AlarmRecord activeAlarm;
                bool isActive = activeAlarmsByDevice.TryGetValue(alarm.Reading.DeviceId, out activeAlarm)
                    && ReferenceEquals(activeAlarm, alarm);
                string endTime = alarm.EndTime.HasValue
                    ? alarm.EndTime.Value.ToString("MM-dd HH:mm:ss")
                    : "处理中";
                items.Add(new OverviewAlarmItem
                {
                    Id = alarm.Id.ToString(), Device = alarm.Reading.DeviceId,
                    Type = alarm.Reason, Peak = alarm.Peak,
                    Details = "温度阈值 60.0 °C / 压力阈值 1.8 MPa",
                    Start = alarm.StartTime.ToString("MM-dd HH:mm:ss"),
                    End = endTime, IsActive = isActive
                });
            }
            overviewAlarmControl.SetItems(items, resetAlarmPagePending);
            resetAlarmPagePending = false;
        }
    }
}
