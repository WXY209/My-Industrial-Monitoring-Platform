namespace My_Industrial_Monitoring_Platform
{
    partial class HMI
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.toppanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.breadcrumbPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.namelabel_UI = new System.Windows.Forms.Label();
            this.statusPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.timelabel_UI = new System.Windows.Forms.Label();
            this.idtypelabel_UI = new System.Windows.Forms.Label();
            this.idlable_UI = new System.Windows.Forms.Label();
            this.checklabel_UI = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button7 = new System.Windows.Forms.Button();
            this.selectedNavIndicator = new System.Windows.Forms.Panel();
            this.button6 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.userManagementPage = new My_Industrial_Monitoring_Platform.UserManagementPage();
            this.toppanel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.breadcrumbPanel.SuspendLayout();
            this.statusPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.rootLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // toppanel
            // 
            this.toppanel.BackColor = System.Drawing.Color.White;
            this.rootLayout.SetColumnSpan(this.toppanel, 2);
            this.toppanel.Controls.Add(this.tableLayoutPanel1);
            this.toppanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toppanel.Location = new System.Drawing.Point(3, 3);
            this.toppanel.Name = "toppanel";
            this.toppanel.Size = new System.Drawing.Size(769, 34);
            this.toppanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.Controls.Add(this.breadcrumbPanel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.statusPanel, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(769, 34);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // breadcrumbPanel
            // 
            this.breadcrumbPanel.BackColor = System.Drawing.Color.White;
            this.breadcrumbPanel.Controls.Add(this.label1);
            this.breadcrumbPanel.Controls.Add(this.namelabel_UI);
            this.breadcrumbPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.breadcrumbPanel.Location = new System.Drawing.Point(0, 0);
            this.breadcrumbPanel.Margin = new System.Windows.Forms.Padding(0);
            this.breadcrumbPanel.Name = "breadcrumbPanel";
            this.breadcrumbPanel.Padding = new System.Windows.Forms.Padding(8, 12, 4, 0);
            this.breadcrumbPanel.Size = new System.Drawing.Size(307, 34);
            this.breadcrumbPanel.TabIndex = 0;
            this.breadcrumbPanel.WrapContents = false;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "首页 /";
            // 
            // namelabel_UI
            // 
            this.namelabel_UI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.namelabel_UI.AutoSize = true;
            this.namelabel_UI.Location = new System.Drawing.Point(61, 12);
            this.namelabel_UI.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.namelabel_UI.Name = "namelabel_UI";
            this.namelabel_UI.Size = new System.Drawing.Size(53, 12);
            this.namelabel_UI.TabIndex = 1;
            this.namelabel_UI.Text = "用户管理";
            // 
            // statusPanel
            // 
            this.statusPanel.BackColor = System.Drawing.Color.White;
            this.statusPanel.Controls.Add(this.timelabel_UI);
            this.statusPanel.Controls.Add(this.idtypelabel_UI);
            this.statusPanel.Controls.Add(this.idlable_UI);
            this.statusPanel.Controls.Add(this.checklabel_UI);
            this.statusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.statusPanel.Location = new System.Drawing.Point(307, 0);
            this.statusPanel.Margin = new System.Windows.Forms.Padding(0);
            this.statusPanel.Name = "statusPanel";
            this.statusPanel.Padding = new System.Windows.Forms.Padding(4, 12, 8, 0);
            this.statusPanel.Size = new System.Drawing.Size(462, 34);
            this.statusPanel.TabIndex = 1;
            this.statusPanel.WrapContents = false;
            // 
            // timelabel_UI
            // 
            this.timelabel_UI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.timelabel_UI.AutoSize = true;
            this.timelabel_UI.Location = new System.Drawing.Point(417, 13);
            this.timelabel_UI.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.timelabel_UI.Name = "timelabel_UI";
            this.timelabel_UI.Size = new System.Drawing.Size(29, 12);
            this.timelabel_UI.TabIndex = 0;
            this.timelabel_UI.Text = "时间";
            // 
            // idtypelabel_UI
            // 
            this.idtypelabel_UI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.idtypelabel_UI.AutoSize = true;
            this.idtypelabel_UI.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.idtypelabel_UI.Location = new System.Drawing.Point(366, 12);
            this.idtypelabel_UI.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.idtypelabel_UI.Name = "idtypelabel_UI";
            this.idtypelabel_UI.Size = new System.Drawing.Size(43, 14);
            this.idtypelabel_UI.TabIndex = 1;
            this.idtypelabel_UI.Text = "管理员";
            // 
            // idlable_UI
            // 
            this.idlable_UI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.idlable_UI.AutoSize = true;
            this.idlable_UI.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.idlable_UI.Location = new System.Drawing.Point(321, 12);
            this.idlable_UI.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.idlable_UI.Name = "idlable_UI";
            this.idlable_UI.Size = new System.Drawing.Size(37, 14);
            this.idlable_UI.TabIndex = 2;
            this.idlable_UI.Text = "admin";
            // 
            // checklabel_UI
            // 
            this.checklabel_UI.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.checklabel_UI.AutoSize = true;
            this.checklabel_UI.Location = new System.Drawing.Point(242, 13);
            this.checklabel_UI.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.checklabel_UI.Name = "checklabel_UI";
            this.checklabel_UI.Size = new System.Drawing.Size(71, 12);
            this.checklabel_UI.TabIndex = 3;
            this.checklabel_UI.Text = "● 系统正常";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(26)))), ((int)(((byte)(46)))));
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(3, 43);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(131, 523);
            this.panel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.button7, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.selectedNavIndicator, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.button6, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.button1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button5, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(131, 523);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(59)))), ((int)(((byte)(91)))));
            this.button7.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button7.FlatAppearance.BorderSize = 0;
            this.button7.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(59)))), ((int)(((byte)(91)))));
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button7.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button7.Location = new System.Drawing.Point(0, 156);
            this.button7.Margin = new System.Windows.Forms.Padding(0);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(131, 52);
            this.button7.TabIndex = 6;
            this.button7.Text = "用户管理";
            this.button7.UseVisualStyleBackColor = false;
            // 
            // selectedNavIndicator
            // 
            this.selectedNavIndicator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(130)))), ((int)(((byte)(245)))));
            this.selectedNavIndicator.Dock = System.Windows.Forms.DockStyle.Left;
            this.selectedNavIndicator.Enabled = false;
            this.selectedNavIndicator.Location = new System.Drawing.Point(0, 208);
            this.selectedNavIndicator.Margin = new System.Windows.Forms.Padding(0);
            this.selectedNavIndicator.Name = "selectedNavIndicator";
            this.selectedNavIndicator.Size = new System.Drawing.Size(3, 315);
            this.selectedNavIndicator.TabIndex = 7;
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(26)))), ((int)(((byte)(46)))));
            this.button6.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button6.FlatAppearance.BorderSize = 0;
            this.button6.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(45)))), ((int)(((byte)(69)))));
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button6.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button6.Location = new System.Drawing.Point(0, 104);
            this.button6.Margin = new System.Windows.Forms.Padding(0);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(131, 52);
            this.button6.TabIndex = 5;
            this.button6.Text = "系统设置";
            this.button6.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(26)))), ((int)(((byte)(46)))));
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(45)))), ((int)(((byte)(69)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(131, 52);
            this.button1.TabIndex = 0;
            this.button1.Text = "首页总览";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(26)))), ((int)(((byte)(46)))));
            this.button5.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(45)))), ((int)(((byte)(69)))));
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button5.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button5.Location = new System.Drawing.Point(0, 52);
            this.button5.Margin = new System.Windows.Forms.Padding(0);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(131, 52);
            this.button5.TabIndex = 4;
            this.button5.Text = "网络通讯";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 2;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 137F));
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.toppanel, 0, 0);
            this.rootLayout.Controls.Add(this.panel1, 0, 1);
            this.rootLayout.Controls.Add(this.userManagementPage, 1, 1);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.RowCount = 2;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(775, 569);
            this.rootLayout.TabIndex = 2;
            // 
            // userManagementPage
            // 
            this.userManagementPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.userManagementPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userManagementPage.Location = new System.Drawing.Point(140, 43);
            this.userManagementPage.Name = "userManagementPage";
            this.userManagementPage.Padding = new System.Windows.Forms.Padding(12);
            this.userManagementPage.Size = new System.Drawing.Size(632, 523);
            this.userManagementPage.TabIndex = 0;
            // 
            // HMI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(775, 569);
            this.Controls.Add(this.rootLayout);
            this.Name = "HMI";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "工业监控平台界面";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toppanel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.breadcrumbPanel.ResumeLayout(false);
            this.breadcrumbPanel.PerformLayout();
            this.statusPanel.ResumeLayout(false);
            this.statusPanel.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.rootLayout.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel toppanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel breadcrumbPanel;
        private System.Windows.Forms.FlowLayoutPanel statusPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label namelabel_UI;
        private System.Windows.Forms.Label checklabel_UI;
        private System.Windows.Forms.Label idlable_UI;
        private System.Windows.Forms.Label idtypelabel_UI;
        private System.Windows.Forms.Label timelabel_UI;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Panel selectedNavIndicator;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private UserManagementPage userManagementPage;
    }
}
