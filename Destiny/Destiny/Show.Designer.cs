namespace Destiny
{
    partial class Show
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
            this.labelType = new System.Windows.Forms.Label();
            this.richLong = new System.Windows.Forms.RichTextBox();
            this.richWhat = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelType.Location = new System.Drawing.Point(12, 146);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(54, 21);
            this.labelType.TabIndex = 1;
            this.labelType.Text = "事业";
            // 
            // richLong
            // 
            this.richLong.BackColor = System.Drawing.SystemColors.Control;
            this.richLong.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richLong.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richLong.Location = new System.Drawing.Point(12, 12);
            this.richLong.Name = "richLong";
            this.richLong.Size = new System.Drawing.Size(530, 119);
            this.richLong.TabIndex = 2;
            this.richLong.Text = "long";
            // 
            // richWhat
            // 
            this.richWhat.BackColor = System.Drawing.SystemColors.Control;
            this.richWhat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richWhat.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richWhat.Location = new System.Drawing.Point(12, 186);
            this.richWhat.Name = "richWhat";
            this.richWhat.Size = new System.Drawing.Size(515, 74);
            this.richWhat.TabIndex = 2;
            this.richWhat.Text = "说";
            // 
            // Show
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 272);
            this.Controls.Add(this.richWhat);
            this.Controls.Add(this.richLong);
            this.Controls.Add(this.labelType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Show";
            this.Text = "Show";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.RichTextBox richLong;
        private System.Windows.Forms.RichTextBox richWhat;
    }
}