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
        public readonly UploadOrganizer uploadOrganizer = new UploadOrganizer();
        public readonly ScreenshotManager screenshotManager;

        public frmMain()
        {
            screenshotManager = new ScreenshotManager(uploadOrganizer);

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
                tbUser.Text = lines[0];
                tbPword.Text = lines[1];
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
                Rectangle rect = ScreenshotManager.NativeMethods.GetActiveWindowAbsoluteClientRect();
                screenshotManager.AreaScreenShot(rect, ScreenshotManager.NativeMethods.GetActiveWindowTitle());
            }
            else
            {
                Rectangle rect = screenshotManager.GetCompleteScreen();
                screenshotManager.AreaScreenShot(rect);
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
            Rectangle rect = screenshotManager.GetCompleteScreen();
            screenshotManager.AreaScreenShot(rect); ;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            File.WriteAllText("config.cfg", tbUser.Text + Environment.NewLine + tbPword.Text);
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
            uploadOrganizer.Stop();
            Application.Exit();
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
