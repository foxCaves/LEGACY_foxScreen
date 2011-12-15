using System;
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
        frmDropArea dropForm;

        KeyboardHook kbHook = new KeyboardHook();
        UploadOrganizer uploadOrganizer = new UploadOrganizer();

        public frmMain()
        {
            InitializeComponent();

            kbHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            kbHook.RegisterHotKey(0, Keys.PrintScreen);
            kbHook.RegisterHotKey(ModifierKeysH.Alt, Keys.PrintScreen);
            kbHook.RegisterHotKey(ModifierKeysH.Control, Keys.PrintScreen);

            dropForm = new frmDropArea(uploadOrganizer);
            dropForm.Show();
            dropForm.Hide();

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

        private void AreaScreenShot(int x, int y, Size size, string customname)
        {            
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

            int imgHeight = 64;
            int imgWidth = (int)(((float)watermark.Width / (float)watermark.Height) * (float)imgHeight);

            g.DrawImage(watermark, (b.Width - imgWidth) - 5, (b.Height - imgHeight) - 5, imgWidth, imgHeight);

            g.Flush();

            MemoryStream mstr = new MemoryStream();
            b.Save(mstr, System.Drawing.Imaging.ImageFormat.Png);

            g.Dispose();

            uploadOrganizer.AddUpload(customname, "png", mstr);
        }

        private void btnFullshot_Click(object sender, EventArgs e)
        {
            CompleteScreenShot();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText("config.cfg", tbHost.Text + Environment.NewLine + tbUser.Text + Environment.NewLine + tbPword.Text + Environment.NewLine + tbLB.Text);
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

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            uploadOrganizer.Stop();
            Application.Exit();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            dropForm.ToggleVisibility();
        }
    }
}
