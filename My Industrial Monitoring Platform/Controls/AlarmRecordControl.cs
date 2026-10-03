using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>报警记录控件，支持报警类型筛选及分页。</summary>
    public sealed class AlarmRecordControl : UserControl
    {
        private const int PageSize = 15;
        private int currentPage = 1;
        private int totalPages = 1;
        private DataGridView grid;
        private ComboBox typeFilter;
        private Label pageInfo;
        private NumericUpDown jumpPage;
        private readonly IRecordService recordService;
        private int loadVersion;

        public AlarmRecordControl() : this(new RecordService())
        {
        }

        public AlarmRecordControl(IRecordService recordService)
        {
            if (recordService == null) throw new ArgumentNullException("recordService");
            this.recordService = recordService;
            RecordControlFactory.Build(this, "⚠  报警记录", false);
            grid = FindControl<DataGridView>("alarmGrid");
            typeFilter = FindControl<ComboBox>("alarmType");
            pageInfo = FindControl<Label>("alarmPageInfo");
            jumpPage = FindControl<NumericUpDown>("alarmJump");

            FindControl<Button>("alarmRefresh").Click += async (s, e) => await LoadPageAsync(currentPage);
            typeFilter.SelectedIndexChanged += async (s, e) => await LoadPageAsync(1);
            FindControl<Button>("alarmFirst").Click += async (s, e) => await LoadPageAsync(1);
            FindControl<Button>("alarmPrevious").Click += async (s, e) => await LoadPageAsync(currentPage - 1);
            FindControl<Button>("alarmNext").Click += async (s, e) => await LoadPageAsync(currentPage + 1);
            FindControl<Button>("alarmLast").Click += async (s, e) => await LoadPageAsync(totalPages);
            FindControl<Button>("alarmGo").Click += async (s, e) => await LoadPageAsync((int)jumpPage.Value);
        }

        public Task LoadRecordsAsync()
        {
            return LoadPageAsync(currentPage);
        }

        private async Task LoadPageAsync(int requestedPage)
        {
            int requestVersion = ++loadVersion;
            string alarmType = typeFilter.SelectedIndex <= 0 ? string.Empty : typeFilter.SelectedItem.ToString();
            try
            {
                int total = await recordService.GetAlarmCountAsync(alarmType);
                if (requestVersion != loadVersion || IsDisposed || Disposing) return;
                int pages = Math.Max(1, (total + PageSize - 1) / PageSize);
                int targetPage = Math.Max(1, Math.Min(requestedPage, pages));
                DataTable rows = await recordService.GetAlarmPageAsync(alarmType, targetPage, PageSize);
                if (requestVersion != loadVersion || IsDisposed || Disposing) return;

                var displayRows = new DataTable();
                foreach (DataColumn column in rows.Columns) displayRows.Columns.Add(column.ColumnName, typeof(string));
                foreach (DataRow row in rows.Rows)
                    displayRows.Rows.Add(row["编号"].ToString(), row["设备"].ToString(), row["报警类型"].ToString(),
                        row["报警值"].ToString(), FormatTime(row["开始时间"]), FormatTime(row["结束时间"]), row["状态"].ToString());
                totalPages = pages; currentPage = targetPage; grid.DataSource = displayRows;
                pageInfo.Text = "第 " + currentPage + "/" + totalPages + " 页（共 " + total + " 条）";
                jumpPage.Maximum = totalPages; jumpPage.Value = currentPage; UpdatePagerButtons();
            }
            catch (Exception ex) { if (!IsDisposed && !Disposing) MessageBox.Show("查询报警记录失败：" + ex.Message, "数据记录", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static string FormatTime(object value)
        {
            if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString())) return "—";
            DateTime time = DateTime.Parse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            return time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void UpdatePagerButtons()
        {
            FindControl<Button>("alarmFirst").Enabled = currentPage > 1;
            FindControl<Button>("alarmPrevious").Enabled = currentPage > 1;
            FindControl<Button>("alarmNext").Enabled = currentPage < totalPages;
            FindControl<Button>("alarmLast").Enabled = currentPage < totalPages;
        }

        private T FindControl<T>(string name) where T : Control
        {
            Control[] found = Controls.Find(name, true);
            if (found.Length == 0) throw new InvalidOperationException("找不到控件：" + name);
            return (T)found[0];
        }
    }
}
