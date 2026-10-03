using System;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>独立的新增用户表单控件。</summary>
    public sealed class AddUserControl : UserControl
    {
        public event EventHandler UserCreated;
        private readonly TextBox usernameInput;
        private readonly TextBox passwordInput;
        private readonly ComboBox roleInput;
        private readonly Label feedbackLabel;

        public AddUserControl()
        {
            BackColor = Color.White; Padding = new Padding(16);
            var layout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 1, RowCount = 9, AutoSize = true, BackColor = Color.White, Margin = Padding.Empty };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            for (int i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i % 2 == 0 ? 24F : 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "＋ 新增用户", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("微软雅黑", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(35, 48, 66) }, 0, 0);
            usernameInput = AddField(layout, 1, "用户名");
            passwordInput = AddField(layout, 3, "密码", true);
            roleInput = AddRoleField(layout, 5);
            var create = new Button { Text = "创建用户", Anchor = AnchorStyles.None, Size = new Size(120, 34), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(26, 43, 67), ForeColor = Color.White, Font = new Font("微软雅黑", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            create.FlatAppearance.BorderSize = 0; create.Click += CreateUser_Click; layout.Controls.Add(create, 0, 7);
            feedbackLabel = new Label { Text = string.Empty, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("微软雅黑", 8.5F) };
            layout.Controls.Add(feedbackLabel, 0, 8);
        }

        private static TextBox AddField(TableLayoutPanel layout, int row, string caption, bool password = false)
        {
            layout.Controls.Add(new Label { Text = caption, Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, ForeColor = Color.FromArgb(74, 85, 104), Font = new Font("微软雅黑", 9F) }, 0, row);
            var input = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(248, 250, 253), Font = new Font("微软雅黑", 9F), Margin = new Padding(0, 3, 0, 5), UseSystemPasswordChar = password };
            layout.Controls.Add(input, 0, row + 1); return input;
        }

        private static ComboBox AddRoleField(TableLayoutPanel layout, int row)
        {
            layout.Controls.Add(new Label { Text = "角色", Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, ForeColor = Color.FromArgb(74, 85, 104), Font = new Font("微软雅黑", 9F) }, 0, row);
            var role = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("微软雅黑", 9F), Margin = new Padding(0, 3, 0, 5) };
            role.Items.AddRange(new object[] { "用户", "管理员" }); role.SelectedIndex = 0; layout.Controls.Add(role, 0, row + 1); return role;
        }

        private void CreateUser_Click(object sender, EventArgs e)
        {
            string username = usernameInput.Text.Trim(), password = passwordInput.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) { ShowFeedback("请输入用户名和密码。", Color.FromArgb(210, 55, 65)); return; }
            try
            {
                if (!UserDB.CreateUser(username, password, roleInput.SelectedItem.ToString())) { ShowFeedback("该用户名已存在，请更换用户名。", Color.FromArgb(210, 55, 65)); return; }
                usernameInput.Clear(); passwordInput.Clear(); roleInput.SelectedIndex = 0;
                ShowFeedback("用户“" + username + "”创建成功。", Color.FromArgb(20, 145, 105));
                EventHandler handler = UserCreated; if (handler != null) handler(this, EventArgs.Empty);
            }
            catch (Exception ex) { ShowFeedback("创建失败：" + ex.Message, Color.FromArgb(210, 55, 65)); }
        }

        private void ShowFeedback(string message, Color color) { feedbackLabel.Text = message; feedbackLabel.ForeColor = color; }
    }
}
