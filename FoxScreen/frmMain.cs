using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;

namespace FoxScreen
{
    public partial class frmMain : Form
    {
        KeyboardHook hook = new KeyboardHook();

        public frmMain()
        {
            InitializeComponent();

            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(0, Keys.PrintScreen);
            hook.RegisterHotKey(ModifierKeysH.Alt, Keys.PrintScreen);

            try
            {
                string[] lines = File.ReadAllLines("config.cfg");
                tbHost.Text = lines[0];
                tbUser.Text = lines[1];
                tbPword.Text = lines[2];
                tbLB.Text = lines[3];
            }
            catch { }
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == ModifierKeysH.Alt)
            {
                CurWndScreenShot();
            }
            else
            {
                CompleteScreenShot();
            }
        }

        public void CompleteScreenShot()
        {
            Size allScreensSize = new Size(0, 0);
            foreach (Screen scr in Screen.AllScreens)
            {
                allScreensSize += scr.Bounds.Size;
            }
            AreaScreenShot(0, 0, allScreensSize);
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
        public void AreaScreenShot(int x, int y, Size size, string customname)
        {
            if (customname != "FoxScreen") customname = "FS_" + customname;

            int imax = customname.Length;
            char c;
            char[] cna = customname.ToCharArray(0, imax);
            for (int i = 0; i < imax; i++)
            {
                c = cna[i];
                if((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                {
                    cna[i] = '_';
                }
            }
            customname = new String(cna);

            Font font = new Font("Arial",(Math.Min(size.Width,size.Height) / 6),FontStyle.Regular,GraphicsUnit.Pixel);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            Brush brush = new SolidBrush(Color.FromArgb(64, 255, 0, 255));

            Image watermark = Image.FromFile("watermark.png");

            Bitmap b = new Bitmap(size.Width  + 20, size.Height + 20,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);

            g.Clear(Color.White);
            g.CopyFromScreen(x, y, 10, 10, size);

            g.DrawString("FoxScreen\n(c) Doridian",font,brush,new PointF(size.Width / 2, size.Height / 2),format);

            int imgWidth = 32;
            int imgHeight = (watermark.Height / watermark.Width) * imgWidth;


            g.DrawImage(watermark, (b.Width - imgWidth) - 5, (b.Height - imgHeight) - 5, imgWidth, imgHeight);

            g.Flush();
            g.Dispose();
            try
            {
                customname = customname + "_" + FixTwoChar(DateTime.Now.Day) + "-" + FixTwoChar(DateTime.Now.Month) + "-" + DateTime.Now.Year + "_" + FixTwoChar(DateTime.Now.Hour) + "-" + FixTwoChar(DateTime.Now.Minute) + "-" + FixTwoChar(DateTime.Now.Second) + ".png";
                FtpWebRequest ftpr = (FtpWebRequest)FtpWebRequest.Create(tbHost.Text + "/" + customname);
                ftpr.Method = WebRequestMethods.Ftp.UploadFile;
                ftpr.UsePassive = true;
                ftpr.UseBinary = true;
                ftpr.Credentials = new NetworkCredential(tbUser.Text, tbPword.Text);
                Stream str = ftpr.GetRequestStream();
                b.Save(str, System.Drawing.Imaging.ImageFormat.Png);
                str.Close();
                FtpWebResponse resp = (FtpWebResponse)ftpr.GetResponse();
                resp.Close();

                customname = tbLB.Text + customname;
                Clipboard.SetText(customname);

                notifyIcon.ShowBalloonTip(1000, "FoxScreen: Screenshot uploaded!", "Saved as: " + customname, ToolTipIcon.Info);
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
            else this.Show();
        }
    }
}
