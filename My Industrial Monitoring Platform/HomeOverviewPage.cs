using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

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

        private const int AlarmPageSize = 15;
        private const int MaximumAlarmRecords = 75;
        private readonly Color pageBackground = Color.FromArgb(241, 245, 249);
        private readonly Color darkText = Color.FromArgb(44, 62, 80);
        private readonly MonitoringSimulator simulator = new MonitoringSimulator();
        private readonly Timer simulationTimer = new Timer { Interval = 1000 };
        private readonly Dictionary<string, List<SensorReading>> readingsByDevice =
            new Dictionary<string, List<SensorReading>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<AlarmRecord> alarmRecords = new List<AlarmRecord>();
        private readonly List<string> devices = new List<string> { "DEV-001" };
        private Button startButton;
        private Button stopButton;
        private Button addDeviceButton;
        private Button deleteDeviceButton;
        private FlowLayoutPanel deviceTabs;
        private Label runSummary;
        private Label deviceSummary;
        private Label deviceCountSummary;
        private Label temperatureSummary;
        private Label pressureSummary;
        private Label statusBanner;
        private Chart temperatureChart;
        private Chart pressureChart;
        private DataGridView historyGrid;
        private Button firstAlarmPageButton;
        private Button previousAlarmPageButton;
        private Button nextAlarmPageButton;
        private Button lastAlarmPageButton;
        private Label alarmPageInfo;
        private string selectedDevice = "DEV-001";
        private bool previousAlarm;
        private AlarmRecord activeAlarmRecord;
        private int currentAlarmPage = 1;
        private int nextAlarmId = 1;

        public HomeOverviewPage()
        {
            BackColor = pageBackground;
            Padding = new Padding(10);
            BuildLayout();
            simulationTimer.Tick += SimulationTimer_Tick;
            Disposed += (sender, e) => simulationTimer.Dispose();
        }

        private void BuildLayout()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = pageBackground,
                Margin = Padding.Empty
            };
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            Controls.Add(page);

            page.Controls.Add(BuildToolbar(), 0, 0);
            page.Controls.Add(BuildStatusBanner(), 0, 1);
            page.Controls.Add(BuildDeviceTabs(), 0, 2);
            page.Controls.Add(BuildCharts(), 0, 3);
            page.Controls.Add(BuildHistoryCard(), 0, 4);
        }

        private Control BuildToolbar()
        {
            var toolbar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.White,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(0, 0, 0, 6)
            };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            startButton = CreateActionButton("▶ 开始监控", Color.FromArgb(16, 185, 129));
            stopButton = CreateActionButton("■ 停止监控", Color.FromArgb(239, 83, 80));
            startButton.Click += StartMonitoring_Click;
            stopButton.Click += StopMonitoring_Click;
            actions.Controls.Add(startButton);
            actions.Controls.Add(stopButton);
            addDeviceButton = CreateActionButton("＋ 添加设备", Color.FromArgb(55, 130, 245));
            deleteDeviceButton = CreateActionButton("× 删除设备", Color.FromArgb(100, 112, 130));
            addDeviceButton.Click += AddDevice_Click;
            deleteDeviceButton.Click += DeleteDevice_Click;
            actions.Controls.Add(addDeviceButton);
            actions.Controls.Add(deleteDeviceButton);
            toolbar.Controls.Add(actions, 0, 0);

            var summary = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 7, 0, 0),
                Margin = Padding.Empty
            };
            runSummary = CreateSummaryLabel("运行: 0", Color.FromArgb(55, 130, 245));
            deviceSummary = CreateSummaryLabel("设备: DEV-001", darkText);
            temperatureSummary = CreateSummaryLabel("温度: -- °C", Color.FromArgb(239, 83, 80));
            pressureSummary = CreateSummaryLabel("压力: -- MPa", Color.FromArgb(55, 130, 245));
            deviceCountSummary = CreateSummaryLabel("设备数: 1", Color.FromArgb(16, 185, 129));
            summary.Controls.Add(runSummary);
            summary.Controls.Add(deviceCountSummary);
            summary.Controls.Add(pressureSummary);
            summary.Controls.Add(temperatureSummary);
            summary.Controls.Add(deviceSummary);
            toolbar.Controls.Add(summary, 1, 0);

            return toolbar;
        }

        private static Button CreateActionButton(string text, Color color)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8.5F, FontStyle.Bold),
                Margin = new Padding(2, 1, 4, 1),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private static Label CreateSummaryLabel(string text, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = color,
                Font = new Font("微软雅黑", 8F, FontStyle.Bold),
                Margin = new Padding(8, 1, 2, 0)
            };
        }

        private Control BuildStatusBanner()
        {
            statusBanner = new Label
            {
                Dock = DockStyle.Fill,
                Text = "●  系统正常运行",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = darkText,
                Font = new Font("微软雅黑", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 2, 0, 4)
            };
            return statusBanner;
        }

        private Control BuildDeviceTabs()
        {
            deviceTabs = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                BackColor = pageBackground,
                Margin = Padding.Empty,
                Padding = new Padding(0, 2, 0, 0)
            };
            AddDeviceTab("DEV-001");
            return deviceTabs;
        }

        private void AddDeviceTab(string deviceId)
        {
            var button = CreateDeviceButton("▦  " + deviceId, deviceId == selectedDevice);
            button.Tag = deviceId;
            button.Click += (sender, e) => SelectDevice((string)((Button)sender).Tag);
            deviceTabs.Controls.Add(button);
        }

        private static Button CreateDeviceButton(string text, bool selected)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = selected ? Color.White : Color.FromArgb(241, 245, 249),
                ForeColor = selected ? Color.FromArgb(55, 130, 245) : Color.FromArgb(60, 70, 85),
                Font = new Font("微软雅黑", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Control BuildCharts()
        {
            var chartLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = pageBackground,
                Margin = new Padding(0, 0, 0, 6)
            };
            chartLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            chartLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            chartLayout.Controls.Add(CreateChartCard("▦  实时温度 (°C)", 30, 70, 5, 60, Color.Red, "°C", Color.Red, out temperatureChart), 0, 0);
            chartLayout.Controls.Add(CreateChartCard("▦  实时压力 (MPa)", 0.8, 2.2, 0.1, 1.8, Color.FromArgb(255, 153, 51), "MPa", Color.Blue, out pressureChart), 1, 0);
            return chartLayout;
        }

        private static Control CreateChartCard(
            string titleText,
            double minimum,
            double maximum,
            double interval,
            double threshold,
            Color thresholdColor,
            string axisTitle,
            Color seriesColor,
            out Chart chart)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8),
                Margin = new Padding(0, 0, 5, 0)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            card.Controls.Add(layout);

            var title = new Label
            {
                Text = titleText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(57, 75, 96),
                Font = new Font("微软雅黑", 8.5F, FontStyle.Bold)
            };
            layout.Controls.Add(title, 0, 0);

            chart = new Chart
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Palette = ChartColorPalette.None,
                BorderlineColor = Color.White,
                BorderlineDashStyle = ChartDashStyle.Solid
            };

            var area = new ChartArea("Main")
            {
                BackColor = Color.White
            };
            area.AxisX.Minimum = 0;
            area.AxisX.Maximum = 10;
            area.AxisX.Interval = 0.5;
            area.AxisX.IsReversed = false;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LabelStyle.ForeColor = Color.Gray;
            area.AxisX.LabelStyle.Font = new Font("微软雅黑", 7.5F);
            area.AxisX.LineColor = Color.Gainsboro;

            area.AxisY.Minimum = minimum;
            area.AxisY.Maximum = maximum;
            area.AxisY.Interval = interval;
            area.AxisY.Title = axisTitle;
            area.AxisY.TitleFont = new Font("微软雅黑", 9F);
            area.AxisY.LabelStyle.ForeColor = Color.Gray;
            area.AxisY.LabelStyle.Font = new Font("微软雅黑", 7.5F);
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(237, 239, 242);
            area.AxisY.LineColor = Color.Gainsboro;

            area.AxisY.StripLines.Add(new StripLine
            {
                Interval = 0,
                IntervalOffset = threshold,
                StripWidth = 0,
                BorderColor = thresholdColor,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dash
            });

            chart.ChartAreas.Add(area);
            chart.Series.Add(new Series(axisTitle == "°C" ? "Temperature" : "Pressure")
            {
                ChartArea = "Main",
                ChartType = SeriesChartType.Spline,
                BorderWidth = 2,
                Color = seriesColor
            });
            layout.Controls.Add(chart, 0, 1);
            return card;
        }

        private Control BuildHistoryCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(8),
                Margin = Padding.Empty
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            card.Controls.Add(layout);

            var header = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            header.Controls.Add(new Label
            {
                Text = "▤  历史报警记录",
                AutoSize = true,
                ForeColor = Color.FromArgb(57, 75, 96),
                Font = new Font("微软雅黑", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 5, 10, 0)
            });

            var refresh = new Button
            {
                Text = "⟳ 刷新",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 130, 245),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 8F),
                Margin = new Padding(0, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            refresh.FlatAppearance.BorderSize = 0;
            refresh.Click += (sender, e) => RenderAlarmPage();
            header.Controls.Add(refresh);
            layout.Controls.Add(header, 0, 0);

            var history = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 32,
                RowTemplate = { Height = 31 }
            };
            historyGrid = history;
            history.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(66, 78, 95),
                Font = new Font("微软雅黑", 8F, FontStyle.Bold),
                SelectionBackColor = Color.White,
                SelectionForeColor = Color.FromArgb(66, 78, 95)
            };
            history.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(65, 72, 82),
                SelectionBackColor = Color.FromArgb(239, 245, 252),
                SelectionForeColor = Color.FromArgb(65, 72, 82),
                Padding = new Padding(3)
            };

            string[] headers = { "编号", "设备", "类型", "峰值", "阈值详情", "开始时间", "结束时间" };
            string[] names = { "Id", "Device", "Type", "Value", "Details", "Start", "End" };
            for (int i = 0; i < headers.Length; i++)
            {
                history.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = names[i],
                    HeaderText = headers[i],
                    FillWeight = i == 4 ? 150 : (i >= 5 ? 125 : 70)
                });
            }

            layout.Controls.Add(history, 0, 1);

            var footer = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(0, 2, 0, 0),
                Margin = Padding.Empty
            };
            lastAlarmPageButton = CreatePageButton("末页 ▶▶");
            nextAlarmPageButton = CreatePageButton("▶");
            previousAlarmPageButton = CreatePageButton("◀");
            firstAlarmPageButton = CreatePageButton("首页 ◀◀");
            lastAlarmPageButton.Click += (sender, e) => ChangeAlarmPage(GetAlarmPageCount());
            nextAlarmPageButton.Click += (sender, e) => ChangeAlarmPage(currentAlarmPage + 1);
            previousAlarmPageButton.Click += (sender, e) => ChangeAlarmPage(currentAlarmPage - 1);
            firstAlarmPageButton.Click += (sender, e) => ChangeAlarmPage(1);
            footer.Controls.Add(lastAlarmPageButton);
            footer.Controls.Add(nextAlarmPageButton);
            alarmPageInfo = new Label
            {
                Text = "第 1/1 页（共 0 条）",
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font("微软雅黑", 8F),
                Margin = new Padding(8, 7, 8, 0)
            };
            footer.Controls.Add(alarmPageInfo);
            footer.Controls.Add(previousAlarmPageButton);
            footer.Controls.Add(firstAlarmPageButton);
            layout.Controls.Add(footer, 0, 2);
            UpdateAlarmPager();

            return card;
        }

        private static Button CreatePageButton(string text)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 130, 245),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 7.5F),
                Margin = new Padding(2, 0, 2, 0),
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void StartMonitoring_Click(object sender, System.EventArgs e)
        {
            if (simulationTimer.Enabled)
                return;

            runSummary.Text = "运行: 1";
            simulationTimer.Start();
            UpdateReading();
        }

        private void StopMonitoring_Click(object sender, System.EventArgs e)
        {
            simulationTimer.Stop();
            runSummary.Text = "运行: 0";

            CloseActiveAlarm(DateTime.Now);
            previousAlarm = false;
            statusBanner.Text = "●  系统正常运行（模拟监控已停止）";
            statusBanner.BackColor = Color.FromArgb(226, 232, 240);
            statusBanner.ForeColor = darkText;
        }

        private void SelectDevice(string deviceId)
        {
            if (string.Equals(deviceId, selectedDevice, StringComparison.OrdinalIgnoreCase))
                return;

            CloseActiveAlarm(DateTime.Now);
            selectedDevice = deviceId;
            deviceSummary.Text = "设备: " + selectedDevice;

            foreach (Control control in deviceTabs.Controls)
            {
                var button = control as Button;
                if (button != null)
                    SetDeviceButtonStyle(button, string.Equals((string)button.Tag, deviceId, StringComparison.OrdinalIgnoreCase));
            }

            RenderDeviceHistory(selectedDevice);
            previousAlarm = false;

            if (simulationTimer.Enabled)
                UpdateReading();
            else
                ShowCachedDeviceSummary(selectedDevice);
        }

        private static void SetDeviceButtonStyle(Button button, bool selected)
        {
            button.BackColor = selected
                ? Color.White
                : Color.FromArgb(241, 245, 249);
            button.ForeColor = selected
                ? Color.FromArgb(55, 130, 245)
                : Color.FromArgb(60, 70, 85);
        }

        private void AddDevice_Click(object sender, EventArgs e)
        {
            int number = 1;
            foreach (string deviceId in devices)
            {
                int parsedNumber;
                if (deviceId.StartsWith("DEV-", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(deviceId.Substring(4), out parsedNumber)
                    && parsedNumber >= number)
                {
                    number = parsedNumber + 1;
                }
            }

            string newDeviceId = "DEV-" + number.ToString("D3");
            devices.Add(newDeviceId);
            AddDeviceTab(newDeviceId);
            deviceCountSummary.Text = "设备数: " + devices.Count;
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
            CloseActiveAlarm(DateTime.Now);
            previousAlarm = false;
            devices.Remove(removedDevice);
            readingsByDevice.Remove(removedDevice);
            simulator.RemoveDevice(removedDevice);

            for (int i = deviceTabs.Controls.Count - 1; i >= 0; i--)
            {
                var button = deviceTabs.Controls[i] as Button;
                if (button != null && string.Equals((string)button.Tag, removedDevice, StringComparison.OrdinalIgnoreCase))
                {
                    deviceTabs.Controls.RemoveAt(i);
                    button.Dispose();
                    break;
                }
            }

            deviceCountSummary.Text = "设备数: " + devices.Count;
            selectedDevice = string.Empty;
            SelectDevice(devices[0]);
        }

        private void SimulationTimer_Tick(object sender, System.EventArgs e)
        {
            UpdateReading();
        }

        private void UpdateReading()
        {
            SensorReading reading = simulator.NextReading(selectedDevice);

            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(selectedDevice, out deviceReadings))
            {
                deviceReadings = new List<SensorReading>();
                readingsByDevice.Add(selectedDevice, deviceReadings);
            }

            deviceReadings.Add(reading);
            if (deviceReadings.Count > 11)
                deviceReadings.RemoveAt(0);

            temperatureSummary.Text = "温度: " + reading.Temperature.ToString("F2") + " °C";
            pressureSummary.Text = "压力: " + reading.Pressure.ToString("F2") + " MPa";
            RenderDeviceHistory(selectedDevice);

            bool temperatureAlarm = reading.Temperature >= 60.0;
            bool pressureAlarm = reading.Pressure >= 1.8;
            bool alarm = temperatureAlarm || pressureAlarm;

            if (alarm)
            {
                string reason = temperatureAlarm && pressureAlarm
                    ? "温度+压力超限"
                    : (temperatureAlarm ? "温度超限" : "压力超限");
                statusBanner.Text = "⚠  " + selectedDevice + " " + reason;
                statusBanner.BackColor = Color.FromArgb(254, 226, 226);
                statusBanner.ForeColor = Color.FromArgb(220, 38, 38);

                if (!previousAlarm)
                    AddAlarmRow(reading, reason);
            }
            else
            {
                statusBanner.Text = "●  正在模拟监控 " + selectedDevice + "，系统正常运行";
                statusBanner.BackColor = Color.FromArgb(226, 232, 240);
                statusBanner.ForeColor = darkText;

                if (previousAlarm && activeAlarmRecord != null)
                    CloseActiveAlarm(reading.Timestamp);
            }

            previousAlarm = alarm;
        }

        private void RenderDeviceHistory(string deviceId)
        {
            temperatureChart.Series[0].Points.Clear();
            pressureChart.Series[0].Points.Clear();

            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(deviceId, out deviceReadings))
                return;

            for (int i = 0; i < deviceReadings.Count; i++)
            {
                temperatureChart.Series[0].Points.AddXY(i, deviceReadings[i].Temperature);
                pressureChart.Series[0].Points.AddXY(i, deviceReadings[i].Pressure);
            }
        }

        private void ShowCachedDeviceSummary(string deviceId)
        {
            List<SensorReading> deviceReadings;
            if (!readingsByDevice.TryGetValue(deviceId, out deviceReadings) || deviceReadings.Count == 0)
            {
                temperatureSummary.Text = "温度: -- °C";
                pressureSummary.Text = "压力: -- MPa";
                statusBanner.Text = "●  " + deviceId + " 尚无模拟数据（监控已停止）";
                statusBanner.BackColor = Color.FromArgb(226, 232, 240);
                statusBanner.ForeColor = darkText;
                return;
            }

            SensorReading latest = deviceReadings[deviceReadings.Count - 1];
            temperatureSummary.Text = "温度: " + latest.Temperature.ToString("F2") + " °C";
            pressureSummary.Text = "压力: " + latest.Pressure.ToString("F2") + " MPa";

            bool alarm = latest.Temperature >= 60.0 || latest.Pressure >= 1.8;
            statusBanner.Text = alarm
                ? "⚠  " + deviceId + " 上次读数超限（监控已停止）"
                : "●  " + deviceId + " 已显示保留数据（监控已停止）";
            statusBanner.BackColor = alarm
                ? Color.FromArgb(254, 226, 226)
                : Color.FromArgb(226, 232, 240);
            statusBanner.ForeColor = alarm
                ? Color.FromArgb(220, 38, 38)
                : darkText;
        }

        private void CloseActiveAlarm(DateTime endTime)
        {
            if (activeAlarmRecord != null)
                activeAlarmRecord.EndTime = endTime;

            activeAlarmRecord = null;
            RenderAlarmPage();
        }

        private void AddAlarmRow(SensorReading reading, string reason)
        {
            string peak = reason.Contains("温度")
                ? reading.Temperature.ToString("F2")
                : reading.Pressure.ToString("F2");

            var alarm = new AlarmRecord
            {
                Id = nextAlarmId++,
                Reading = reading,
                Reason = reason,
                Peak = peak,
                StartTime = reading.Timestamp
            };
            alarmRecords.Insert(0, alarm);
            activeAlarmRecord = alarm;

            if (alarmRecords.Count > MaximumAlarmRecords)
                alarmRecords.RemoveAt(alarmRecords.Count - 1);

            // 新报警加入列表顶部，切回第一页让它立即可见。
            currentAlarmPage = 1;
            RenderAlarmPage();
        }

        private int GetAlarmPageCount()
        {
            return Math.Max(1, (int)Math.Ceiling(alarmRecords.Count / (double)AlarmPageSize));
        }

        private void ChangeAlarmPage(int page)
        {
            currentAlarmPage = Math.Max(1, Math.Min(page, GetAlarmPageCount()));
            RenderAlarmPage();
        }

        private void RenderAlarmPage()
        {
            if (historyGrid == null)
                return;

            int pageCount = GetAlarmPageCount();
            currentAlarmPage = Math.Max(1, Math.Min(currentAlarmPage, pageCount));
            int startIndex = (currentAlarmPage - 1) * AlarmPageSize;
            int endIndex = Math.Min(startIndex + AlarmPageSize, alarmRecords.Count);

            historyGrid.Rows.Clear();
            activeAlarmRecord = activeAlarmRecord != null && alarmRecords.Contains(activeAlarmRecord)
                ? activeAlarmRecord
                : null;

            for (int i = startIndex; i < endIndex; i++)
            {
                AlarmRecord alarm = alarmRecords[i];
                string endTime = alarm.EndTime.HasValue
                    ? alarm.EndTime.Value.ToString("MM-dd HH:mm:ss")
                    : "处理中";

                int rowIndex = historyGrid.Rows.Add(
                    alarm.Id.ToString(),
                    alarm.Reading.DeviceId,
                    alarm.Reason,
                    alarm.Peak,
                    "温度阈值 60.0 °C / 压力阈值 1.8 MPa",
                    alarm.StartTime.ToString("MM-dd HH:mm:ss"),
                    endTime);

                if (ReferenceEquals(alarm, activeAlarmRecord))
                    historyGrid.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38);
            }

            UpdateAlarmPager();
        }

        private void UpdateAlarmPager()
        {
            if (alarmPageInfo == null)
                return;

            int pageCount = GetAlarmPageCount();
            alarmPageInfo.Text = "第 " + currentAlarmPage + "/" + pageCount
                + " 页（共 " + alarmRecords.Count + " 条）";
            firstAlarmPageButton.Enabled = previousAlarmPageButton.Enabled = currentAlarmPage > 1;
            nextAlarmPageButton.Enabled = lastAlarmPageButton.Enabled = currentAlarmPage < pageCount;

            Color enabledColor = Color.FromArgb(55, 130, 245);
            Color disabledColor = Color.FromArgb(180, 190, 205);
            firstAlarmPageButton.BackColor = previousAlarmPageButton.BackColor = currentAlarmPage > 1 ? enabledColor : disabledColor;
            nextAlarmPageButton.BackColor = lastAlarmPageButton.BackColor = currentAlarmPage < pageCount ? enabledColor : disabledColor;
        }
    }
}
