namespace FoxScreen
{
    partial class frmDropArea
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
            this.lbDragDrop = new System.Windows.Forms.Label();
            this.tmOpacity = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lbDragDrop
            // 
            this.lbDragDrop.AutoSize = true;
            this.lbDragDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDragDrop.ForeColor = System.Drawing.Color.Red;
            this.lbDragDrop.Location = new System.Drawing.Point(112, 104);
            this.lbDragDrop.Name = "lbDragDrop";
            this.lbDragDrop.Size = new System.Drawing.Size(87, 37);
            this.lbDragDrop.TabIndex = 1;
            this.lbDragDrop.Text = "Drag";
            // 
            // tmOpacity
            // 
            this.tmOpacity.Enabled = true;
            this.tmOpacity.Interval = 10;
            this.tmOpacity.Tick += new System.EventHandler(this.tmOpacity_Tick);
            // 
            // frmDropArea
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(311, 244);
            this.Controls.Add(this.lbDragDrop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmDropArea";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmDropArea";
            this.TopMost = true;
            this.Click += new System.EventHandler(this.frmDropArea_Click);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmDropArea_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmDropArea_DragEnter);
            this.DragLeave += new System.EventHandler(this.frmDropArea_DragLeave);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.frmDropArea_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbDragDrop;
        private System.Windows.Forms.Timer tmOpacity;

    }
}