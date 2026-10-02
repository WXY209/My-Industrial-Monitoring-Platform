using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>Modbus 寄存器映射表示例控件。</summary>
    public sealed class RegisterMappingControl : UserControl
    {
        public RegisterMappingControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("寄存器映射（示例）", out body));
            var grid = CommunicationUi.CreateGrid("序号", "监控点", "功能码", "寄存器地址", "数据类型", "系数", "单位");
            grid.Columns[0].FillWeight = 38;
            grid.Columns[1].FillWeight = 75;
            grid.Columns[2].FillWeight = 60;
            grid.Columns[3].FillWeight = 85;
            grid.Columns[4].FillWeight = 75;
            grid.Columns[5].FillWeight = 48;
            grid.Columns[6].FillWeight = 45;
            grid.Rows.Add("1", "温度", "03", "0001", "Int16", "0.1", "°C");
            grid.Rows.Add("2", "压力", "03", "0002", "Int16", "0.01", "MPa");
            body.Controls.Add(grid);
        }
    }
}
