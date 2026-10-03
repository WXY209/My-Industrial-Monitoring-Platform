using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>历史采样和报警记录左右分栏页面。</summary>
    public partial class DataRecordPage : UserControl
    {
        private readonly HistoryRecordControl historyControl;
        private readonly AlarmRecordControl alarmControl;

        public DataRecordPage() : this(new RecordService())
        {
        }

        public DataRecordPage(IRecordService recordService)
        {
            if (recordService == null) throw new ArgumentNullException("recordService");
            InitializeComponent();
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.FromArgb(241, 245, 249), Padding = new Padding(6) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            historyControl = new HistoryRecordControl(recordService) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 5, 0) };
            alarmControl = new AlarmRecordControl(recordService) { Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) };
            layout.Controls.Add(historyControl, 0, 0);
            layout.Controls.Add(alarmControl, 1, 0);
            Controls.Add(layout);
        }

        public Task LoadRecordsAsync()
        {
            // 两个记录控件并行刷新各自的查询结果。
            return Task.WhenAll(historyControl.LoadRecordsAsync(), alarmControl.LoadRecordsAsync());
        }
    }
}
