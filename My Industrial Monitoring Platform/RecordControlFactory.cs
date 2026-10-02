using System.Drawing;
using System.Windows.Forms;
namespace My_Industrial_Monitoring_Platform
{
    internal static class RecordControlFactory
    {
        internal static void Build(UserControl owner, string titleText, bool history)
        {
            string prefix = history ? "history" : "alarm";
            owner.BackColor = Color.White; owner.Padding = new Padding(10);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.White };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            var title = new Label { Text = titleText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("微软雅黑",10F,FontStyle.Bold), ForeColor = Color.FromArgb(44,62,80) };
            var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = true, AutoScroll = true, Padding = new Padding(0,4,0,2) };
            if (history)
            {
                filters.Controls.Add(FilterLabel("开始日期")); var start = DatePicker(); start.Name = prefix + "StartDate"; filters.Controls.Add(start);
                filters.Controls.Add(FilterLabel("结束日期")); var end = DatePicker(); end.Name = prefix + "EndDate"; filters.Controls.Add(end);
                filters.Controls.Add(FilterLabel("设备")); var device = Combo("全部"); device.Name = prefix + "Device"; filters.Controls.Add(device);
                var query = Button("查询", Color.FromArgb(55,130,245)); query.Name = prefix + "Query"; filters.Controls.Add(query);
                var refresh = Button("刷新", Color.FromArgb(16,185,129)); refresh.Name = prefix + "Refresh"; filters.Controls.Add(refresh);
            }
            else
            {
                filters.Controls.Add(FilterLabel("报警类型"));
                var types = Combo("全部", "温度超限", "压力超限", "温度+压力超限"); types.Name = prefix + "Type"; types.Width = 145; filters.Controls.Add(types);
                var refresh = Button("刷新", Color.FromArgb(55,130,245)); refresh.Name = prefix + "Refresh"; filters.Controls.Add(refresh);
            }
            var headers = history ? new[] { "编号", "设备", "采样时间", "温度", "压力" } : new[] { "编号", "设备", "报警类型", "报警值", "开始时间", "结束时间", "状态" };
            var grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None, ColumnHeadersHeight = 38, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, EnableHeadersVisualStyles = false, GridColor = Color.FromArgb(210,214,220), ReadOnly = true, RowHeadersVisible = false, RowTemplate = { Height = 38 }, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            grid.DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(44,62,80), SelectionBackColor = Color.FromArgb(235,242,250), SelectionForeColor = Color.FromArgb(44,62,80), Font = new Font("微软雅黑",8F), Padding = new Padding(3) };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.White, ForeColor = Color.FromArgb(44,62,80), Font = new Font("微软雅黑",8F,FontStyle.Bold), SelectionBackColor = Color.White, SelectionForeColor = Color.FromArgb(44,62,80), Padding = new Padding(3) };
            grid.AutoGenerateColumns = false;
            foreach (var header in headers)
                grid.Columns.Add(new DataGridViewTextBoxColumn { Name = header, HeaderText = header, DataPropertyName = header, SortMode = DataGridViewColumnSortMode.NotSortable });
            grid.Name = prefix + "Grid";
            var pager = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true, Padding = new Padding(0,6,0,0), BackColor = Color.FromArgb(241,245,249) };
            var first = Button("首页", Color.FromArgb(55,130,245)); first.Name = prefix + "First"; pager.Controls.Add(first);
            var previous = Button("◀", Color.FromArgb(55,130,245)); previous.Name = prefix + "Previous"; pager.Controls.Add(previous);
            var info = new Label { Text = "第 1/1 页（共 0 条）", AutoSize = true, Margin = new Padding(3,8,3,0), Font = new Font("微软雅黑",8F), ForeColor = Color.FromArgb(100,116,139), Name = prefix + "PageInfo" }; pager.Controls.Add(info);
            var next = Button("▶", Color.FromArgb(55,130,245)); next.Name = prefix + "Next"; pager.Controls.Add(next);
            var last = Button("末页", Color.FromArgb(55,130,245)); last.Name = prefix + "Last"; pager.Controls.Add(last);
            pager.Controls.Add(new Label { Text = "跳转", AutoSize = true, Margin = new Padding(4,8,1,0), Font = new Font("微软雅黑",8F), ForeColor = Color.FromArgb(71,85,105) });
            var jump = new NumericUpDown { Minimum = 1, Maximum = 1, Value = 1, Width = 42, Margin = new Padding(1,3,2,0), Font = new Font("微软雅黑",8F), Name = prefix + "Jump" }; pager.Controls.Add(jump);
            var go = Button("Go", Color.FromArgb(16,185,129)); go.Name = prefix + "Go"; pager.Controls.Add(go);
            layout.Controls.Add(title,0,0); layout.Controls.Add(filters,0,1); layout.Controls.Add(grid,0,2); layout.Controls.Add(pager,0,3); owner.Controls.Add(layout);
        }
        private static Label FilterLabel(string text) { return new Label { Text=text+"：", AutoSize=true, Margin=new Padding(2,8,2,0), Font=new Font("微软雅黑",8.5F), ForeColor=Color.FromArgb(71,85,105) }; }
        private static DateTimePicker DatePicker() { return new DateTimePicker { Format=DateTimePickerFormat.Custom, CustomFormat="yyyy/M/d", Width=108, Margin=new Padding(0,2,6,3), Font=new Font("微软雅黑",8F) }; }
        private static ComboBox Combo(params string[] values) { var c=new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList, Width=95, Margin=new Padding(0,2,6,3), Font=new Font("微软雅黑",8F) }; c.Items.AddRange(values); if(c.Items.Count>0)c.SelectedIndex=0; return c; }
        private static Button Button(string text, Color color) { var b=new Button { Text=text, Width=56, Height=27, Margin=new Padding(2,1,3,3), FlatStyle=FlatStyle.Flat, BackColor=color, ForeColor=Color.White, Font=new Font("微软雅黑",8F), Cursor=Cursors.Hand }; b.FlatAppearance.BorderSize=0; return b; }
    }
}
