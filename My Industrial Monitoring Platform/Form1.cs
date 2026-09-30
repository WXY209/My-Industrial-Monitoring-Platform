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
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 登录功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loginbutton_Click(object sender, EventArgs e)
        {
            string username =UsertextBox.Text.Trim();
            string password =PsdtextBox.Text;
            if (UserDB.DataLogin(username,password))
            {
                this.Hide();
                using (var hmi =new HMI(username))
                {
                    hmi.ShowDialog();
                }
                this.Close();
            }
            else
            {
                MessageBox.Show("账号或密码错误");
            }
        }
        /// <summary>
        /// 注册功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Registerbutton_Click(object sender, EventArgs e)
        {
            string username=UsertextBox.Text.Trim();
            string password=PsdtextBox.Text;
            if (string.IsNullOrWhiteSpace(username)||string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("请输入账号密码");
                return;
            }
            bool register=UserDB.RegisterUser(username,password);
            if (register)
            {
                MessageBox.Show("注册成功");
            }
            else
            {
                MessageBox.Show("请更换账号");
            }
        }
    }
}
