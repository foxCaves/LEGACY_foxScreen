using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FoxScreen
{
    public class ScreenshotManager
    {
        private UploadOrganizer uploadOrganizer;
        public ScreenshotManager(UploadOrganizer m_uploadOrganizer)
        {
            uploadOrganizer = m_uploadOrganizer;
        }

        public Rectangle GetCompleteScreen()
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
                if (scr.Bounds.Top < my) my = scr.Bounds.Top;
            }
            return new Rectangle(mx, my, x - mx, y - my);
        }

        public Rectangle GetCurWndRect()
        {
            IntPtr ahWnd = GetForegroundWindow();
            RECT rect;
            GetWindowRect(ahWnd, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public void AreaScreenShot(int x, int y, int width, int height)
        {
            AreaScreenShot(x, y, width, height, "Screenshot");
        }
        public void AreaScreenShot(int x, int y, int width, int height, string customname)
        {
            AreaScreenShot(x, y, new Size(width, height), customname);
        }
        public void AreaScreenShot(int x, int y, Size size)
        {
            AreaScreenShot(x, y, size, "Screenshot");
        }

        public void MakeScreenShotFromBitmap(string customname, Bitmap bitmap)
        {
            MakeScreenShotFromBitmap(customname, bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        }

        public void MakeScreenShotFromBitmap(string customname, Bitmap bitmap, Rectangle rect)
        {
            int imax = customname.Length;
            char c;
            char[] cna = customname.ToCharArray(0, imax);
            for (int i = 0; i < imax; i++)
            {
                c = cna[i];
                if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9') && c != '.' && c != '-' && c != '_')
                {
                    cna[i] = '_';
                }
            }
            customname = new String(cna);

            Image watermark = Image.FromFile("watermark.png");

            Bitmap b = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);

            g.Clear(Color.White);
            g.DrawImage(bitmap, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);

            int imgHeight = 64;
            int imgWidth = (int)(((float)watermark.Width / (float)watermark.Height) * (float)imgHeight);

            g.DrawImage(watermark, (b.Width - imgWidth) - 5, (b.Height - imgHeight) - 5, imgWidth, imgHeight);

            g.Flush();

            MemoryStream mstr = new MemoryStream();
            b.Save(mstr, System.Drawing.Imaging.ImageFormat.Png);

            g.Dispose();

            uploadOrganizer.AddUpload(customname + ".png", mstr);
        }

        public void AreaScreenShot(Rectangle rect)
        {
            AreaScreenShot(rect.X, rect.Y, rect.Size, "Screenshot");
        }

        public void AreaScreenShot(Rectangle rect, string customname)
        {
            AreaScreenShot(rect.X, rect.Y, rect.Size, customname);
        }

        public void AreaScreenShot(int x, int y, Size size, string customname)
        {
            Bitmap b = MakeBitmapFromScreen(x, y, size);
            MakeScreenShotFromBitmap(customname, MakeBitmapFromScreen(x, y, size));
        }

        public Bitmap MakeBitmapFromScreen(int x, int y, Size size)
        {
            Bitmap b = new Bitmap(size.Width + 20, size.Height + 20, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            g.CopyFromScreen(x, y, 10, 10, size);
            g.Flush();
            return b;
        }

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendMessage(IntPtr handleWindow, uint message, uint wParam, StringBuilder lParam);

        public static string GetWindowTitle(IntPtr handleWindow)
        {
            StringBuilder sb = new StringBuilder(256);
            SendMessage(handleWindow, 0x000D /*WM_GETTEXT*/, 256, sb);
            return sb.ToString();
        }
    }
}
