using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>历史采样和报警记录左右分栏页面。</summary>
    public partial class DataRecordPage : UserControl
    {
        public DataRecordPage()
        {
            InitializeComponent();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(6) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.Controls.Add(new HistoryRecordControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 5, 0) }, 0, 0);
            layout.Controls.Add(new AlarmRecordControl { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) }, 1, 0);
            Controls.Add(layout);
        }
    }
}
