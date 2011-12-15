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
            targetHeight = this.Height;
        }

        private void SetLocation()
        {
            Rectangle wArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(wArea.Right - this.Width, wArea.Bottom - this.Height);
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
            SetLocation();
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

        private List<Label> labels = new List<Label>();
        public void AddLabel(string text)
        {
            this.Invoke(new MethodInvoker(delegate() {
                Label lbNew = new Label();
                lbNew.Text = text;
                lbNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                lbNew.AutoSize = true;
                labels.Add(lbNew);
                this.Controls.Add(lbNew);
                lbNew.Location = new Point(lbStatus.Left, -(lbNew.Height - 5 - (this.Height - targetHeight)));
                targetHeight += lbNew.Height;
            }));
        }

        public void RemoveLastLabel()
        {
            this.Invoke(new MethodInvoker(delegate() {
                Label lbOld = labels[0];
                targetHeight -= lbOld.Height;
                labelsMove += lbOld.Height;
                labels.RemoveAt(0);
                this.Controls.Remove(lbOld);
            }));
        }

        private int targetHeight;
        private int labelsMove;
        private void tmResize_Tick(object sender, EventArgs e)
        {
            if (labelsMove > 0)
            {
                labelsMove--;
                foreach (Label lbCur in labels)
                {
                    lbCur.Top++;
                }
            }

            if (this.Height > targetHeight)
            {
                this.Height--;
            }
            else if (this.Height < targetHeight)
            {
                this.Height++;
            }
            else
            {
                return;
            }

            SetLocation();
        }
    }
}
