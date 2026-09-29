namespace My_Industrial_Monitoring_Platform
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.title = new System.Windows.Forms.Label();
            this.Userlabel = new System.Windows.Forms.Label();
            this.UsertextBox = new System.Windows.Forms.TextBox();
            this.Psdlabel = new System.Windows.Forms.Label();
            this.PsdtextBox = new System.Windows.Forms.TextBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.Registerbutton = new System.Windows.Forms.Button();
            this.Loginbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.title.Location = new System.Drawing.Point(130, 9);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(191, 39);
            this.title.TabIndex = 0;
            this.title.Text = "工业监控平台";
            // 
            // Userlabel
            // 
            this.Userlabel.AutoSize = true;
            this.Userlabel.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Userlabel.Location = new System.Drawing.Point(72, 74);
            this.Userlabel.Name = "Userlabel";
            this.Userlabel.Size = new System.Drawing.Size(59, 28);
            this.Userlabel.TabIndex = 1;
            this.Userlabel.Text = "账号:";
            // 
            // UsertextBox
            // 
            this.UsertextBox.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UsertextBox.Location = new System.Drawing.Point(137, 71);
            this.UsertextBox.Name = "UsertextBox";
            this.UsertextBox.Size = new System.Drawing.Size(193, 35);
            this.UsertextBox.TabIndex = 0;
            // 
            // Psdlabel
            // 
            this.Psdlabel.AutoSize = true;
            this.Psdlabel.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Psdlabel.Location = new System.Drawing.Point(72, 125);
            this.Psdlabel.Name = "Psdlabel";
            this.Psdlabel.Size = new System.Drawing.Size(59, 28);
            this.Psdlabel.TabIndex = 1;
            this.Psdlabel.Text = "密码:";
            // 
            // PsdtextBox
            // 
            this.PsdtextBox.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.PsdtextBox.Location = new System.Drawing.Point(137, 122);
            this.PsdtextBox.Name = "PsdtextBox";
            this.PsdtextBox.PasswordChar = '*';
            this.PsdtextBox.Size = new System.Drawing.Size(193, 35);
            this.PsdtextBox.TabIndex = 2;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton1.Location = new System.Drawing.Point(77, 174);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(93, 32);
            this.radioButton1.TabIndex = 3;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "管理员";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioButton2.Location = new System.Drawing.Point(258, 174);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(72, 32);
            this.radioButton2.TabIndex = 3;
            this.radioButton2.Text = "用户";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // Registerbutton
            // 
            this.Registerbutton.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Registerbutton.Location = new System.Drawing.Point(77, 223);
            this.Registerbutton.Name = "Registerbutton";
            this.Registerbutton.Size = new System.Drawing.Size(112, 36);
            this.Registerbutton.TabIndex = 4;
            this.Registerbutton.Text = "注册";
            this.Registerbutton.UseVisualStyleBackColor = true;
            // 
            // Loginbutton
            // 
            this.Loginbutton.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Loginbutton.Location = new System.Drawing.Point(218, 223);
            this.Loginbutton.Name = "Loginbutton";
            this.Loginbutton.Size = new System.Drawing.Size(112, 36);
            this.Loginbutton.TabIndex = 4;
            this.Loginbutton.Text = "登录";
            this.Loginbutton.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 284);
            this.Controls.Add(this.Loginbutton);
            this.Controls.Add(this.Registerbutton);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.PsdtextBox);
            this.Controls.Add(this.Psdlabel);
            this.Controls.Add(this.UsertextBox);
            this.Controls.Add(this.Userlabel);
            this.Controls.Add(this.title);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label Userlabel;
        private System.Windows.Forms.TextBox UsertextBox;
        private System.Windows.Forms.Label Psdlabel;
        private System.Windows.Forms.TextBox PsdtextBox;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.Button Registerbutton;
        private System.Windows.Forms.Button Loginbutton;
    }
}

