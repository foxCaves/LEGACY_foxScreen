using System;
using System.Drawing;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor.WinApi;
using MouseKeyboardActivityMonitor;

namespace FoxScreen
{
    public partial class frmPickArea : Form
    {
        private KeyboardHookListener keyboardHookManager;
        private MouseHookListener mouseHookManager;

        public frmMain main;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        Point startPos = new Point();

        public frmPickArea()
        {
            InitializeComponent();

            keyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            keyboardHookManager.Enabled = true;

            mouseHookManager = new MouseHookListener(new GlobalHooker());
            mouseHookManager.Enabled = true;

            mouseHookManager.MouseMove += HookManager_MouseMove;
            mouseHookManager.MouseClick += HookManager_MouseClick;
            keyboardHookManager.KeyDown += HookManager_KeyDown; 

            GetCursorPos(ref startPos);

            AdaptTo(startPos);
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
            if (e.Button != MouseButtons.Left)
                this.Cancel();
            else
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

            mouseHookManager.MouseMove -= HookManager_MouseMove;
            mouseHookManager.MouseClick -= HookManager_MouseClick;
            keyboardHookManager.KeyDown -= HookManager_KeyDown;

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
            main.AreaScreenShot(this.Left - 1, this.Top - 1, this.Width - 2, this.Height - 2);
            this.Close();
        }

        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            AdaptTo(new Point(e.X, e.Y));
        }

        private void AdaptTo(Point e)
        {
            if (e.X < startPos.X)
            {
                this.Left = e.X - 1;
                this.Width = (startPos.X - e.X) + 2;
            }
            else
            {
                this.Left = startPos.X - 1;
                this.Width = (e.X - startPos.X) + 2;
            }

            if (e.Y < startPos.Y)
            {
                this.Top = e.Y - 1;
                this.Height = (startPos.Y - e.Y) + 2;
            }
            else
            {
                this.Top = startPos.Y - 1;
                this.Height = (e.Y - startPos.Y) + 2;
            }

            this.Refresh();
        }
    }
}
