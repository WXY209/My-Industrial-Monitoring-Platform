using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信方式和 Modbus RTU/TCP 参数配置控件（仅界面）。</summary>
    public sealed class CommunicationConfigControl : UserControl
    {
        private ComboBox protocolSelector;
        private Panel serialSettings;
        private Panel tcpSettings;

        public CommunicationConfigControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("通信配置", out body));
            BuildConfiguration(body);
        }

        private void BuildConfiguration(Panel parent)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var protocolRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
            protocolRow.Controls.Add(CommunicationUi.CreateFieldLabel("通信方式"));
            protocolSelector = CommunicationUi.CreateCombo("Modbus RTU（串口）", "Modbus TCP");
            protocolSelector.Width = 205;
            protocolRow.Controls.Add(protocolSelector);

            var settingsHost = new Panel { Dock = DockStyle.Fill };
            serialSettings = BuildSerialSettings();
            tcpSettings = BuildTcpSettings();
            settingsHost.Controls.Add(tcpSettings);
            settingsHost.Controls.Add(serialSettings);
            protocolSelector.SelectedIndexChanged += (s, e) => UpdateProtocolSettings();
            UpdateProtocolSettings();

            layout.Controls.Add(protocolRow, 0, 0);
            layout.Controls.Add(settingsHost, 0, 1);
            parent.Controls.Add(layout);
        }

        private Panel BuildSerialSettings()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White };
            var fields = CommunicationUi.CreateFieldsTable(3);
            CommunicationUi.AddFieldPair(fields, 0, "串口号", CommunicationUi.CreateCombo("COM1", "COM2", "COM3", "COM4"),
                "波特率", CommunicationUi.CreateCombo("9600", "19200", "38400", "115200"));
            CommunicationUi.AddFieldPair(fields, 1, "数据位", CommunicationUi.CreateCombo("8", "7"),
                "校验位", CommunicationUi.CreateCombo("None", "Even", "Odd"));
            CommunicationUi.AddFieldPair(fields, 2, "停止位", CommunicationUi.CreateCombo("1", "2"),
                "从站地址", CommunicationUi.CreateNumber(1, 247, 1));
            panel.Controls.Add(fields);
            return panel;
        }

        private Panel BuildTcpSettings()
        {
            var panel = new Panel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White };
            var fields = CommunicationUi.CreateFieldsTable(2);
            CommunicationUi.AddFieldPair(fields, 0, "设备 IP", new TextBox { Text = "192.168.1.100" },
                "端口", CommunicationUi.CreateNumber(1, 65535, 502));
            CommunicationUi.AddFieldPair(fields, 1, "Unit ID", CommunicationUi.CreateNumber(1, 247, 1),
                "超时(ms)", CommunicationUi.CreateNumber(100, 30000, 1000));
            panel.Controls.Add(fields);
            return panel;
        }

        private void UpdateProtocolSettings()
        {
            bool isRtu = protocolSelector.SelectedIndex == 0;
            serialSettings.Visible = isRtu;
            tcpSettings.Visible = !isRtu;
            (isRtu ? serialSettings : tcpSettings).BringToFront();
        }
    }
}
