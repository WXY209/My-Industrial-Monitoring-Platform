using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>独立的用户列表控件，负责刷新、显示和删除用户。</summary>
    public sealed class UserListControl : UserControl
    {
        private readonly DataGridView userGrid;
        private readonly Label feedbackLabel;

        public UserListControl()
        {
            BackColor = Color.White;
            Padding = new Padding(16);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            header.Controls.Add(new Label { Text = "♟  用户列表", AutoSize = true, Font = new Font("微软雅黑", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(35, 48, 66), Location = new Point(0, 5) });
            var refresh = new Button { Text = "⟳ 刷新", AutoSize = true, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(55, 130, 245), ForeColor = Color.White, Font = new Font("微软雅黑", 9F), Location = new Point(112, 1), Cursor = Cursors.Hand };
            refresh.FlatAppearance.BorderSize = 0;
            refresh.Click += (s, e) => RefreshUsers();
            header.Controls.Add(refresh);
            layout.Controls.Add(header, 0, 0);

            userGrid = new DataGridView
            {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                ReadOnly = true, RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                EnableHeadersVisualStyles = false, AutoGenerateColumns = false, ColumnHeadersHeight = 38,
                RowTemplate = { Height = 42 }
            };
            userGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(247, 249, 252), ForeColor = Color.FromArgb(74, 85, 104), Font = new Font("微软雅黑", 9F, FontStyle.Bold), SelectionBackColor = Color.FromArgb(247, 249, 252), SelectionForeColor = Color.FromArgb(74, 85, 104) };
            userGrid.DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("微软雅黑", 9F), ForeColor = Color.FromArgb(52, 64, 84), SelectionBackColor = Color.FromArgb(232, 241, 255), SelectionForeColor = Color.FromArgb(35, 48, 66), Padding = new Padding(5) };
            userGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", DataPropertyName = "Username", HeaderText = "用户名", FillWeight = 35 });
            userGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", DataPropertyName = "Role", HeaderText = "角色", FillWeight = 25 });
            userGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "状态", FillWeight = 25 });
            userGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Delete", HeaderText = "操作", Text = "删除", UseColumnTextForButtonValue = true, FlatStyle = FlatStyle.Flat, FillWeight = 20, DefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(235, 75, 75), ForeColor = Color.White, SelectionBackColor = Color.FromArgb(245, 108, 108), SelectionForeColor = Color.White, Alignment = DataGridViewContentAlignment.MiddleCenter } });
            userGrid.CellFormatting += (s, e) => { if (userGrid.Columns[e.ColumnIndex].Name == "Status") e.CellStyle.ForeColor = Color.FromArgb(20, 174, 126); };
            userGrid.CellContentClick += UserGrid_CellContentClick;
            layout.Controls.Add(userGrid, 0, 1);

            feedbackLabel = new Label { AutoSize = true, Visible = false, Font = new Font("微软雅黑", 8.5F) };
            Controls.Add(feedbackLabel);
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                userGrid.Rows.Add("admin", "管理员", "正常", "删除");
                userGrid.Rows.Add("operator", "用户", "正常", "删除");
            }
            else RefreshUsers();
        }

        public void RefreshUsers()
        {
            try { userGrid.DataSource = UserDB.GetUsers(); }
            catch (Exception ex) { ShowFeedback("读取用户失败：" + ex.Message, Color.FromArgb(210, 55, 65)); }
        }

        private void UserGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || userGrid.Columns[e.ColumnIndex].Name != "Delete") return;
            string username = Convert.ToString(userGrid.Rows[e.RowIndex].Cells["Username"].Value);
            if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                ShowFeedback("内置 admin 管理员账号不能删除。", Color.FromArgb(210, 55, 65)); return;
            }
            if (MessageBox.Show("确定删除用户“" + username + "”吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            try
            {
                if (UserDB.DeleteUser(username)) { RefreshUsers(); ShowFeedback("用户“" + username + "”已删除。", Color.FromArgb(20, 145, 105)); }
                else ShowFeedback("删除失败，用户可能已经不存在。", Color.FromArgb(210, 55, 65));
            }
            catch (Exception ex) { ShowFeedback("删除失败：" + ex.Message, Color.FromArgb(210, 55, 65)); }
        }

        private void ShowFeedback(string message, Color color)
        {
            feedbackLabel.Text = message; feedbackLabel.ForeColor = color; feedbackLabel.Visible = true;
        }
    }
}
