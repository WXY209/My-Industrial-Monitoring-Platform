using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>设置温度、压力所读寄存器、功能码和缩放系数。</summary>
    public sealed class RegisterMappingControl : UserControl
    {
        private readonly ComboBox functionSelector;
        private readonly ComboBox dataTypeSelector;
        private readonly NumericUpDown temperatureAddressInput;
        private readonly NumericUpDown pressureAddressInput;
        private readonly NumericUpDown temperatureScaleInput;
        private readonly NumericUpDown pressureScaleInput;

        public RegisterMappingControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("寄存器映射", out body));

            var contentHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 4) };
            var layout = new TableLayoutPanel
            {
                Location = Point.Empty,
                Width = 820,
                Height = 260,
                ColumnCount = 1,
                RowCount = 4,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            // 在公共下拉框行与温度映射行之间留出更大的垂直间隔。
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

            var commonRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            commonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            commonRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            functionSelector = CommunicationUi.CreateCombo("03 保持寄存器", "04 输入寄存器");
            dataTypeSelector = CommunicationUi.CreateCombo("无符号 16 位", "有符号 16 位");
            commonRow.Controls.Add(CreateField("功能码", functionSelector), 0, 0);
            commonRow.Controls.Add(CreateField("寄存器格式", dataTypeSelector), 1, 0);

            temperatureAddressInput = CommunicationUi.CreateNumber(0, 65535, 0);
            pressureAddressInput = CommunicationUi.CreateNumber(0, 65535, 1);
            temperatureScaleInput = CreateScale(0.1M);
            pressureScaleInput = CreateScale(0.01M);
            var temperatureRow = CreatePointRow("温度", temperatureAddressInput, temperatureScaleInput);
            var pressureRow = CreatePointRow("压力", pressureAddressInput, pressureScaleInput);

            var note = new Label
            {
                Text = "地址按 0 起始填写。默认地址和倍率仅为示例，请按设备手册修改；32 位浮点数暂不支持。",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 8F),
                ForeColor = CommunicationUi.Muted,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            layout.Controls.Add(commonRow, 0, 0);
            layout.Controls.Add(temperatureRow, 0, 1);
            layout.Controls.Add(pressureRow, 0, 2);
            layout.Controls.Add(note, 0, 3);
            contentHost.Controls.Add(layout);
            contentHost.Resize += (s, e) =>
            {
                layout.Width = System.Math.Max(1, System.Math.Min(820, contentHost.ClientSize.Width));
            };
            body.Controls.Add(contentHost);
        }

        public RegisterFunction SelectedFunction
        {
            get { return functionSelector.SelectedIndex == 1 ? RegisterFunction.InputRegister : RegisterFunction.HoldingRegister; }
        }

        public ushort TemperatureAddress { get { return (ushort)temperatureAddressInput.Value; } }
        public ushort PressureAddress { get { return (ushort)pressureAddressInput.Value; } }
        public double TemperatureScale { get { return (double)temperatureScaleInput.Value; } }
        public double PressureScale { get { return (double)pressureScaleInput.Value; } }
        public bool SignedValues { get { return dataTypeSelector.SelectedIndex == 1; } }

        private static Control CreatePointRow(string title, NumericUpDown address, NumericUpDown scale)
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 8, 0, 4),
                Margin = new Padding(0, 4, 0, 4)
            };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            var pointLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Margin = new Padding(0, 0, 12, 0)
            };
            row.Controls.Add(pointLabel, 0, 0);
            row.Controls.Add(CreateField("寄存器地址（0 起始）", address), 1, 0);
            row.Controls.Add(CreateField("缩放系数", scale), 2, 0);
            return row;
        }

        private static NumericUpDown CreateScale(decimal value)
        {
            return new NumericUpDown
            {
                Minimum = 0.001M,
                Maximum = 1000M,
                Increment = 0.001M,
                DecimalPlaces = 3,
                Value = value,
                Font = new Font("微软雅黑", 8F)
            };
        }

        private static Control CreateField(string title, Control input)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(0, 0, 14, 0) };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var label = CommunicationUi.CreateFieldLabel(title);
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.Margin = Padding.Empty;
            layout.Controls.Add(label, 0, 0);
            // 保留控件本身的默认宽高；间距通过容器留白控制。
            input.Dock = DockStyle.Left;
            input.Margin = new Padding(0, 12, 0, 0);
            layout.Controls.Add(input, 0, 1);
            return layout;
        }
    }
}
