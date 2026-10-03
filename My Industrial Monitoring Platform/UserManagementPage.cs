using System;
using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>用户管理页面容器：左侧列表，右侧新增表单。</summary>
    public sealed class UserManagementPage : UserControl
    {
        private readonly UserListControl userList;
        private readonly AddUserControl addUser;

        public UserManagementPage() : this(new UserManagementService())
        {
        }

        public UserManagementPage(IUserManagementService userService)
        {
            if (userService == null) throw new ArgumentNullException("userService");
            BackColor = Color.FromArgb(241, 245, 249);
            Padding = new Padding(12);
            var columns = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = BackColor, Margin = Padding.Empty };
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68F));
            columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            columns.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(columns);

            userList = new UserListControl(userService) { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
            addUser = new AddUserControl(userService) { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 0) };
            addUser.UserCreated += async (sender, e) => await userList.RefreshUsersAsync();
            columns.Controls.Add(userList, 0, 0);
            columns.Controls.Add(addUser, 1, 0);
        }
    }
}
