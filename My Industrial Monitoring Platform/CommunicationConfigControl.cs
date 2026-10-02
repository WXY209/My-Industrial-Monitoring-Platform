using System;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    public enum CommunicationMode
    {
        Simulation,
        ModbusRtu,
        ModbusTcp
    }

    public enum RegisterFunction
    {
        HoldingRegister,
        InputRegister
    }

    /// <summary>本次连接和采样所用的通信参数。</summary>
    public sealed class CommunicationSettings
    {
        public CommunicationMode Mode { get; set; }
        public string DeviceId { get; set; }
        public int SamplingInterval { get; set; }
        public string SerialPortName { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public string IpAddress { get; set; }
        public int TcpPort { get; set; }
        public byte UnitId { get; set; }
        public int Timeout { get; set; }
        public RegisterFunction Function { get; set; }
        public ushort TemperatureAddress { get; set; }
        public ushort PressureAddress { get; set; }
        public double TemperatureScale { get; set; }
        public double PressureScale { get; set; }
        public bool SignedValues { get; set; }
    }

    /// <summary>模拟、Modbus RTU 和 Modbus TCP 的连接参数。</summary>
    public sealed class CommunicationConfigControl : UserControl
    {
        private readonly ComboBox modeSelector;
        private readonly ComboBox deviceSelector;
        private readonly Panel settingsHost;
        private readonly Panel serialSettings;
        private readonly Panel tcpSettings;
        private readonly Panel simulationSettings;
        private readonly ComboBox portSelector;
        private readonly ComboBox baudRateSelector;
        private readonly ComboBox dataBitsSelector;
        private readonly ComboBox paritySelector;
        private readonly ComboBox stopBitsSelector;
        private readonly NumericUpDown rtuUnitIdInput;
        private readonly NumericUpDown rtuTimeoutInput;
        private readonly TextBox ipAddressInput;
        private readonly NumericUpDown tcpPortInput;
        private readonly NumericUpDown tcpUnitIdInput;
        private readonly NumericUpDown tcpTimeoutInput;
        private readonly NumericUpDown intervalInput;

        public CommunicationConfigControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("通信配置", out body));

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(8, 6, 8, 2)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));

            var modeRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Padding = new Padding(0, 2, 0, 0) };
            modeRow.Controls.Add(CommunicationUi.CreateFieldLabel("模式"));
            modeSelector = CommunicationUi.CreateCombo("模拟模式", "Modbus RTU（串口）", "Modbus TCP");
            modeSelector.Width = 180;
            modeSelector.SelectedIndexChanged += (s, e) => UpdateModeSettings();
            modeRow.Controls.Add(modeSelector);
            modeRow.Controls.Add(CommunicationUi.CreateFieldLabel("设备"));
            deviceSelector = CommunicationUi.CreateCombo();
            deviceSelector.Width = 110;
            deviceSelector.DropDown += DeviceSelector_DropDown;
            modeRow.Controls.Add(deviceSelector);

            serialSettings = new Panel { Dock = DockStyle.Fill };
            var serialGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
            serialGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            serialGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 4; i++) serialGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            portSelector = CommunicationUi.CreateCombo("选择串口");
            portSelector.DropDown += PortSelector_DropDown;
            baudRateSelector = CommunicationUi.CreateCombo("9600", "19200", "38400", "57600", "115200");
            dataBitsSelector = CommunicationUi.CreateCombo("8", "7");
            paritySelector = CommunicationUi.CreateCombo("None", "Even", "Odd");
            stopBitsSelector = CommunicationUi.CreateCombo("1", "2");
            rtuUnitIdInput = CommunicationUi.CreateNumber(1, 247, 1);
            rtuTimeoutInput = CommunicationUi.CreateNumber(100, 60000, 1000);
            serialGrid.Controls.Add(CreateField("串口", portSelector), 0, 0);
            serialGrid.Controls.Add(CreateField("波特率", baudRateSelector), 1, 0);
            serialGrid.Controls.Add(CreateField("数据位", dataBitsSelector), 0, 1);
            serialGrid.Controls.Add(CreateField("校验位", paritySelector), 1, 1);
            serialGrid.Controls.Add(CreateField("停止位", stopBitsSelector), 0, 2);
            serialGrid.Controls.Add(CreateField("从站地址", rtuUnitIdInput), 1, 2);
            serialGrid.Controls.Add(CreateField("超时（ms）", rtuTimeoutInput), 0, 3);
            serialSettings.Controls.Add(serialGrid);

            tcpSettings = new Panel { Dock = DockStyle.Fill };
            var tcpGrid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            tcpGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tcpGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tcpGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tcpGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            ipAddressInput = new TextBox { Text = "127.0.0.1", Font = new Font("微软雅黑", 8.5F) };
            tcpPortInput = CommunicationUi.CreateNumber(1, 65535, 502);
            tcpUnitIdInput = CommunicationUi.CreateNumber(1, 247, 1);
            tcpTimeoutInput = CommunicationUi.CreateNumber(100, 60000, 1000);
            tcpGrid.Controls.Add(CreateField("设备 IP", ipAddressInput), 0, 0);
            tcpGrid.Controls.Add(CreateField("端口", tcpPortInput), 1, 0);
            tcpGrid.Controls.Add(CreateField("Unit ID", tcpUnitIdInput), 0, 1);
            tcpGrid.Controls.Add(CreateField("超时（ms）", tcpTimeoutInput), 1, 1);
            tcpSettings.Controls.Add(tcpGrid);

            simulationSettings = new Panel { Dock = DockStyle.Fill };
            simulationSettings.Controls.Add(new Label
            {
                Text = "从项目内置模拟器生成数据；不会连接真实设备。",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 8.5F),
                ForeColor = CommunicationUi.Muted,
                TextAlign = ContentAlignment.MiddleLeft
            });

            settingsHost = new Panel { Dock = DockStyle.Fill };
            settingsHost.Controls.Add(tcpSettings);
            settingsHost.Controls.Add(serialSettings);
            settingsHost.Controls.Add(simulationSettings);

            intervalInput = CommunicationUi.CreateNumber(250, 60000, 1000);
            intervalInput.Increment = 250;
            intervalInput.Width = 110;
            var intervalRow = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            intervalRow.Controls.Add(CommunicationUi.CreateFieldLabel("采样间隔（ms）"));
            intervalRow.Controls.Add(intervalInput);

            var mapNote = new Label
            {
                Text = "寄存器地址按 0 起始填写；温度/压力地址与比例系数见下方映射。",
                Dock = DockStyle.Fill,
                ForeColor = CommunicationUi.Muted,
                Font = new Font("微软雅黑", 7.5F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            layout.Controls.Add(modeRow, 0, 0);
            layout.Controls.Add(settingsHost, 0, 1);
            layout.Controls.Add(intervalRow, 0, 2);
            layout.Controls.Add(mapNote, 0, 3);
            var note = new Label
            {
                Text = "Modbus 仅支持 16 位寄存器读取；具体地址和倍率需按设备手册设置。",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 7.5F),
                ForeColor = Color.FromArgb(148, 163, 184),
                TextAlign = ContentAlignment.MiddleLeft
            };
            layout.Controls.Add(note, 0, 4);
            body.Controls.Add(layout);
            UpdateModeSettings();
        }

        public CommunicationMode Mode { get { return (CommunicationMode)modeSelector.SelectedIndex; } }
        public string SelectedDeviceId { get { return deviceSelector.SelectedItem == null ? null : deviceSelector.SelectedItem.ToString(); } }

        public void RefreshDevices()
        {
            string previous = SelectedDeviceId;
            deviceSelector.Items.Clear();
            DataTable devices = DeviceDB.GetActiveDevices();
            foreach (DataRow row in devices.Rows)
                deviceSelector.Items.Add(row["DeviceId"].ToString());
            if (deviceSelector.Items.Count == 0)
                throw new InvalidOperationException("当前没有可用设备，请先在首页总览中添加设备。");
            int index = previous == null ? -1 : deviceSelector.Items.IndexOf(previous);
            deviceSelector.SelectedIndex = index >= 0 ? index : 0;
        }

        public CommunicationSettings GetSettings(RegisterMappingControl mapping)
        {
            if (string.IsNullOrWhiteSpace(SelectedDeviceId))
                throw new InvalidOperationException("请先选择要关联的设备。");

            var settings = new CommunicationSettings
            {
                Mode = Mode,
                DeviceId = SelectedDeviceId,
                SamplingInterval = (int)intervalInput.Value,
                Function = mapping.SelectedFunction,
                TemperatureAddress = mapping.TemperatureAddress,
                PressureAddress = mapping.PressureAddress,
                TemperatureScale = mapping.TemperatureScale,
                PressureScale = mapping.PressureScale,
                SignedValues = mapping.SignedValues,
                Timeout = Mode == CommunicationMode.ModbusTcp ? (int)tcpTimeoutInput.Value : (int)rtuTimeoutInput.Value
            };

            if (Mode == CommunicationMode.ModbusRtu)
            {
                if (portSelector.SelectedItem == null || portSelector.SelectedItem.ToString() == "选择串口")
                    throw new InvalidOperationException("未选择有效串口；请检查 RS-485 转 USB 适配器及驱动。");
                settings.SerialPortName = portSelector.SelectedItem.ToString();
                settings.BaudRate = int.Parse(baudRateSelector.SelectedItem.ToString());
                settings.DataBits = int.Parse(dataBitsSelector.SelectedItem.ToString());
                settings.Parity = (Parity)Enum.Parse(typeof(Parity), paritySelector.SelectedItem.ToString(), true);
                settings.StopBits = stopBitsSelector.SelectedItem.ToString() == "2" ? StopBits.Two : StopBits.One;
                settings.UnitId = (byte)rtuUnitIdInput.Value;
            }
            else if (Mode == CommunicationMode.ModbusTcp)
            {
                settings.IpAddress = ipAddressInput.Text.Trim();
                settings.TcpPort = (int)tcpPortInput.Value;
                settings.UnitId = (byte)tcpUnitIdInput.Value;
            }

            return settings;
        }

        private void UpdateModeSettings()
        {
            simulationSettings.Visible = Mode == CommunicationMode.Simulation;
            serialSettings.Visible = Mode == CommunicationMode.ModbusRtu;
            tcpSettings.Visible = Mode == CommunicationMode.ModbusTcp;
            if (simulationSettings.Visible) simulationSettings.BringToFront();
            else if (serialSettings.Visible) serialSettings.BringToFront();
            else tcpSettings.BringToFront();
        }

        private void DeviceSelector_DropDown(object sender, EventArgs e)
        {
            try { RefreshDevices(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "设备列表", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void PortSelector_DropDown(object sender, EventArgs e)
        {
            string previous = portSelector.SelectedItem == null ? null : portSelector.SelectedItem.ToString();
            portSelector.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports, StringComparer.OrdinalIgnoreCase);
            if (ports.Length == 0)
                portSelector.Items.Add("选择串口");
            else
                portSelector.Items.AddRange(ports);
            int index = previous == null ? -1 : portSelector.Items.IndexOf(previous);
            portSelector.SelectedIndex = index >= 0 ? index : 0;
        }

        private static Control CreateField(string title, Control input)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, 4, 0) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var label = CommunicationUi.CreateFieldLabel(title);
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.Margin = Padding.Empty;
            layout.Controls.Add(label, 0, 0);
            input.Dock = DockStyle.Left;
            input.Margin = new Padding(0, 1, 0, 0);
            layout.Controls.Add(input, 0, 1);
            return layout;
        }
    }
}
