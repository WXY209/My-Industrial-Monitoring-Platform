using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    public sealed class OverviewAlarmItem
    {
        public string Id { get; set; }
        public string Device { get; set; }
        public string Type { get; set; }
        public string Peak { get; set; }
        public string Details { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>首页报警记录表格与分页控件。</summary>
    public sealed class OverviewAlarmControl : UserControl
    {
        private const int PageSize = 15;
        private readonly DataGridView grid;
        private readonly Label pageInfo;
        private readonly Button first, previous, next, last;
        private IList<OverviewAlarmItem> items = new List<OverviewAlarmItem>();
        private int page = 1;

        public OverviewAlarmControl()
        {
            BackColor = Color.White; Padding = new Padding(8);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.White, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); Controls.Add(layout);
            var header = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.White, Margin = Padding.Empty };
            header.Controls.Add(new Label { Text = "▤  历史报警记录", AutoSize = true, ForeColor = Color.FromArgb(57, 75, 96), Font = new Font("微软雅黑", 8.5F, FontStyle.Bold), Margin = new Padding(0, 5, 10, 0) });
            var refresh = new Button { Text = "⟳ 刷新", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 130, 245), ForeColor = Color.White, Font = new Font("微软雅黑", 8F), Cursor = Cursors.Hand }; refresh.FlatAppearance.BorderSize = 0; refresh.Click += (s, e) => RenderPage(); header.Controls.Add(refresh); layout.Controls.Add(header, 0, 0);
            grid = new DataGridView { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, ReadOnly = true, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 32, RowTemplate = { Height = 31 } };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(66, 78, 95), Font = new Font("微软雅黑", 8F, FontStyle.Bold), SelectionBackColor = Color.White, SelectionForeColor = Color.FromArgb(66, 78, 95) };
            grid.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("微软雅黑", 8F), ForeColor = Color.FromArgb(65, 72, 82), SelectionBackColor = Color.FromArgb(239, 245, 252), SelectionForeColor = Color.FromArgb(65, 72, 82), Padding = new Padding(3) };
            string[] headers = { "编号", "设备", "类型", "峰值", "阈值详情", "开始时间", "结束时间" }; string[] names = { "Id", "Device", "Type", "Value", "Details", "Start", "End" };
            for (int i = 0; i < headers.Length; i++) grid.Columns.Add(new DataGridViewTextBoxColumn { Name = names[i], HeaderText = headers[i], FillWeight = i == 4 ? 150 : (i >= 5 ? 125 : 70) });
            layout.Controls.Add(grid, 0, 1);
            var footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.White, Padding = new Padding(0, 2, 0, 0), Margin = Padding.Empty };
            last = MakePageButton("末页 ▶▶"); next = MakePageButton("▶"); previous = MakePageButton("◀"); first = MakePageButton("首页 ◀◀");
            first.Click += (s, e) => ChangePage(1); previous.Click += (s, e) => ChangePage(page - 1); next.Click += (s, e) => ChangePage(page + 1); last.Click += (s, e) => ChangePage(PageCount);
            footer.Controls.Add(last); footer.Controls.Add(next); pageInfo = new Label { AutoSize = true, ForeColor = Color.Gray, Font = new Font("微软雅黑", 8F), Margin = new Padding(8, 7, 8, 0) }; footer.Controls.Add(pageInfo); footer.Controls.Add(previous); footer.Controls.Add(first); layout.Controls.Add(footer, 0, 2); RenderPage();
        }

        public void SetItems(IList<OverviewAlarmItem> alarmItems, bool goToFirstPage)
        {
            items = alarmItems ?? new List<OverviewAlarmItem>(); if (goToFirstPage) page = 1; RenderPage();
        }
        private int PageCount { get { return Math.Max(1, (int)Math.Ceiling(items.Count / (double)PageSize)); } }
        private void ChangePage(int value) { page = Math.Max(1, Math.Min(value, PageCount)); RenderPage(); }
        private void RenderPage()
        {
            if (grid == null) return; page = Math.Max(1, Math.Min(page, PageCount)); grid.Rows.Clear();
            int end = Math.Min(page * PageSize, items.Count);
            for (int i = (page - 1) * PageSize; i < end; i++) { OverviewAlarmItem item = items[i]; int row = grid.Rows.Add(item.Id, item.Device, item.Type, item.Peak, item.Details, item.Start, item.End); if (item.IsActive) grid.Rows[row].DefaultCellStyle.ForeColor = Color.FromArgb(220, 38, 38); }
            pageInfo.Text = "第 " + page + "/" + PageCount + " 页（共 " + items.Count + " 条）";
            first.Enabled = previous.Enabled = page > 1; next.Enabled = last.Enabled = page < PageCount;
            Color enabled = Color.FromArgb(55, 130, 245), disabled = Color.FromArgb(180, 190, 205); first.BackColor = previous.BackColor = page > 1 ? enabled : disabled; next.BackColor = last.BackColor = page < PageCount ? enabled : disabled;
        }
        private static Button MakePageButton(string text) { var button = new Button { Text = text, AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 130, 245), ForeColor = Color.White, Font = new Font("微软雅黑", 7.5F), Margin = new Padding(2, 0, 2, 0), Cursor = Cursors.Hand }; button.FlatAppearance.BorderSize = 0; return button; }
    }
}
