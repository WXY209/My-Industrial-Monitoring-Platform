using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>连接状态、统计信息及操作按钮控件（仅界面）。</summary>
    public sealed class CommunicationStatusControl : UserControl
    {
        public CommunicationStatusControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("连接状态", out body));
            BuildStatus(body);
        }

        private void BuildStatus(Panel parent)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4, Padding = new Padding(2, 4, 2, 0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 24F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));

            layout.Controls.Add(CreateStatusItem("当前状态", CreateValue("●  未连接", Color.FromArgb(239, 68, 68))), 0, 0);
            layout.Controls.Add(CreateStatusItem("最后通信时间", CreateValue("—", CommunicationUi.Muted)), 1, 0);
            layout.Controls.Add(CreateStatusItem("成功次数", CreateValue("0", Color.FromArgb(16, 185, 129))), 0, 1);
            layout.Controls.Add(CreateStatusItem("失败次数", CreateValue("0", Color.FromArgb(239, 68, 68))), 1, 1);

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
            actions.Controls.Add(CommunicationUi.CreateButton("连接", Color.FromArgb(16, 185, 129)));
            actions.Controls.Add(CommunicationUi.CreateButton("断开", Color.FromArgb(100, 116, 139)));
            actions.Controls.Add(CommunicationUi.CreateButton("读取测试", CommunicationUi.Accent));
            layout.Controls.Add(actions, 0, 2);
            layout.SetColumnSpan(actions, 2);

            var note = new Label
            {
                Text = "界面原型：尚未连接真实设备",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 8F),
                ForeColor = Color.FromArgb(148, 163, 184)
            };
            layout.Controls.Add(note, 0, 3);
            layout.SetColumnSpan(note, 2);
            parent.Controls.Add(layout);
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
