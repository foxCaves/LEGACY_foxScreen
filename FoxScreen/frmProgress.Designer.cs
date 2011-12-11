namespace FoxScreen
{
    partial class frmProgress
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
            this.components = new System.ComponentModel.Container();
            this.pbUpload = new System.Windows.Forms.ProgressBar();
            this.lbUpload = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.tmHide = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // pbUpload
            // 
            this.pbUpload.Location = new System.Drawing.Point(12, 25);
            this.pbUpload.Name = "pbUpload";
            this.pbUpload.Size = new System.Drawing.Size(225, 18);
            this.pbUpload.TabIndex = 0;
            // 
            // lbUpload
            // 
            this.lbUpload.AutoSize = true;
            this.lbUpload.Location = new System.Drawing.Point(237, 28);
            this.lbUpload.Name = "lbUpload";
            this.lbUpload.Size = new System.Drawing.Size(33, 13);
            this.lbUpload.TabIndex = 1;
            this.lbUpload.Text = "100%";
            // 
            // lbStatus
            // 
            this.lbStatus.AutoSize = true;
            this.lbStatus.Location = new System.Drawing.Point(12, 9);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(166, 13);
            this.lbStatus.TabIndex = 2;
            this.lbStatus.Text = "Uploading: ###############";
            // 
            // tmHide
            // 
            this.tmHide.Interval = 1000;
            this.tmHide.Tick += new System.EventHandler(this.tmHide_Tick);
            // 
            // frmProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 52);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.lbUpload);
            this.Controls.Add(this.pbUpload);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmProgress";
            this.Opacity = 0.5D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmProgress";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmProgress_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbUpload;
        private System.Windows.Forms.Label lbUpload;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.Timer tmHide;
    }
}