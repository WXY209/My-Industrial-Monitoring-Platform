using System.Windows.Forms;

namespace My_Industrial_Monitoring_Platform
{
    /// <summary>通信收发日志列表控件，目前仅展示空列表。</summary>
    public sealed class CommunicationLogControl : UserControl
    {
        public CommunicationLogControl()
        {
            Panel body;
            Controls.Add(CommunicationUi.CreateCard("通信日志", out body));
            body.Controls.Add(CommunicationUi.CreateGrid("时间", "方向", "操作/报文", "结果", "说明"));
        }
    }
}
