using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Threading;
using FoxCavesAPI;
using System.IO.Pipes;

namespace FoxScreen
{
    public class UploadOrganizer : IDisposable
    {
        readonly Uploader uploader;
        readonly frmProgress uploadProgress;
        readonly NamedPipeServerStream pipeServer = new NamedPipeServerStream("foxScreenUploadPipe");

        bool isRunning;
        Thread pipeReaderThread;

        public UploadOrganizer()
        {
            uploadProgress = new frmProgress();
            uploadProgress.Show();
            uploadProgress.Hide();

            uploader = new Uploader();

            uploader.UploadStarted += uploader_UploadStarted;
            uploader.UploadFinished += uploader_UploadFinished;
            uploader.UploadProgress += uploader_UploadProgress;

            isRunning = true;

            pipeReaderThread = new Thread(new ThreadStart(PipeReaderThread));
            pipeReaderThread.Start();
        }

        private void PipeReaderThread()
        {
            StreamReader reader = new StreamReader(pipeServer);

            while (isRunning)
            {
                try
                {
                    pipeServer.WaitForConnection();
                    string filename = reader.ReadLine();
                    AddFileUpload(filename);
                    pipeServer.Disconnect();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "foxScreen: ERROR");
                }
            }
        }

        public void Dispose()
        {
            uploader.Dispose();
            isRunning = false;
            try
            {
                pipeReaderThread.Abort();
            }
            catch { }
        }

        public void SetCredentials(string username, string password)
        {
            uploader.SetCredentials(username, password);
        }

        public void AddFileUpload(string file)
        {
            if ((!File.Exists(file)) || Directory.Exists(file))
                return;

            int fPathPos = file.LastIndexOf('/');
            int fPathPos2 = file.LastIndexOf('\\');
            if (fPathPos2 > fPathPos) fPathPos = fPathPos2;
            string filename = file.Substring(fPathPos + 1);

            MemoryStream mstr = new MemoryStream(File.ReadAllBytes(file));
            AddUpload(filename, mstr);
        }

        public void AddUpload(string filename, MemoryStream mstr)
        {
            Uploader.UploadInfo uploadInfo = uploader.QueueAsync(filename, mstr);
            uploadProgress.AddLabel(uploadInfo.filename);
        }

        void uploader_UploadProgress(Uploader.UploadInfo uploadInfo, double progress)
        {
            uploadProgress.SetProgress((float)progress);
        }

        void uploader_UploadStarted(Uploader.UploadInfo uploadInfo)
        {
            uploadProgress.RemoveLastLabel();
            uploadProgress.SetStatus(uploadInfo.filename);
            uploadProgress.SetProgress(0);
            uploadProgress.SetBackColor(Color.Yellow);
            uploadProgress.DoShow();
        }

        void uploader_UploadFinished(Uploader.UploadInfo uploadInfo, bool success, string error_or_link)
        {
            uploadProgress.SetProgress(1);

            if (success)
            {
                Main.mainFrm.Invoke(new MethodInvoker(delegate()
                {
                    Clipboard.SetText(error_or_link);
                }));

                uploadProgress.SetBackColor(Color.Green);
            }
            else
            {
                MessageBox.Show(error_or_link, "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);

                uploadProgress.SetBackColor(Color.Red);
            }

            uploadProgress.DoHide();
        }
    }
}
