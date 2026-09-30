using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 用户管理页面的静态界面控件。
    /// 当前仅负责页面布局，不连接数据库或执行用户操作。
    /// </summary>
    public sealed class UserManagementPage : UserControl
    {
        private readonly Color pageBackground = Color.FromArgb(241, 245, 249);
        private readonly Color accent = Color.FromArgb(55, 130, 245);

        public UserManagementPage()
        {
            BackColor = pageBackground;
            Padding = new Padding(12);
            BuildLayout();
        }

        private void BuildLayout()
        {
            var columns = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = pageBackground,
                Margin = Padding.Empty
            };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(columns);

            var listCard = CreateCard();
            listCard.Margin = new Padding(0, 0, 6, 0);
            var addCard = CreateCard();
            addCard.Margin = new Padding(6, 0, 0, 0);
            columns.Controls.Add(listCard, 0, 0);
            columns.Controls.Add(addCard, 1, 0);

            BuildListCard(listCard);
            BuildAddCard(addCard);
        }

        private static Panel CreateCard()
        {
            return new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(16),
                Margin = Padding.Empty
            };
        }

        private void BuildListCard(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            card.Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var title = new Label
            {
                Text = "♟  用户列表",
                AutoSize = true,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 48, 66),
                Location = new Point(0, 5)
            };
            var refresh = new Button
            {
                Text = "⟳ 刷新",
                AutoSize = true,
                FlatStyle = FlatStyle.Flat,
                BackColor = accent,
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9F),
                Location = new Point(112, 1),
                Cursor = Cursors.Hand
            };
            refresh.FlatAppearance.BorderSize = 0;
            header.Controls.Add(title);
            header.Controls.Add(refresh);
            layout.Controls.Add(header, 0, 0);

            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 42 }
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 249, 252),
                ForeColor = Color.FromArgb(74, 85, 104),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                SelectionBackColor = Color.FromArgb(247, 249, 252),
                SelectionForeColor = Color.FromArgb(74, 85, 104)
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.FromArgb(52, 64, 84),
                SelectionBackColor = Color.FromArgb(232, 241, 255),
                SelectionForeColor = Color.FromArgb(35, 48, 66),
                Padding = new Padding(5)
            };
            grid.Columns.Add("username", "用户名");
            grid.Columns.Add("role", "角色");
            grid.Columns.Add("status", "状态");
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "actions", HeaderText = "操作" });
            grid.Rows.Add("admin", "Admin", "● 活跃", "禁用   删除");
            grid.Rows.Add("operator", "Operator", "● 活跃", "禁用   删除");
            grid.Rows.Add("viewer", "Viewer", "● 活跃", "禁用   删除");
            grid.Rows.Add("admin123", "Viewer", "● 活跃", "禁用   删除");
            foreach (DataGridViewRow row in grid.Rows)
                row.Cells[2].Style.ForeColor = Color.FromArgb(20, 174, 126);
            layout.Controls.Add(grid, 0, 1);
        }

        private void BuildAddCard(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 8,
                AutoSize = true,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            for (int i = 0; i < 6; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 24F : 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            card.Controls.Add(layout);

            var title = new Label
            {
                Text = "＋ 新增用户",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 48, 66)
            };
            layout.Controls.Add(title, 0, 0);

            AddField(layout, 1, "用户名");
            AddField(layout, 3, "密码", true);
            AddRoleField(layout, 5);

            var create = new Button
            {
                Text = "创建用户",
                Anchor = AnchorStyles.None,
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(26, 43, 67),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            create.FlatAppearance.BorderSize = 0;
            layout.Controls.Add(create, 0, 7);
        }

        private static void AddField(TableLayoutPanel layout, int row, string caption, bool password = false)
        {
            layout.Controls.Add(new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = Color.FromArgb(74, 85, 104),
                Font = new Font("微软雅黑", 9F)
            }, 0, row);

            var input = new TextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 250, 253),
                Font = new Font("微软雅黑", 9F),
                Margin = new Padding(0, 3, 0, 5),
                UseSystemPasswordChar = password
            };
            layout.Controls.Add(input, 0, row + 1);
        }

        private static void AddRoleField(TableLayoutPanel layout, int row)
        {
            layout.Controls.Add(new Label
            {
                Text = "角色",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                ForeColor = Color.FromArgb(74, 85, 104),
                Font = new Font("微软雅黑", 9F)
            }, 0, row);

            var role = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 9F),
                Margin = new Padding(0, 3, 0, 5)
            };
            role.Items.AddRange(new object[] { "Viewer", "Operator", "Admin" });
            role.SelectedIndex = 0;
            layout.Controls.Add(role, 0, row + 1);
        }
    }
}
