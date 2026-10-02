using System;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信运行状态、采样统计和操作按钮。</summary>
    public sealed class CommunicationStatusControl : UserControl
    {
        private readonly Label stateValue;
        private readonly Label timeValue;
        private readonly Label successValue;
        private readonly Label failureValue;
        private readonly Label note;
        private int successCount;
        private int failureCount;

        public event EventHandler ConnectRequested;
        public event EventHandler DisconnectRequested;
        public event EventHandler TestReadRequested;

        public CommunicationStatusControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("连接状态", out body));

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(2, 4, 2, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));

            stateValue = CreateValue("●  已停止", Color.FromArgb(100, 116, 139));
            timeValue = CreateValue("—", CommunicationUi.Muted);
            successValue = CreateValue("0", Color.FromArgb(16, 185, 129));
            failureValue = CreateValue("0", Color.FromArgb(239, 68, 68));
            layout.Controls.Add(CreateStatusItem("当前状态", stateValue), 0, 0);
            layout.Controls.Add(CreateStatusItem("最后采样时间", timeValue), 1, 0);
            layout.Controls.Add(CreateStatusItem("成功次数", successValue), 0, 1);
            layout.Controls.Add(CreateStatusItem("失败次数", failureValue), 1, 1);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
            var connectButton = CommunicationUi.CreateButton("连接", Color.FromArgb(16, 185, 129));
            var disconnectButton = CommunicationUi.CreateButton("断开", Color.FromArgb(100, 116, 139));
            var testButton = CommunicationUi.CreateButton("读取测试", CommunicationUi.Accent);
            connectButton.Click += (s, e) => { var handler = ConnectRequested; if (handler != null) handler(this, EventArgs.Empty); };
            disconnectButton.Click += (s, e) => { var handler = DisconnectRequested; if (handler != null) handler(this, EventArgs.Empty); };
            testButton.Click += (s, e) => { var handler = TestReadRequested; if (handler != null) handler(this, EventArgs.Empty); };
            actions.Controls.Add(connectButton);
            actions.Controls.Add(disconnectButton);
            actions.Controls.Add(testButton);
            layout.Controls.Add(actions, 0, 2);
            layout.SetColumnSpan(actions, 2);

            note = new Label
            {
                Text = "当前未连接",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(148, 163, 184)
            };
            layout.Controls.Add(note, 0, 3);
            layout.SetColumnSpan(note, 2);
            body.Controls.Add(layout);
        }

        public void SetConnecting(string mode)
        {
            stateValue.Text = "●  正在连接 " + mode;
            stateValue.ForeColor = CommunicationUi.Accent;
            note.Text = "正在建立连接，请稍候。";
        }

        public void SetRunning(string mode, string deviceId)
        {
            stateValue.Text = "●  " + mode + " 运行中：" + deviceId;
            stateValue.ForeColor = Color.FromArgb(16, 185, 129);
            note.Text = mode == "模拟" ? "数据由项目内置模拟器生成，不是实际设备通信。" : "已连接设备并开始采样。";
        }

        public void SetStopped(string message = "当前未连接")
        {
            stateValue.Text = "●  已停止";
            stateValue.ForeColor = Color.FromArgb(100, 116, 139);
            note.Text = message;
        }

        public void RecordSuccess(DateTime timestamp)
        {
            successCount++;
            successValue.Text = successCount.ToString();
            timeValue.Text = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void RecordFailure()
        {
            failureCount++;
            failureValue.Text = failureCount.ToString();
        }

        private Control CreateStatusItem(string title, Control value)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(4, 4, 4, 0) };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            var label = CommunicationUi.CreateFieldLabel(title);
            value.Dock = DockStyle.Fill;
            layout.Controls.Add(label, 0, 0);
            layout.Controls.Add(value, 0, 1);
            return layout;
        }

        private static Label CreateValue(string text, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("微软雅黑", 10F, FontStyle.Bold),
                ForeColor = color,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }
    }
}
