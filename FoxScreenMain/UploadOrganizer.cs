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

namespace FoxScreen
{
    public class UploadOrganizer : IDisposable
    {
        readonly Uploader uploader;
        readonly frmProgress uploadProgress;

        public UploadOrganizer()
        {
            uploadProgress = new frmProgress();
            uploadProgress.Show();
            uploadProgress.Hide();

            uploader = new Uploader();

            uploader.UploadStarted += uploader_UploadStarted;
            uploader.UploadFinished += uploader_UploadFinished;
            uploader.UploadProgress += uploader_UploadProgress;
        }

        public void Dispose()
        {
            uploader.Dispose();
        }

        public void SetCredentials(string username, string password)
        {
            uploader.SetCredentials(username, password);
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
