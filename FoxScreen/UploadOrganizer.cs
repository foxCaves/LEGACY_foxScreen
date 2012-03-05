using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;

namespace FoxScreen
{
    public class UploadOrganizer
    {
        frmProgress uploadProgress;
        Thread uploadThread;
        Thread uploadCheckerThread;

        Queue<UploadThreadInfo> uploads = new Queue<UploadThreadInfo>();

        public UploadOrganizer()
        {
            uploadProgress = new frmProgress();
            uploadProgress.Show();
            uploadProgress.Hide();

            uploadCheckerThread = new Thread(new ThreadStart(UploadCheckerThread));
            uploadCheckerThread.Start();
        }

        ~UploadOrganizer()
        {
            this.Stop();
        }

        public void Stop()
        {
            try
            {
                uploadCheckerThread.Abort();
            }
            catch { }

            try
            {
                uploadThread.Abort();
            }
            catch { }
        }

        public void AddUpload(string customname, string extension, MemoryStream mstr)
        {
            uploads.Enqueue(new UploadThreadInfo(customname, extension, mstr, uploadProgress));
        }

        private void UploadCheckerThread()
        {
            while (true)
            {
                do {
                    Thread.Sleep(100);
                } while(uploadThread != null && uploadThread.IsAlive);

                if (uploads.Count > 0)
                {
                    uploadThread = new Thread(new ParameterizedThreadStart(UploadThread));
                    uploadThread.Start(uploads.Dequeue());
                }
            }
        }

        private void UploadThread(object obj)
        {
            UploadThreadInfo info = (UploadThreadInfo)obj;
            string customname = info.customname;
            MemoryStream mstr = info.mstr;

            try
            {
                mstr.Seek(0, SeekOrigin.Begin);
            }
            catch { }

            try
            {
                uploadProgress.RemoveLastLabel();
                uploadProgress.SetStatus(customname);
                uploadProgress.SetProgress(0);
                uploadProgress.SetBackColor(Color.Yellow);
                uploadProgress.DoShow();

                FtpWebRequest ftpr = (FtpWebRequest)FtpWebRequest.Create(Program.mainFrm.tbHost.Text + "/" + customname);
                ftpr.Method = WebRequestMethods.Ftp.UploadFile;
                ftpr.UsePassive = true;
                ftpr.UseBinary = true;
                ftpr.Credentials = new NetworkCredential(Program.mainFrm.tbUser.Text, Program.mainFrm.tbPword.Text);
                Stream str = ftpr.GetRequestStream();

                byte[] buffer = new byte[256];
                int readb;
                while (mstr.CanRead)
                {
                    readb = (int)(mstr.Length - mstr.Position);
                    if (readb > 256) readb = 256;
                    readb = mstr.Read(buffer, 0, readb);
                    if (readb <= 0) break;
                    str.Write(buffer, 0, readb);

                    uploadProgress.SetProgress(((float)mstr.Position) / ((float)mstr.Length));
                }
                str.Close();
                mstr.Close();
                FtpWebResponse resp = (FtpWebResponse)ftpr.GetResponse();
                resp.Close();

                uploadProgress.SetProgress(1);
                uploadProgress.DoHide();
                uploadProgress.SetBackColor(Color.Green);

                customname = Program.mainFrm.tbLB.Text + customname;
                Program.mainFrm.Invoke(new MethodInvoker(delegate()
                {
                    string cbtext = "";
                    try {
                        cbtext = Clipboard.GetText();
                        if (cbtext == null)
                        {
                            cbtext = "";
                        }
                    } catch { }

                    if (!cbtext.StartsWith(Program.mainFrm.tbLB.Text))
                    {
                        cbtext = "";
                    }
                    else
                    {
                        cbtext += "\r\n";
                    }
                    Clipboard.SetText(cbtext + customname);
                }));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "ERROR!");
            }
        }

        private static string FixTwoChar(int num)
        {
            if (num < 10) return "0" + num.ToString();
            return num.ToString();
        }

        internal class UploadThreadInfo
        {
            public readonly string customname;
            public readonly MemoryStream mstr;

            public UploadThreadInfo(string customname, string extension, MemoryStream mstr, frmProgress uploadProgress)
            {
                this.customname = customname + "_" + FixTwoChar(DateTime.Now.Day) + "-" + FixTwoChar(DateTime.Now.Month) + "-" + DateTime.Now.Year + "_" + FixTwoChar(DateTime.Now.Hour) + "-" + FixTwoChar(DateTime.Now.Minute) + "-" + FixTwoChar(DateTime.Now.Second) + "." + extension;

                uploadProgress.AddLabel(this.customname);
                
                this.mstr = mstr;
            }
        }
    }
}
