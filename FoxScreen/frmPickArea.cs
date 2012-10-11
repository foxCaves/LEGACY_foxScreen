using System;
using System.Drawing;
using System.Windows.Forms;
using MouseKeyboardActivityMonitor.WinApi;
using MouseKeyboardActivityMonitor;
using System.Collections.Generic;

namespace FoxScreen
{
    public partial class frmPickArea : Form
    {
        private KeyboardHookListener keyboardHookManager;

        public frmMain main;

        private Bitmap screenBitmap;

        public frmPickArea(frmMain mainFrm)
        {
            main = mainFrm;

            InitializeComponent();

            keyboardHookManager = new KeyboardHookListener(new GlobalHooker());
            keyboardHookManager.Enabled = true;
            keyboardHookManager.KeyDown += HookManager_KeyDown; 

            Rectangle rect = main.screenshotManager.GetCompleteScreen();
            screenBitmap = main.screenshotManager.MakeBitmapFromScreen(rect.X, rect.Y, rect.Size);

            this.Location = rect.Location;
            this.Size = rect.Size;
        }

        ~frmPickArea()
        {
            this.Close();
        }

        private void CloseMeEvent(object sender, EventArgs e)
        {
            this.Cancel();
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
            main.screenshotManager.MakeScreenShotFromBitmap("Area", screenBitmap, GetSelectionRectangle());
            this.Close();
        }

        private Rectangle GetSelectionRectangle()
        {
            Rectangle rect = new Rectangle();

            int minX, maxX, minY, maxY;

            if (startPoint.X <= endPoint.X)
            {
                minX = startPoint.X;
                maxX = endPoint.X;
            }
            else
            {
                minX = endPoint.X;
                maxX = startPoint.X;
            }

            if (startPoint.Y <= endPoint.Y)
            {
                minY = startPoint.Y;
                maxY = endPoint.Y;
            }
            else
            {
                minY = endPoint.Y;
                maxY = startPoint.Y;
            }

            rect.X = minX;
            rect.Y = minY;
            rect.Width = maxX - minX;
            rect.Height = maxY - minY;

            return rect;
        }

        private Brush alphaBrush = new SolidBrush(Color.FromArgb(128, 100, 100, 100));
        private void frmPickArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(screenBitmap, 0, 0);

            Rectangle selection = GetSelectionRectangle();
            g.DrawRectangle(Pens.Red, selection);
            
            g.SetClip(new Region(selection), System.Drawing.Drawing2D.CombineMode.Exclude);
            g.FillRectangle(alphaBrush, new Rectangle(0, 0, this.Width, this.Height));
        }

        private Point startPoint;
        private Point endPoint;
        private bool isPulling = false;

        private void frmPickArea_MouseDown(object sender, MouseEventArgs e)
        {
            startPoint = e.Location;
            endPoint = new Point(e.Location.X, e.Location.Y);
            isPulling = true;
        }

        private void frmPickArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPulling) return;

            endPoint.X = e.X;
            endPoint.Y = e.Y;

            this.Invalidate();
        }

        private void frmPickArea_MouseUp(object sender, MouseEventArgs e)
        {
            DoShot();
        }

        private void frmPickArea_Load(object sender, EventArgs e)
        {

        }
    }
}
