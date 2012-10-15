using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FoxScreen
{
    public partial class FormSelfUpdater : Form
    {
        private static readonly string UPDATEURL_BASE = "https://d3rith5u07eivj.cloudfront.net/static/dls/";

        public FormSelfUpdater()
        {
            InitializeComponent();
        }

        private void FormSelfUpdater_Load(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(UpdateThread)).Start();
        }

        private void InvokeClose()
        {
            this.Invoke(new MethodInvoker(Close));
        }

        private void SetStatus(string status)
        {
            lbStatus.Invoke(new MethodInvoker(delegate() {
                lbStatus.Text = status;
            }));
        }

        private void UpdateThread()
        {
            if (UpdateSelf()) return;
            UpdateDLL();
            this.InvokeClose();
        }

        private bool UpdateDLL()
        {
            return UpdateFile("foxScreenMain.dll", "foxScreenMain.dll");
        }

        private bool UpdateSelf()
        {
            if (Debugger.IsAttached) return false;
            string myFile = Process.GetCurrentProcess().MainModule.FileName;
            if (UpdateFile(myFile, "foxScreen.exe"))
            {
                Process.Start(myFile);
                Process.GetCurrentProcess().Kill();
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool UpdateFile(string myFile, string internal_filename)
        {
            string oldFile = myFile + ".old";

            while (File.Exists(oldFile))
            {
                try
                {
                    File.Delete(oldFile);
                }
                catch
                {
                    oldFile += ".old";
                }
            }

            string tstamp = DateTime.Now.ToFileTimeUtc().ToString();

            string correctMD5 = HTTP.GET(UPDATEURL_BASE + internal_filename + ".md5?v=" + tstamp).Trim();
            string myMD5 = DownloaderHash.HashFile(myFile);
            if (myMD5 == correctMD5)
            {
                return false;
            }

            SetStatus("Self-Updating " + internal_filename + " now!");

            if (File.Exists(myFile))
            {
                File.Move(myFile, oldFile);
            }

            DownloaderHash.DownloadFileTo(myFile, UPDATEURL_BASE + internal_filename + "?v=" + tstamp, correctMD5);

            if (!File.Exists(myFile))
            {
                if (File.Exists(oldFile))
                {
                    MessageBox.Show("Self-Updating failed! Using old " + internal_filename + "!", "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    File.Move(oldFile, myFile);
                }
                else
                {
                    MessageBox.Show("Self-Updating failed but no fallback found :c", "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Process.GetCurrentProcess().Kill();
                }
                return false;
            }

            if (File.Exists(oldFile))
            {
                try
                {
                    File.Delete(oldFile);
                }
                catch
                {  
                    //If its not locked, just get rid of it :3
                }
            }
            return true;
        }
    }
}
