using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    public partial class Form1 : Form
    {
        private readonly IUserManagementService userService = new UserManagementService();
        private bool isProcessingUserAction;

        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 登录功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Loginbutton_Click(object sender, EventArgs e)
        {
            if (isProcessingUserAction)
                return;

            string username = UsertextBox.Text.Trim();
            string password = PsdtextBox.Text;
            SetUserActionBusy(true);
            try
            {
                if (await userService.ValidateLoginAsync(username, password))
                {
                    Hide();
                    using (var hmi = new HMI(username))
                    {
                        hmi.ShowDialog();
                    }
                    Close();
                }
                else
                    MessageBox.Show("账号或密码错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录失败：" + ex.Message, "登录错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetUserActionBusy(false); }
        }
        /// <summary>
        /// 注册功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Registerbutton_Click(object sender, EventArgs e)
        {
            if (isProcessingUserAction)
                return;

            string username = UsertextBox.Text.Trim();
            string password = PsdtextBox.Text;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("请输入账号密码");
                return;
            }

            SetUserActionBusy(true);
            try
            {
                bool registered = await userService.RegisterUserAsync(username, password);
                if (registered)
                    MessageBox.Show("注册成功");
                else
                    MessageBox.Show("请更换账号");
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册失败：" + ex.Message, "注册错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { SetUserActionBusy(false); }
        }

        private void SetUserActionBusy(bool busy)
        {
            isProcessingUserAction = busy;
            Loginbutton.Enabled = Registerbutton.Enabled = !busy;
            UseWaitCursor = busy;
        }
    }
}
