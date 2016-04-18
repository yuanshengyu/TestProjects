namespace Destiny
{
    partial class ForWhat
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.radioCause = new System.Windows.Forms.RadioButton();
            this.radioBusiness = new System.Windows.Forms.RadioButton();
            this.radioFame = new System.Windows.Forms.RadioButton();
            this.radioGoOut = new System.Windows.Forms.RadioButton();
            this.radioLove = new System.Windows.Forms.RadioButton();
            this.radioDecision = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // radioCause
            // 
            this.radioCause.AutoSize = true;
            this.radioCause.Location = new System.Drawing.Point(38, 24);
            this.radioCause.Name = "radioCause";
            this.radioCause.Size = new System.Drawing.Size(47, 16);
            this.radioCause.TabIndex = 0;
            this.radioCause.Text = "事业";
            this.radioCause.UseVisualStyleBackColor = true;
            // 
            // radioBusiness
            // 
            this.radioBusiness.AutoSize = true;
            this.radioBusiness.Location = new System.Drawing.Point(38, 63);
            this.radioBusiness.Name = "radioBusiness";
            this.radioBusiness.Size = new System.Drawing.Size(47, 16);
            this.radioBusiness.TabIndex = 0;
            this.radioBusiness.Text = "经商";
            this.radioBusiness.UseVisualStyleBackColor = true;
            // 
            // radioFame
            // 
            this.radioFame.AutoSize = true;
            this.radioFame.Location = new System.Drawing.Point(38, 102);
            this.radioFame.Name = "radioFame";
            this.radioFame.Size = new System.Drawing.Size(47, 16);
            this.radioFame.TabIndex = 0;
            this.radioFame.Text = "求名";
            this.radioFame.UseVisualStyleBackColor = true;
            // 
            // radioGoOut
            // 
            this.radioGoOut.AutoSize = true;
            this.radioGoOut.Location = new System.Drawing.Point(38, 141);
            this.radioGoOut.Name = "radioGoOut";
            this.radioGoOut.Size = new System.Drawing.Size(47, 16);
            this.radioGoOut.TabIndex = 0;
            this.radioGoOut.Text = "外出";
            this.radioGoOut.UseVisualStyleBackColor = true;
            // 
            // radioLove
            // 
            this.radioLove.AutoSize = true;
            this.radioLove.Checked = true;
            this.radioLove.Location = new System.Drawing.Point(38, 180);
            this.radioLove.Name = "radioLove";
            this.radioLove.Size = new System.Drawing.Size(47, 16);
            this.radioLove.TabIndex = 0;
            this.radioLove.TabStop = true;
            this.radioLove.Text = "婚恋";
            this.radioLove.UseVisualStyleBackColor = true;
            // 
            // radioDecision
            // 
            this.radioDecision.AutoSize = true;
            this.radioDecision.Location = new System.Drawing.Point(38, 219);
            this.radioDecision.Name = "radioDecision";
            this.radioDecision.Size = new System.Drawing.Size(47, 16);
            this.radioDecision.TabIndex = 0;
            this.radioDecision.Text = "决策";
            this.radioDecision.UseVisualStyleBackColor = true;
            // 
            // ForWhat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 262);
            this.Controls.Add(this.radioDecision);
            this.Controls.Add(this.radioLove);
            this.Controls.Add(this.radioGoOut);
            this.Controls.Add(this.radioFame);
            this.Controls.Add(this.radioBusiness);
            this.Controls.Add(this.radioCause);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ForWhat";
            this.Text = "ForWhat";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioCause;
        private System.Windows.Forms.RadioButton radioBusiness;
        private System.Windows.Forms.RadioButton radioFame;
        private System.Windows.Forms.RadioButton radioGoOut;
        private System.Windows.Forms.RadioButton radioLove;
        private System.Windows.Forms.RadioButton radioDecision;
    }
}