namespace FoxScreen
{
    partial class frmPickArea
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
            this.SuspendLayout();
            // 
            // frmPickArea
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(300, 300);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmPickArea";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "frmPickArea";
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.CloseMeEvent);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmPickArea_Paint);
            this.Leave += new System.EventHandler(this.CloseMeEvent);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.frmPickArea_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.frmPickArea_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.frmPickArea_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

    }
}