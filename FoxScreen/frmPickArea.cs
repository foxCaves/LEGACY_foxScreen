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

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        private Bitmap screenBitmap;

        private PullPoint top_left, bottom_right;
        private PullPoint[] pullPoints = new PullPoint[4];

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

            pullPoints[0] = new PullPoint();
            pullPoints[1] = new PullPoint();
            pullPoints[2] = new PullPoint();
            pullPoints[3] = new PullPoint();

            pullPoints[0].addSyncPointX(pullPoints[2]);
            pullPoints[1].addSyncPointX(pullPoints[3]);
            pullPoints[0].addSyncPointY(pullPoints[1]);
            pullPoints[2].addSyncPointY(pullPoints[3]);

            top_left = pullPoints[0];
            bottom_right = pullPoints[3];

            top_left.X = 0;
            top_left.Y = 0;
            bottom_right.X = this.Width;
            bottom_right.Y = this.Height;
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
            main.screenshotManager.MakeScreenShotFromBitmap("Area", screenBitmap, getSelectionRectangle());
            this.Close();
        }

        private Rectangle getSelectionRectangle()
        {
            Rectangle rect = new Rectangle();
            rect.X = top_left.X;
            rect.Y = top_left.Y;
            rect.Width = bottom_right.X - top_left.X;
            rect.Height = bottom_right.Y - top_left.Y;
            return rect;
        }

        private Brush alphaBrush = new SolidBrush(Color.FromArgb(128, 100, 100, 100));
        private void frmPickArea_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(screenBitmap, 0, 0);

            Rectangle selection = getSelectionRectangle();
            g.DrawRectangle(Pens.Red, selection);
            
            g.SetClip(new Region(selection), System.Drawing.Drawing2D.CombineMode.Exclude);
            g.FillRectangle(alphaBrush, new Rectangle(0, 0, this.Width, this.Height));
        }

        class PullPoint
        {
            int m_X;
            public int X
            {
                get
                {
                    return m_X;
                }
                set
                {
                    m_X = value;
                    foreach (PullPoint p in syncX)
                    {
                        if (p == this) continue;
                        p.m_X = value;
                    }
                }
            }

            int m_Y;
            public int Y
            {
                get
                {
                    return m_Y;
                }
                set
                {
                    m_Y = value;
                    foreach (PullPoint p in syncY)
                    {
                        if (p == this) continue;
                        p.m_Y = value;
                    }
                }
            }

            public PullPoint() : this(0, 0)
            {

            }

            public PullPoint(int X, int Y)
            {
                m_X = X;
                m_Y = Y;
            }

            private List<PullPoint> syncX = new List<PullPoint>();
            public void addSyncPointX(PullPoint other)
            {
                syncX.Add(other);
                other.syncX.Add(this);
            }
            public void delSyncPointX(PullPoint other)
            {
                syncX.Remove(other);
                other.syncX.Remove(this);
            }

            private List<PullPoint> syncY = new List<PullPoint>();
            public void addSyncPointY(PullPoint other)
            {
                syncY.Add(other);
                other.syncY.Add(this);
            }
            public void delSyncPointY(PullPoint other)
            {
                syncY.Remove(other);
                other.syncY.Remove(this);
            }

            public int distanceSq(Point other)
            {
                int XD = other.X - this.m_X;
                int YD = other.Y - this.m_Y;
                return (XD * XD) + (YD * YD);
            }

            public int distanceSq(PullPoint other)
            {
                int XD = other.m_X - this.m_X;
                int YD = other.m_Y - this.m_Y;
                return (XD * XD) + (YD * YD);
            }

            public Point getPoint()
            {
                return new Point(X, Y);
            }
        }

        private PullPoint currentPulling;
        private void frmPickArea_MouseDown(object sender, MouseEventArgs e)
        {
            int minDist = int.MaxValue;
            PullPoint decided = null;
            Point pos = e.Location;
            foreach(PullPoint p in pullPoints)
            {
                int dist = p.distanceSq(pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    decided = p;
                }
                else if (dist == minDist)
                {
                    decided = null;
                }
            }
            currentPulling = decided;
        }

        private void frmPickArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentPulling == null) return;

            currentPulling.X = e.X;
            currentPulling.Y = e.Y;

            this.Invalidate();
        }

        private void frmPickArea_MouseUp(object sender, MouseEventArgs e)
        {
            currentPulling = null;
        }
    }
}
