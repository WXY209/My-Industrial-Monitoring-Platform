using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>显示模拟采样和通信操作的日志。</summary>
    public sealed class CommunicationLogControl : UserControl
    {
        private const int MaximumRows = 200;
        private readonly DataGridView grid;
        private readonly ICommunicationLogService logService;
        private long nextRowId;

        public CommunicationLogControl() : this(new CommunicationLogService())
        {
        }

        public CommunicationLogControl(ICommunicationLogService logService)
        {
            if (logService == null) throw new ArgumentNullException("logService");
            this.logService = logService;
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("通信日志", out body));
            grid = CommunicationUi.CreateGrid("时间", "方向", "操作/数据", "结果", "说明");
            grid.Columns[0].FillWeight = 105;
            grid.Columns[1].FillWeight = 45;
            grid.Columns[2].FillWeight = 110;
            grid.Columns[3].FillWeight = 55;
            grid.Columns[4].FillWeight = 150;
            body.Controls.Add(grid);

            // 设计器预览时不访问 SQLite，避免打开控件设计器时触发数据库异常。
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                Load += async (s, e) => await LoadRecentEntriesAsync();
        }

        public async void AddEntry(string direction, string operation, string result, string details, string deviceId = null)
        {
            // 先显示日志，再异步保存；数据库失败不会中断通信流程。
            DateTime timestamp = DateTime.Now;
            long rowId = ++nextRowId;
            DataGridViewRow row = InsertRow(timestamp, direction, operation, result, details);
            row.Tag = rowId;
            try
            {
                await logService.AddEntryAsync(timestamp, deviceId, direction, operation, result, details);
            }
            catch (Exception ex)
            {
                // 日志故障不应中断通信流程；只在对应的可见行补充错误信息。
                if (!IsDisposed && !Disposing)
                {
                    foreach (DataGridViewRow current in grid.Rows)
                    {
                        if (current.Tag is long && (long)current.Tag == rowId)
                        {
                            current.Cells[4].Value = details + "（数据库保存失败：" + ex.Message + "）";
                            break;
                        }
                    }
                }
            }
        }

        private async Task LoadRecentEntriesAsync()
        {
            try
            {
                DataTable entries = await logService.GetRecentEntriesAsync(MaximumRows);
                if (IsDisposed || Disposing) return;
                for (int i = entries.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = entries.Rows[i];
                    DateTime timestamp = DateTime.Parse(row["Timestamp"].ToString(),
                        CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    InsertRow(timestamp, row["Direction"].ToString(), row["Operation"].ToString(),
                        row["Result"].ToString(), row["Details"] == DBNull.Value ? string.Empty : row["Details"].ToString());
                }
            }
            catch (Exception ex)
            {
                InsertRow(DateTime.Now, "系统", "加载历史通信日志", "失败", ex.Message);
            }
        }

        private DataGridViewRow InsertRow(DateTime timestamp, string direction, string operation, string result, string details)
        {
            grid.Rows.Insert(0,
                timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                direction,
                operation,
                result,
                details);
            DataGridViewRow row = grid.Rows[0];
            row.DefaultCellStyle.ForeColor = result == "成功"
                ? Color.FromArgb(51, 65, 85)
                : Color.FromArgb(220, 38, 38);

            while (grid.Rows.Count > MaximumRows)
                grid.Rows.RemoveAt(grid.Rows.Count - 1);
            return row;
        }
    }
}
