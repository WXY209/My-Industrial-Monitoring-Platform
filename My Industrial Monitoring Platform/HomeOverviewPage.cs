using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
        }

        private const int MaximumAlarmRecords = 75;
        private readonly Color pageBackground = Color.FromArgb(241, 245, 249);
        private readonly MonitoringSimulator simulator = new MonitoringSimulator();
        private readonly Timer simulationTimer = new Timer { Interval = 1000 };
        private readonly Dictionary<string, List<SensorReading>> readingsByDevice =
            new Dictionary<string, List<SensorReading>>(StringComparer.OrdinalIgnoreCase);
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

        public HomeOverviewPage()
        {
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

        private void HomeOverviewPage_Load(object sender, EventArgs e)
        {
            if (IsDesignTime())
                return;

            LoadSavedData();
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

        private void LoadSavedData()
        {
            DataTable deviceTable = DeviceDB.GetActiveDevices();
            devices.Clear();
            foreach (DataRow row in deviceTable.Rows)
                devices.Add(row["DeviceId"].ToString());

            if (devices.Count == 0)
            {
                string defaultId = DeviceDB.GetNextDeviceId();
                DeviceDB.AddDevice(defaultId, defaultId);
                devices.Add(defaultId);
            }

            selectedDevice = devices[0];
            foreach (string deviceId in devices)
            {
                LoadSavedReadings(deviceId);
            }

            overviewToolbarControl.SetDevices(devices, selectedDevice);
            LoadSavedAlarms();
            RenderDeviceHistory(selectedDevice);
            ShowCachedDeviceSummary(selectedDevice);
            UpdateMonitoringControls();
        }

        private void LoadSavedReadings(string deviceId)
        {
            if (!readingsByDevice.ContainsKey(deviceId))
                readingsByDevice.Add(deviceId, ReadingDB.GetRecentReadings(deviceId, 11));
        }

        private void LoadSavedAlarms()
        {
            alarmRecords.Clear();
            activeAlarmsByDevice.Clear();
            alarmStateByDevice.Clear();
            DataTable table = AlarmDB.GetLatestAlarms(MaximumAlarmRecords);
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

        private void SelectDevice(string deviceId)
        {
            if (string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                return;

            selectedDevice = deviceId;
            overviewToolbarControl.SetSelectedDevice(selectedDevice);

            LoadSavedReadings(selectedDevice);
            RenderDeviceHistory(selectedDevice);
            ShowCachedDeviceSummary(selectedDevice);
            UpdateMonitoringControls();
        }

        private void UpdateMonitoringControls()
        {
            overviewToolbarControl.SetCounts(runningDevices.Count + communicationDevices.Count, devices.Count);
            overviewToolbarControl.SetMonitoringButtons(!runningDevices.Contains(selectedDevice)
                && !communicationDevices.Contains(selectedDevice), runningDevices.Contains(selectedDevice));
        }

        private void AddDevice_Click(object sender, EventArgs e)
        {
            string newDeviceId = DeviceDB.GetNextDeviceId();
            if (!DeviceDB.AddDevice(newDeviceId, newDeviceId))
            {
                MessageBox.Show("设备编号已存在，请重试。", "添加失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            devices.Add(newDeviceId);
            overviewToolbarControl.SetDevices(devices, selectedDevice);
            SelectDevice(newDeviceId);
        }

        private void DeleteDevice_Click(object sender, EventArgs e)
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
            if (DeviceDB.GetActiveCount() <= 1)
            {
                MessageBox.Show("至少需要保留一台正常设备。", "无法删除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!DeviceDB.SetDeleted(removedDevice, true))
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

            overviewToolbarControl.SetDevices(devices, devices[0]);
            selectedDevice = string.Empty;
            SelectDevice(devices[0]);
            UpdateMonitoringControls();
        }

        private void RestoreDevice_Click(object sender, EventArgs e)
        {
            DataTable deletedDevices = DeviceDB.GetDeletedDevices();
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
                restoreButton.Click += (restoreSender, restoreArgs) =>
                {
                    if (grid.CurrentRow == null)
                    {
                        MessageBox.Show(dialog, "请先选择一台已删除的设备。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string deviceId = Convert.ToString(grid.CurrentRow.Cells[0].Value);
                    if (!DeviceDB.SetDeleted(deviceId, false))
                    {
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
                LoadSavedReadings(deviceId);
                deviceReadings = readingsByDevice[deviceId];
            }

            ReadingDB.AddReading(reading);

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
                AlarmDB.CloseAlarm(activeAlarm.Id, endTime);
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
                Id = AlarmDB.AddAlarm(reading, reason, 60.0, 1.8),
                Reading = reading,
                Reason = reason,
                Peak = peak,
                StartTime = reading.Timestamp
            };
            alarmRecords.Insert(0, alarm);
            activeAlarmsByDevice[reading.DeviceId] = alarm;
            alarmStateByDevice[reading.DeviceId] = true;

            if (alarmRecords.Count > MaximumAlarmRecords)
                alarmRecords.RemoveAt(alarmRecords.Count - 1);

            // 新报警加入列表顶部，切回第一页让它立即可见。
            resetAlarmPagePending = true;
            RenderAlarmPage();
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
