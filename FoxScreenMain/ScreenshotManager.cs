using SlimDX.Direct3D9;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FoxScreen
{
    public class ScreenshotManager
    {
        private Device[] dxdevices;
        private Point[] dxdevice_offsets;
        private Size[] dxdevice_sizes;

        private UploadOrganizer uploadOrganizer;

        public ScreenshotManager(UploadOrganizer m_uploadOrganizer)
        {
            uploadOrganizer = m_uploadOrganizer;
            ResolutionChanged();
        }

        public void ResolutionChanged()
        {
            Direct3D d3d = new Direct3D();
            PresentParameters present_params = new PresentParameters();
            present_params.Windowed = true;
            present_params.SwapEffect = SwapEffect.Discard;

            dxdevice_offsets = new Point[Screen.AllScreens.Length];
            dxdevice_sizes = new Size[Screen.AllScreens.Length];
            dxdevices = new Device[Screen.AllScreens.Length];

            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                dxdevices[i] = new Device(d3d, i, DeviceType.Hardware, IntPtr.Zero, CreateFlags.SoftwareVertexProcessing, present_params);
                dxdevice_offsets[i] = Screen.AllScreens[i].Bounds.Location;
                dxdevice_sizes[i] = Screen.AllScreens[i].Bounds.Size;
            }
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
            MemoryStream mstr = new MemoryStream();
            bitmap.Save(mstr, System.Drawing.Imaging.ImageFormat.Png);
            uploadOrganizer.AddUpload(customname + ".png", mstr);
        }

        public void MakeScreenShotFromBitmap(string customname, Bitmap bitmap, Rectangle rect)
        {
            Bitmap b = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(bitmap, 0, 0, rect, GraphicsUnit.Pixel);
            g.Flush();
            MakeScreenShotFromBitmap(customname, b);
        }

        public void AreaScreenShot(Rectangle rect)
        {
            AreaScreenShot(rect.X, rect.Y, rect.Size, "Screenshot");
        }

        public void AreaScreenShot(int x, int y, Size size, string customname)
        {
            AreaScreenShot(new Rectangle(x, y, size.Width, size.Height), customname);
        }

        public void AreaScreenShot(Rectangle rect, string customname)
        {
            MakeScreenShotFromBitmap(customname, MakeBitmapFromScreen(), rect);
        }

        internal class NativeMethods
        {
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

            [DllImport("user32.dll")]
            private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint SendMessage(IntPtr handleWindow, uint message, uint wParam, StringBuilder lParam);

            public static string GetActiveWindowTitle()
            {
                return GetWindowTitle(GetForegroundWindow());
            }

            public static Rectangle GetActiveWindowAbsoluteClientRect()
            {
                return GetAbsoluteClientRect(GetForegroundWindow());
            }

            public static string GetWindowTitle(IntPtr handleWindow)
            {
                StringBuilder sb = new StringBuilder(256);
                SendMessage(handleWindow, 0x000D /*WM_GETTEXT*/, 256, sb);
                return sb.ToString();
            }

            public static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
            {
                RECT windowRect;
                //RECT clientRect;

                GetWindowRect(hWnd, out windowRect);
                //GetClientRect(hWnd, out clientRect);

                Size windowRectSize = new Size(windowRect.Right - windowRect.Left, windowRect.Bottom - windowRect.Top);
                //Size clientRectSize = new Size(clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top);

                return new Rectangle(new Point(windowRect.Left, windowRect.Top), windowRectSize);

                // This gives us the width of the left, right and bottom chrome - we can then determine the top height
                //int chromeWidth = (int)((windowRectSize.Width - clientRectSize.Width) / 2);

                //return new Rectangle(new Point(windowRect.Left + chromeWidth, windowRect.Top + (windowRectSize.Height - clientRectSize.Height - chromeWidth)), clientRectSize);
            }
        }

        public Bitmap MakeBitmapFromScreen()
        {
            Rectangle completeScreen = GetCompleteScreen();

            Bitmap b = new Bitmap(completeScreen.Width, completeScreen.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);

            for (int i = 0; i < dxdevices.Length; i++)
            {
                Surface surface = Surface.CreateOffscreenPlain(dxdevices[i], dxdevice_sizes[i].Width, dxdevice_sizes[i].Height, Format.A8R8G8B8, Pool.Scratch);
                dxdevices[i].GetFrontBufferData(0, surface);
                Bitmap bmp = new Bitmap(Surface.ToStream(surface, ImageFileFormat.Bmp));
                g.DrawImage(bmp, new Rectangle(dxdevice_offsets[i].X - completeScreen.X, dxdevice_offsets[i].Y - completeScreen.Y, completeScreen.Width, completeScreen.Height), 0, 0, completeScreen.Width, completeScreen.Height, GraphicsUnit.Pixel);
                bmp.Dispose();
                surface.Dispose();
            }

            g.Flush();
            return b;
        }
    }
}
