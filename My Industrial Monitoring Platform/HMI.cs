using System;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    public partial class HMI : Form
    {
        private Timer clockTime;
        /// <summary>
        /// 用于在只打开HMI时使用
        /// </summary>
        public HMI() : this("admin")
        {
        }
        /// <summary>
        /// 打开HMI时传入登录的用户名
        /// </summary>
        /// <param name="username"></param>
        public HMI(string username)
        {
            InitializeComponent();
            idlable_UI.Text = username;
            StartClock();
        }
        /// <summary>
        /// 顶部导航栏时间显示
        /// </summary>
        private void StartClock()
        {
            clockTime = new Timer();
            clockTime.Tick += (sender, e) =>
            {
                timelabel_UI.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            clockTime.Start();
        }

    }
}
