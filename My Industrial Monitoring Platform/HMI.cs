using System;
using System.Drawing;
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
            networkCommunicationPage.ReadingProduced += homeOverviewPage.AcceptCommunicationReading;
            networkCommunicationPage.CommunicationStopped += homeOverviewPage.CommunicationStopped;
            button1.Click += (sender, e) => ShowPage(homeOverviewPage, button1, "首页总览");
            button5.Click += (sender, e) => ShowPage(networkCommunicationPage, button5, "网络通讯");
            button6.Click += async (sender, e) =>
            {
                ShowPage(dataRecordPage, button6, "数据记录");
                await dataRecordPage.LoadRecordsAsync();
            };
            button7.Click += (sender, e) => ShowPage(userManagementPage, button7, "用户管理");
            ShowPage(homeOverviewPage, button1, "首页总览");
            StartClock();
        }

        private void ShowPage(Control page, Button selectedButton, string pageName)
        {
            // 同步切换内容区、页面标题和左侧导航选中状态。
            homeOverviewPage.Visible = page == homeOverviewPage;
            networkCommunicationPage.Visible = page == networkCommunicationPage;
            userManagementPage.Visible = page == userManagementPage;
            dataRecordPage.Visible = page == dataRecordPage;
            page.BringToFront();
            namelabel_UI.Text = pageName;

            button1.BackColor = page == homeOverviewPage
                ? Color.FromArgb(32, 59, 91)
                : Color.FromArgb(16, 26, 46);
            button5.BackColor = page == networkCommunicationPage
                ? Color.FromArgb(32, 59, 91)
                : Color.FromArgb(16, 26, 46);
            button7.BackColor = page == userManagementPage
                ? Color.FromArgb(32, 59, 91)
                : Color.FromArgb(16, 26, 46);
            button6.BackColor = page == dataRecordPage
                ? Color.FromArgb(32, 59, 91)
                : Color.FromArgb(16, 26, 46);

            selectedNavIndicator.Parent = selectedButton;
            selectedNavIndicator.Dock = DockStyle.Left;
            selectedNavIndicator.BringToFront();
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
