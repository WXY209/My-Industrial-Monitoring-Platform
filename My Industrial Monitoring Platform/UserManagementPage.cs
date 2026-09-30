using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>
    /// 用户管理页面：显示数据库用户、创建用户和删除用户。
    /// </summary>
    public sealed class UserManagementPage : UserControl
    {
        private readonly Color pageBackground =
            Color.FromArgb(241, 245, 249);

        private readonly Color accent =
            Color.FromArgb(55, 130, 245);

        private DataGridView userGrid;
        private TextBox usernameInput;
        private TextBox passwordInput;
        private ComboBox roleInput;
        private Label feedbackLabel;

        public UserManagementPage()
        {
            BackColor = pageBackground;
            Padding = new Padding(12);

            BuildLayout();

            // Visual Studio 设计器显示预览数据；
            // 程序运行时从 SQLite 加载真实用户。
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                ShowDesignPreview();
            }
            else
            {
                LoadUsers();
            }
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

            columns.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 68F));
            columns.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 32F));
            columns.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

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

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100F));

            card.Controls.Add(layout);

            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

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
            refresh.Click += (sender, e) => LoadUsers();

            header.Controls.Add(title);
            header.Controls.Add(refresh);
            layout.Controls.Add(header, 0, 0);

            userGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                AutoGenerateColumns = false,
                ColumnHeadersHeight = 38,
                RowTemplate = { Height = 42 }
            };

            userGrid.ColumnHeadersDefaultCellStyle =
                new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(247, 249, 252),
                    ForeColor = Color.FromArgb(74, 85, 104),
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                    SelectionBackColor = Color.FromArgb(247, 249, 252),
                    SelectionForeColor = Color.FromArgb(74, 85, 104)
                };

            userGrid.DefaultCellStyle =
                new DataGridViewCellStyle
                {
                    Font = new Font("微软雅黑", 9F),
                    ForeColor = Color.FromArgb(52, 64, 84),
                    SelectionBackColor = Color.FromArgb(232, 241, 255),
                    SelectionForeColor = Color.FromArgb(35, 48, 66),
                    Padding = new Padding(5)
                };

            userGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Username",
                DataPropertyName = "Username",
                HeaderText = "用户名",
                FillWeight = 35
            });

            userGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Role",
                DataPropertyName = "Role",
                HeaderText = "角色",
                FillWeight = 25
            });

            userGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "状态",
                FillWeight = 25
            });

            userGrid.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "操作",
                Text = "删除",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                FillWeight = 20,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(235, 75, 75),
                    ForeColor = Color.White,
                    SelectionBackColor = Color.FromArgb(245, 108, 108),
                    SelectionForeColor = Color.White,
                    Alignment =
                        DataGridViewContentAlignment.MiddleCenter
                }
            });

            userGrid.CellFormatting += (sender, e) =>
            {
                if (userGrid.Columns[e.ColumnIndex].Name == "Status")
                {
                    e.CellStyle.ForeColor =
                        Color.FromArgb(20, 174, 126);
                }
            };

            userGrid.CellContentClick += UserGrid_CellContentClick;

            layout.Controls.Add(userGrid, 0, 1);
        }

        private void BuildAddCard(Panel card)
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 9,
                AutoSize = true,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 42F));

            for (int i = 0; i < 6; i++)
            {
                layout.RowStyles.Add(
                    new RowStyle(
                        SizeType.Absolute,
                        i % 2 == 0 ? 24F : 40F));
            }

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 52F));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 34F));

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

            usernameInput = AddField(layout, 1, "用户名");
            passwordInput = AddField(layout, 3, "密码", true);
            roleInput = AddRoleField(layout, 5);

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
            create.Click += CreateUser_Click;
            layout.Controls.Add(create, 0, 7);

            feedbackLabel = new Label
            {
                Text = string.Empty,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 8.5F)
            };

            layout.Controls.Add(feedbackLabel, 0, 8);
        }

        private static TextBox AddField(
            TableLayoutPanel layout,
            int row,
            string caption,
            bool password = false)
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
            return input;
        }

        private static ComboBox AddRoleField(
            TableLayoutPanel layout,
            int row)
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

            role.Items.AddRange(new object[]
            {
                "用户",
                "管理员"
            });
            role.SelectedIndex = 0;

            layout.Controls.Add(role, 0, row + 1);
            return role;
        }

        private void LoadUsers()
        {
            try
            {
                userGrid.DataSource = UserDB.GetUsers();
            }
            catch (Exception ex)
            {
                ShowFeedback(
                    "读取用户失败：" + ex.Message,
                    Color.FromArgb(210, 55, 65));
            }
        }

        private void CreateUser_Click(object sender, EventArgs e)
        {
            string username = usernameInput.Text.Trim();
            string password = passwordInput.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                ShowFeedback(
                    "请输入用户名和密码。",
                    Color.FromArgb(210, 55, 65));
                return;
            }

            try
            {
                string role = roleInput.SelectedItem.ToString();

                bool created = UserDB.CreateUser(
                    username,
                    password,
                    role);

                if (!created)
                {
                    ShowFeedback(
                        "该用户名已存在，请更换用户名。",
                        Color.FromArgb(210, 55, 65));
                    return;
                }

                usernameInput.Clear();
                passwordInput.Clear();
                roleInput.SelectedIndex = 0;

                LoadUsers();

                ShowFeedback(
                    "用户“" + username + "”创建成功。",
                    Color.FromArgb(20, 145, 105));
            }
            catch (Exception ex)
            {
                ShowFeedback(
                    "创建失败：" + ex.Message,
                    Color.FromArgb(210, 55, 65));
            }
        }

        private void UserGrid_CellContentClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                userGrid.Columns[e.ColumnIndex].Name != "Delete")
            {
                return;
            }

            string username = Convert.ToString(
                userGrid.Rows[e.RowIndex].Cells["Username"].Value);

            if (string.Equals(
                username,
                "admin",
                StringComparison.OrdinalIgnoreCase))
            {
                ShowFeedback(
                    "内置 admin 管理员账号不能删除。",
                    Color.FromArgb(210, 55, 65));
                return;
            }

            DialogResult result = MessageBox.Show(
                "确定删除用户“" + username + "”吗？",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (UserDB.DeleteUser(username))
                {
                    LoadUsers();

                    ShowFeedback(
                        "用户“" + username + "”已删除。",
                        Color.FromArgb(20, 145, 105));
                }
                else
                {
                    ShowFeedback(
                        "删除失败，用户可能已经不存在。",
                        Color.FromArgb(210, 55, 65));
                }
            }
            catch (Exception ex)
            {
                ShowFeedback(
                    "删除失败：" + ex.Message,
                    Color.FromArgb(210, 55, 65));
            }
        }

        private void ShowFeedback(string message, Color color)
        {
            feedbackLabel.Text = message;
            feedbackLabel.ForeColor = color;
        }

        private void ShowDesignPreview()
        {
            userGrid.Rows.Add(
                "admin",
                "管理员",
                "正常",
                "删除");

            userGrid.Rows.Add(
                "operator",
                "用户",
                "正常",
                "删除");
        }
    }
}