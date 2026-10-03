using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>历史采样记录控件，支持日期、设备筛选及分页。</summary>
    public sealed class HistoryRecordControl : UserControl
    {
        private const int PageSize = 15;
        private int currentPage = 1;
        private int totalPages = 1;
        private DataGridView grid;
        private DateTimePicker startDate;
        private DateTimePicker endDate;
        private ComboBox deviceFilter;
        private Label pageInfo;
        private NumericUpDown jumpPage;
        private readonly IRecordService recordService;
        private int loadVersion;

        public HistoryRecordControl() : this(new RecordService())
        {
        }

        public HistoryRecordControl(IRecordService recordService)
        {
            if (recordService == null) throw new ArgumentNullException("recordService");
            this.recordService = recordService;
            RecordControlFactory.Build(this, "◧  历史数据记录", true);
            grid = FindControl<DataGridView>("historyGrid");
            startDate = FindControl<DateTimePicker>("historyStartDate");
            endDate = FindControl<DateTimePicker>("historyEndDate");
            deviceFilter = FindControl<ComboBox>("historyDevice");
            pageInfo = FindControl<Label>("historyPageInfo");
            jumpPage = FindControl<NumericUpDown>("historyJump");

            FindControl<Button>("historyQuery").Click += async (s, e) => await LoadPageAsync(1);
            FindControl<Button>("historyRefresh").Click += async (s, e) => { await LoadDevicesAsync(); await LoadPageAsync(currentPage); };
            FindControl<Button>("historyFirst").Click += async (s, e) => await LoadPageAsync(1);
            FindControl<Button>("historyPrevious").Click += async (s, e) => await LoadPageAsync(currentPage - 1);
            FindControl<Button>("historyNext").Click += async (s, e) => await LoadPageAsync(currentPage + 1);
            FindControl<Button>("historyLast").Click += async (s, e) => await LoadPageAsync(totalPages);
            FindControl<Button>("historyGo").Click += async (s, e) => await LoadPageAsync((int)jumpPage.Value);
        }

        public async Task LoadRecordsAsync()
        {
            await LoadDevicesAsync();
            await LoadPageAsync(currentPage);
        }

        private async Task LoadDevicesAsync()
        {
            string selectedDevice = deviceFilter.SelectedItem == null ? "全部" : deviceFilter.SelectedItem.ToString();
            try
            {
                DataTable devices = await recordService.GetActiveDevicesAsync();
                if (IsDisposed || Disposing) return;
                deviceFilter.Items.Clear(); deviceFilter.Items.Add("全部");
                foreach (DataRow row in devices.Rows) deviceFilter.Items.Add(row["DeviceId"].ToString());
                int selectedIndex = deviceFilter.Items.IndexOf(selectedDevice);
                deviceFilter.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("读取设备列表失败：" + ex.Message, "数据记录", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPageAsync(int requestedPage)
        {
            DateTime start = startDate.Value.Date;
            DateTime endExclusive = endDate.Value.Date.AddDays(1);
            if (endExclusive <= start)
            {
                MessageBox.Show("结束日期不能早于开始日期。", "日期范围", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int requestVersion = ++loadVersion;
            string deviceId = deviceFilter.SelectedIndex <= 0 ? string.Empty : deviceFilter.SelectedItem.ToString();
            try
            {
                int total = await recordService.GetHistoryCountAsync(start, endExclusive, deviceId);
                if (requestVersion != loadVersion || IsDisposed || Disposing) return;
                int pages = Math.Max(1, (total + PageSize - 1) / PageSize);
                int targetPage = Math.Max(1, Math.Min(requestedPage, pages));
                DataTable rows = await recordService.GetHistoryPageAsync(start, endExclusive, deviceId, targetPage, PageSize);
                if (requestVersion != loadVersion || IsDisposed || Disposing) return;

                var displayRows = new DataTable();
                foreach (DataColumn column in rows.Columns) displayRows.Columns.Add(column.ColumnName, typeof(string));
                foreach (DataRow row in rows.Rows)
                {
                    DateTime timestamp = DateTime.Parse(row["采样时间"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    displayRows.Rows.Add(row["编号"].ToString(), row["设备"].ToString(), timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        Convert.ToDouble(row["温度"], CultureInfo.InvariantCulture).ToString("F2", CultureInfo.InvariantCulture) + " °C",
                        Convert.ToDouble(row["压力"], CultureInfo.InvariantCulture).ToString("F2", CultureInfo.InvariantCulture) + " MPa");
                }
                totalPages = pages; currentPage = targetPage;
                grid.DataSource = displayRows;
                pageInfo.Text = "第 " + currentPage + "/" + totalPages + " 页（共 " + total + " 条）";
                jumpPage.Maximum = totalPages; jumpPage.Value = currentPage; UpdatePagerButtons();
            }
            catch (Exception ex) { if (!IsDisposed && !Disposing) MessageBox.Show("查询历史数据失败：" + ex.Message, "数据记录", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void UpdatePagerButtons()
        {
            FindControl<Button>("historyFirst").Enabled = currentPage > 1;
            FindControl<Button>("historyPrevious").Enabled = currentPage > 1;
            FindControl<Button>("historyNext").Enabled = currentPage < totalPages;
            FindControl<Button>("historyLast").Enabled = currentPage < totalPages;
        }

        private T FindControl<T>(string name) where T : Control
        {
            Control[] found = Controls.Find(name, true);
            if (found.Length == 0) throw new InvalidOperationException("找不到控件：" + name);
            return (T)found[0];
        }
    }
}
