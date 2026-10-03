using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>首页的监控操作、设备切换和状态摘要控件。</summary>
    public sealed class OverviewToolbarControl : UserControl
    {
        public event EventHandler StartClicked, StopClicked, AddDeviceClicked, DeleteDeviceClicked, RestoreDeviceClicked;
        public event Action<string> DeviceSelected;
        private readonly Button startButton, stopButton;
        private readonly FlowLayoutPanel tabs;
        private readonly Label runSummary, deviceSummary, deviceCountSummary, temperatureSummary, pressureSummary, statusBanner;
        private readonly Color textColor = Color.FromArgb(44, 62, 80);
        private string selectedDevice;

        public OverviewToolbarControl()
        {
            BackColor = Color.FromArgb(241, 245, 249);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = BackColor, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); Controls.Add(layout);
            var toolbar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.White, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0, 0, 0, 6) };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); layout.Controls.Add(toolbar, 0, 0);
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.White, Margin = Padding.Empty };
            startButton = ActionButton("▶ 开始监控", Color.FromArgb(16, 185, 129)); stopButton = ActionButton("■ 停止监控", Color.FromArgb(239, 83, 80));
            Button add = ActionButton("＋ 添加设备", Color.FromArgb(55, 130, 245)); Button delete = ActionButton("× 删除设备", Color.FromArgb(100, 112, 130)); Button restore = ActionButton("↻ 恢复设备", Color.FromArgb(100, 112, 130));
            startButton.Click += (s, e) => Raise(StartClicked); stopButton.Click += (s, e) => Raise(StopClicked); add.Click += (s, e) => Raise(AddDeviceClicked); delete.Click += (s, e) => Raise(DeleteDeviceClicked); restore.Click += (s, e) => Raise(RestoreDeviceClicked);
            actions.Controls.AddRange(new Control[] { startButton, stopButton, add, delete, restore }); toolbar.Controls.Add(actions, 0, 0);
            var summary = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 7, 0, 0), Margin = Padding.Empty };
            runSummary = Summary("运行: 0", Color.FromArgb(55, 130, 245)); deviceCountSummary = Summary("设备数: 1", Color.FromArgb(16, 185, 129)); pressureSummary = Summary("压力: -- MPa", Color.FromArgb(55, 130, 245)); temperatureSummary = Summary("温度: -- °C", Color.FromArgb(239, 83, 80)); deviceSummary = Summary("设备: DEV-001", textColor);
            summary.Controls.AddRange(new Control[] { runSummary, deviceCountSummary, pressureSummary, temperatureSummary, deviceSummary }); toolbar.Controls.Add(summary, 1, 0);
            statusBanner = new Label { Dock = DockStyle.Fill, Text = "●  系统正常运行", TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(226, 232, 240), ForeColor = textColor, Font = new Font("微软雅黑", 8.5F, FontStyle.Bold), Margin = new Padding(0, 2, 0, 4) }; layout.Controls.Add(statusBanner, 0, 1);
            tabs = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoScroll = true, BackColor = BackColor, Margin = Padding.Empty, Padding = new Padding(0, 2, 0, 0) }; layout.Controls.Add(tabs, 0, 2);
        }

        public void SetDevices(IList<string> devices, string selected)
        {
            selectedDevice = selected; tabs.Controls.Clear();
            if (devices == null) return;
            foreach (string id in devices)
            {
                var button = new Button { Text = "▦  " + id, Tag = id, AutoSize = true, Height = 28, FlatStyle = FlatStyle.Flat, Font = new Font("微软雅黑", 8.5F, FontStyle.Bold), Margin = new Padding(0, 0, 4, 0), Cursor = Cursors.Hand };
                button.FlatAppearance.BorderSize = 0; SetTabStyle(button, id == selectedDevice); button.Click += (s, e) => { string target = (string)((Button)s).Tag; Action<string> handler = DeviceSelected; if (handler != null) handler(target); }; tabs.Controls.Add(button);
            }
            deviceSummary.Text = "设备: " + selectedDevice; deviceCountSummary.Text = "设备数: " + devices.Count;
        }
        public void SetSelectedDevice(string id) { selectedDevice = id; deviceSummary.Text = "设备: " + id; foreach (Button button in tabs.Controls) SetTabStyle(button, string.Equals((string)button.Tag, id, StringComparison.OrdinalIgnoreCase)); }
        public void SetCounts(int running, int total) { runSummary.Text = "运行: " + running; deviceCountSummary.Text = "设备数: " + total; }
        public void SetValues(double? temperature, double? pressure) { temperatureSummary.Text = temperature.HasValue ? "温度: " + temperature.Value.ToString("F2") + " °C" : "温度: -- °C"; pressureSummary.Text = pressure.HasValue ? "压力: " + pressure.Value.ToString("F2") + " MPa" : "压力: -- MPa"; }
        public void SetBanner(string message, bool alarm) { statusBanner.Text = message; statusBanner.BackColor = alarm ? Color.FromArgb(254, 226, 226) : Color.FromArgb(226, 232, 240); statusBanner.ForeColor = alarm ? Color.FromArgb(220, 38, 38) : textColor; }
        public void SetMonitoringButtons(bool canStart, bool canStop) { startButton.Enabled = canStart; stopButton.Enabled = canStop; }
        private static void SetTabStyle(Button b, bool active) { b.BackColor = active ? Color.White : Color.FromArgb(241, 245, 249); b.ForeColor = active ? Color.FromArgb(55, 130, 245) : Color.FromArgb(60, 70, 85); }
        private static Button ActionButton(string text, Color color) { var b = new Button { Text = text, AutoSize = true, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = color, ForeColor = Color.White, Font = new Font("微软雅黑", 8.5F, FontStyle.Bold), Margin = new Padding(2, 1, 4, 1), Cursor = Cursors.Hand }; b.FlatAppearance.BorderSize = 0; return b; }
        private static Label Summary(string text, Color color) { return new Label { Text = text, AutoSize = true, ForeColor = color, Font = new Font("微软雅黑", 8F, FontStyle.Bold), Margin = new Padding(8, 1, 2, 0) }; }
        private void Raise(EventHandler handler) { if (handler != null) handler(this, EventArgs.Empty); }
    }
}
