﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace FoxScreen
{
    public partial class frmMain : Form
    {
        public frmPickArea pickArea;
        KeyboardHook hook = new KeyboardHook();

        public frmMain()
        {
            InitializeComponent();

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(0, Keys.PrintScreen);
            hook.RegisterHotKey(ModifierKeysH.Alt, Keys.PrintScreen);
            hook.RegisterHotKey(ModifierKeysH.Control, Keys.PrintScreen);

            try
            {
                string[] lines = File.ReadAllLines("config.cfg");
                tbHost.Text = lines[0];
                tbUser.Text = lines[1];
                tbPword.Text = lines[2];
                tbLB.Text = lines[3];
            }
            catch { }

            screenShotProgress = new frmProgress();
            screenShotProgress.Show();
            screenShotProgress.Hide();
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == ModifierKeysH.Control)
            {
                SelectAreaScreenShot();
            }
            else if (e.Modifier == ModifierKeysH.Alt)
            {
                CurWndScreenShot();
            }
            else
            {
                CompleteScreenShot();
            }
        }

        public void SelectAreaScreenShot()
        {
            if (pickArea != null) return;
            pickArea = new frmPickArea();
            pickArea.main = this;
            pickArea.Show();
        }

        public void CompleteScreenShot()
        {
            int x = 0; int y = 0;
            int mx = 0; int my = 0;
            int c;
            foreach (Screen scr in Screen.AllScreens)
            {
                c = scr.Bounds.Left + scr.Bounds.Width;
                if (c > x) x = c;

                c = scr.Bounds.Top + scr.Bounds.Height;
                if (c > y) y = c;

                if (scr.Bounds.Left < mx) mx = scr.Bounds.Left;
                if (scr.Bounds.Top < my) my = scr.Bounds.Right;
            }
            AreaScreenShot(mx, my, x - mx, y - my);
        }

        public void CurWndScreenShot()
        {
            IntPtr ahWnd = GetForegroundWindow();
            RECT rect;
            GetWindowRect(ahWnd, out rect);
            AreaScreenShot(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, GetWindowTitle(ahWnd));
        }
        public void AreaScreenShot(int x, int y, int width, int height)
        {
            AreaScreenShot(x, y, width, height, "FoxScreen");
        }
        public void AreaScreenShot(int x, int y, int width, int height, string customname)
        {
            AreaScreenShot(x, y, new Size(width, height), customname);
        }
        public void AreaScreenShot(int x, int y, Size size)
        {
            AreaScreenShot(x, y, size, "FoxScreen");
        }

        internal class ScreenShotParams {
            public readonly int x;
            public readonly int y;
            public readonly Size size;
            public readonly string customname;

            public ScreenShotParams(int x, int y, Size size, string customname)
            {
                this.x = x;
                this.y = y;
                this.size = size;
                this.customname = customname;
            }
        }

        Thread screenShotterThread;
        frmProgress screenShotProgress;

        public void AreaScreenShot(int x, int y, Size size, string customname)
        {
            if (screenShotterThread != null && screenShotterThread.IsAlive) return;
            screenShotterThread = new Thread(new ParameterizedThreadStart(_AreaScreenShot));
            screenShotterThread.Start(new ScreenShotParams(x, y, size, customname));
        }

        private void _AreaScreenShot(object obj)
        {
            ScreenShotParams ssp = (ScreenShotParams)obj;
            int x = ssp.x; int y = ssp.y; Size size = ssp.size; string customname = ssp.customname;
            
            if (customname != "FoxScreen") customname = "FS_" + customname;

            int imax = customname.Length;
            char c;
            char[] cna = customname.ToCharArray(0, imax);
            for (int i = 0; i < imax; i++)
            {
                c = cna[i];
                if((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9') && c != '.' && c != '-' && c != '_')
                {
                    cna[i] = '_';
                }
            }
            customname = new String(cna);

            Image watermark = Image.FromFile("watermark.png");

            Bitmap b = new Bitmap(size.Width  + 20, size.Height + 20,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);

            g.Clear(Color.White);
            g.CopyFromScreen(x, y, 10, 10, size);

            /*Font font = new Font("Arial",(Math.Min(size.Width,size.Height) / 6),FontStyle.Regular,GraphicsUnit.Pixel);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            Brush brush = new SolidBrush(Color.FromArgb(64, 255, 0, 255));
            g.DrawString("FoxScreen\n(c) Doridian",font,brush,new PointF(size.Width / 2, size.Height / 2),format);*/

            int imgHeight = 64;
            int imgWidth = (int)(((float)watermark.Width / (float)watermark.Height) * (float)imgHeight);
            //int imgWidth = 32;
            //int imgHeight = (watermark.Height / watermark.Width) * imgWidth;


            g.DrawImage(watermark, (b.Width - imgWidth) - 5, (b.Height - imgHeight) - 5, imgWidth, imgHeight);

            g.Flush();
            g.Dispose();
            try
            {
                customname = customname + "_" + FixTwoChar(DateTime.Now.Day) + "-" + FixTwoChar(DateTime.Now.Month) + "-" + DateTime.Now.Year + "_" + FixTwoChar(DateTime.Now.Hour) + "-" + FixTwoChar(DateTime.Now.Minute) + "-" + FixTwoChar(DateTime.Now.Second) + ".png";

                screenShotProgress.SetStatus("Uploading: " + customname);
                screenShotProgress.SetProgress(0);
                screenShotProgress.DoShow();
                
                FtpWebRequest ftpr = (FtpWebRequest)FtpWebRequest.Create(tbHost.Text + "/" + customname);
                ftpr.Method = WebRequestMethods.Ftp.UploadFile;
                ftpr.UsePassive = true;
                ftpr.UseBinary = true;
                ftpr.Credentials = new NetworkCredential(tbUser.Text, tbPword.Text);
                Stream str = ftpr.GetRequestStream();
                MemoryStream mstr = new MemoryStream();
                b.Save(mstr, System.Drawing.Imaging.ImageFormat.Png);
                mstr.Seek(0,SeekOrigin.Begin);

                byte[] buffer = new byte[256];
                int readb;
                while (mstr.CanRead)
                {
                    Thread.Sleep(200);

                    readb = (int)(mstr.Length - mstr.Position);
                    if(readb > 256) readb = 256;
                    readb = mstr.Read(buffer, 0, readb);
                    if (readb <= 0) break;
                    str.Write(buffer,0,readb);

                    screenShotProgress.SetProgress(((float)mstr.Position) / ((float)mstr.Length));
                }
                str.Close();
                mstr.Close();
                FtpWebResponse resp = (FtpWebResponse)ftpr.GetResponse();
                resp.Close();

                screenShotProgress.SetStatus("Saved as: " + customname);
                screenShotProgress.SetProgress(1);
                screenShotProgress.DoHide();

                customname = tbLB.Text + customname;
                this.Invoke(new MethodInvoker(delegate() {
                    Clipboard.SetText(customname);
                }));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ERROR!");
            }
        }

        private void btnFullshot_Click(object sender, EventArgs e)
        {
            CompleteScreenShot();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText("config.cfg", tbHost.Text + Environment.NewLine + tbUser.Text + Environment.NewLine + tbPword.Text + Environment.NewLine + tbLB.Text);
        }

        private string FixTwoChar(int num)
        {
            if (num < 10) return "0" + num.ToString();
            return num.ToString();
        }


        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendMessage(IntPtr handleWindow, uint message, uint wParam, StringBuilder lParam);

        private static string GetWindowTitle(IntPtr handleWindow)
        {
            StringBuilder sb = new StringBuilder(256);
            SendMessage(handleWindow, 0x000D /*WM_GETTEXT*/, 256, sb);
            return sb.ToString();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible) this.Hide();
            else { this.Show(); this.Activate(); }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (File.Exists("config.cfg")) this.Hide();
        }
    }
}
