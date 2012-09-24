﻿using System;
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

        public const string MAINURL = "https://push.doridian.de/";

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

        public void AddUpload(string customname, MemoryStream mstr)
        {
            int imax = customname.Length;
            char c;
            char[] cna = customname.ToCharArray(0, imax);
            for (int i = 0; i < imax; i++)
            {
                c = cna[i];
                if (c == '<' || c == '>')
                {
                    cna[i] = '_';
                }
            }
            customname = new String(cna);
            uploads.Enqueue(new UploadThreadInfo(customname, mstr, uploadProgress));
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

                NetworkCredential credentials = new NetworkCredential(Program.mainFrm.tbUser.Text, Program.mainFrm.tbPword.Text);

                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(MAINURL + "create?" + customname);
                hwr.Method = WebRequestMethods.Http.Put;
                hwr.Credentials = credentials;
                hwr.Proxy = null;
                Stream str = hwr.GetRequestStream();

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

                HttpWebResponse resp = (HttpWebResponse)hwr.GetResponse();
                StreamReader respreader = new StreamReader(resp.GetResponseStream());
                customname = MAINURL + respreader.ReadToEnd();
                respreader.Close();
                resp.Close();

                uploadProgress.SetProgress(1);
                uploadProgress.DoHide();
                uploadProgress.SetBackColor(Color.Green);

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

                    if (!cbtext.StartsWith(MAINURL))
                    {
                        cbtext = "";
                    }
                    else
                    {
                        cbtext += "\r\n";
                    }

                    try
                    {
                        Clipboard.SetText(cbtext + customname);
                    }
                    catch { }
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

            public UploadThreadInfo(string customname, MemoryStream mstr, frmProgress uploadProgress)
            {
                this.customname = customname;

                uploadProgress.AddLabel(this.customname);
                
                this.mstr = mstr;
            }
        }
    }
}
