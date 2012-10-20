using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace FoxScreen
{
    public partial class frmMain : Form
    {
        public frmPickArea pickArea;
        frmDropArea dropForm;

        KeyboardHook kbHook = new KeyboardHook();

        public frmMain()
        {
            InitializeComponent();

            kbHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            kbHook.RegisterHotKey(0, Keys.PrintScreen);
            kbHook.RegisterHotKey(ModifierKeysH.Alt, Keys.PrintScreen);
            kbHook.RegisterHotKey(ModifierKeysH.Control, Keys.PrintScreen);

            dropForm = new frmDropArea(Main.uploadOrganizer);
            dropForm.Show();
            dropForm.Hide();

            Main.LoadCredentials();
            tbUser.Text = Main.username;
            tbPword.Text = Main.password;
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Modifier == ModifierKeysH.Control)
            {
                SelectAreaScreenShot();
            }
            else if (e.Modifier == ModifierKeysH.Alt)
            {
                Rectangle completeRect = Main.screenshotManager.GetCompleteScreen();
                Rectangle rect = ScreenshotManager.NativeMethods.GetActiveWindowAbsoluteClientRect();
                rect.Offset(-completeRect.X, -completeRect.Y);
                Main.screenshotManager.AreaScreenShot(rect, ScreenshotManager.NativeMethods.GetActiveWindowTitle());
            }
            else
            {
                Rectangle rect = Main.screenshotManager.GetCompleteScreen();
                Main.screenshotManager.AreaScreenShot(rect);
            }
        }

        public void SelectAreaScreenShot()
        {
            if (pickArea != null) return;
            pickArea = new frmPickArea(this);
            pickArea.Show();
        }

        private void btnFullshot_Click(object sender, EventArgs e)
        {
            Rectangle rect = Main.screenshotManager.GetCompleteScreen();
            Main.screenshotManager.AreaScreenShot(rect);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Main.SetCredentials(tbUser.Text, tbPword.Text);
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.Visible) this.Hide();
            else { this.Show(); this.Activate(); }

            dropForm.Opacity = 0;
            dropForm.targetOpacity = 0;
            dropForm.Hide();
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (File.Exists("config.cfg")) this.Hide();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Main.Stop();
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            dropForm.ToggleVisibility();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }
    }
}
