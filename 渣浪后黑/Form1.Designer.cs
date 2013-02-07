namespace 渣浪后黑
{
    partial class fuckSina
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.inputFileBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outputFileBox = new System.Windows.Forms.TextBox();
            this.startButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.rateBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入文件";
            // 
            // inputFileBox
            // 
            this.inputFileBox.AllowDrop = true;
            this.inputFileBox.Location = new System.Drawing.Point(79, 17);
            this.inputFileBox.Name = "inputFileBox";
            this.inputFileBox.Size = new System.Drawing.Size(464, 21);
            this.inputFileBox.TabIndex = 1;
            this.inputFileBox.TextChanged += new System.EventHandler(this.inputFileBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "输出文件";
            // 
            // outputFileBox
            // 
            this.outputFileBox.Location = new System.Drawing.Point(79, 47);
            this.outputFileBox.Name = "outputFileBox";
            this.outputFileBox.Size = new System.Drawing.Size(464, 21);
            this.outputFileBox.TabIndex = 3;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(553, 76);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(167, 38);
            this.startButton.TabIndex = 4;
            this.startButton.Text = "开始(&S)";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(149, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "输出码率            Kbps";
            // 
            // rateBox
            // 
            this.rateBox.Location = new System.Drawing.Point(79, 84);
            this.rateBox.Name = "rateBox";
            this.rateBox.Size = new System.Drawing.Size(62, 21);
            this.rateBox.TabIndex = 6;
            this.rateBox.Text = "900";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(553, 20);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(167, 38);
            this.button1.TabIndex = 7;
            this.button1.Text = "选择文件";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // fuckSina
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 262);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.rateBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.outputFileBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.inputFileBox);
            this.Controls.Add(this.label1);
            this.Name = "fuckSina";
            this.Text = "后黑";
            this.Load += new System.EventHandler(this.fuckSina_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox inputFileBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox outputFileBox;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox rateBox;
        private System.Windows.Forms.Button button1;
    }
}

