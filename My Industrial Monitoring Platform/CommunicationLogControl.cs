using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>显示模拟采样和通信操作的日志。</summary>
    public sealed class CommunicationLogControl : UserControl
    {
        private const int MaximumRows = 200;
        private readonly DataGridView grid;

        public CommunicationLogControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("通信日志", out body));
            grid = CommunicationUi.CreateGrid("时间", "方向", "操作/数据", "结果", "说明");
            grid.Columns[0].FillWeight = 105;
            grid.Columns[1].FillWeight = 45;
            grid.Columns[2].FillWeight = 110;
            grid.Columns[3].FillWeight = 55;
            grid.Columns[4].FillWeight = 150;
            body.Controls.Add(grid);
        }

        public void AddEntry(string direction, string operation, string result, string details)
        {
            grid.Rows.Insert(0,
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                direction,
                operation,
                result,
                details);
            int rowIndex = 0;
            grid.Rows[rowIndex].DefaultCellStyle.ForeColor = result == "成功"
                ? Color.FromArgb(51, 65, 85)
                : Color.FromArgb(220, 38, 38);

            while (grid.Rows.Count > MaximumRows)
                grid.Rows.RemoveAt(grid.Rows.Count - 1);
        }
    }
}
