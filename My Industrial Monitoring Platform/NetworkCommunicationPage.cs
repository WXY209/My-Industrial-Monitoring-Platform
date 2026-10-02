using System.Drawing;
using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>网络通讯页面容器，组合配置、状态、映射和日志四个子控件。</summary>
    public sealed class NetworkCommunicationPage : UserControl
    {
        public NetworkCommunicationPage()
        {
            BackColor = Color.FromArgb(241, 245, 249);
            Padding = new Padding(10);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = BackColor,
                Padding = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 47F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 53F));

            layout.Controls.Add(new CommunicationConfigControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 6) }, 0, 0);
            layout.Controls.Add(new CommunicationStatusControl { Dock = DockStyle.Fill, Margin = new Padding(6, 0, 0, 6) }, 1, 0);
            layout.Controls.Add(new RegisterMappingControl { Dock = DockStyle.Fill, Margin = new Padding(0, 6, 6, 0) }, 0, 1);
            layout.Controls.Add(new CommunicationLogControl { Dock = DockStyle.Fill, Margin = new Padding(6, 6, 0, 0) }, 1, 1);
            Controls.Add(layout);
        }
    }
}
