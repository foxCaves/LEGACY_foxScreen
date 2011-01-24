using System;
using System.Drawing;
using System.Windows.Forms;
using Gma.UserActivityMonitor;

namespace FoxScreen
{
    public partial class frmPickArea : Form
    {
        public frmMain main;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        Point startPos = new Point();

        public frmPickArea()
        {
            InitializeComponent();
            HookManager.MouseMove += new MouseEventHandler(HookManager_MouseMove);
            HookManager.KeyDown += new KeyEventHandler(HookManager_KeyDown);
            HookManager.MouseClick += new MouseEventHandler(HookManager_MouseClick);

            GetCursorPos(ref startPos);
            this.Location = new Point(startPos.X,startPos.Y);
        }

        ~frmPickArea()
        {
            this.Close();
        }

        private void frmPickArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.Lime);
            g.DrawRectangle(Pens.Red, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void CloseMeEvent(object sender, EventArgs e)
        {
            this.Cancel();
        }

        void HookManager_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            this.DoShot();
        }

        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Cancel();
                    break;
                case Keys.Enter:
                case Keys.Space:
                    this.DoShot();
                    break;
            }
        }

        public new void Close()
        {
            main.pickArea = null;
            HookManager.MouseMove -= new MouseEventHandler(HookManager_MouseMove);
            HookManager.KeyDown -= new KeyEventHandler(HookManager_KeyDown);
            HookManager.MouseClick -= new MouseEventHandler(HookManager_MouseClick);
            base.Close();
            this.Dispose();
        }

        private void Cancel()
        {
            this.Close();
        }

        private void DoShot()
        {
            this.Visible = false;
            this.Refresh();
            main.AreaScreenShot(this.Left, this.Top, this.Size);
            this.Close();
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X < startPos.X)
            {
                this.Left = e.X;
                this.Width = (startPos.X - e.X) + 1;
            }
            else
            {
                this.Left = startPos.X;
                this.Width = (e.X - startPos.X) + 1;
            }

            if (e.Y < startPos.Y)
            {
                this.Top = e.Y;
                this.Height = (startPos.Y - e.Y) + 1;
            }
            else
            {
                this.Top = startPos.Y;
                this.Height = (e.Y - startPos.Y) + 1;
            }

            this.Refresh();
        }
    }
}
