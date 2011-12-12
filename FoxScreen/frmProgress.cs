using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FoxScreen
{
    public partial class frmProgress : Form
    {
        public frmProgress()
        {
            InitializeComponent();
        }

        private void frmProgress_Load(object sender, EventArgs e)
        {

        }

        public void DoShow()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DoShow));
                return;
            }

            tmHide.Enabled = false;

            this.Opacity = 0.6;
            this.Show();

            Rectangle wArea = Screen.PrimaryScreen.WorkingArea;

            this.Location = new Point(wArea.Right - this.Width, wArea.Bottom - this.Height);
        }

        public void DoHide()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(DoHide));
                return;
            }

            tmHide.Interval = 1000;
            tmHide.Enabled = true;
        }

        public void SetProgress(float percentage)
        {
            int perc = (int)(percentage * 100);

            this.Invoke(new MethodInvoker(delegate() {
                pbUpload.Value = perc;
                lbUpload.Text = perc + "%";
            }));
        }

        public void SetStatus(string status)
        {
            this.Invoke(new MethodInvoker(delegate() {
                lbStatus.Text = status;
            }));
        }

        public void SetBackColor(Color backColor)
        {
            this.Invoke(new MethodInvoker(delegate() {
                this.BackColor = backColor;
            }));
        }

        private void tmHide_Tick(object sender, EventArgs e)
        {
            tmHide.Interval = 10;
            this.Opacity -= 0.01;
            if (this.Opacity <= 0)
            {
                tmHide.Enabled = false;
                this.Hide();
            }
        }
    }
}
