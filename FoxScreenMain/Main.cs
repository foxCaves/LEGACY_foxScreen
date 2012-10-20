using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace FoxScreen
{
    public static class Main
    {
        public static UploadOrganizer uploadOrganizer;
        public static ScreenshotManager screenshotManager;

        internal static string username;
        internal static string password;

        internal static Mutex mutex;

        public static void Launch()
        {
            if (!LaunchInt(null))
            {
                MessageBox.Show("foxScreen is already running!", "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void LaunchWithUpload(string filename)
        {
            if (LaunchInt(filename))
            {
                return;
            }

            try
            {
                NamedPipeClientStream client = new NamedPipeClientStream("foxScreenUploadPipe");
                client.Connect(1);
                StreamWriter writer = new StreamWriter(client);
                writer.WriteLine(filename);
                writer.Close();
                client.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "foxScreen: ERROR");
            }

            Stop();
        }

        private static bool LaunchInt(string filename)
        {
            string mutexId = "Global\\foxScreenMutex";

            mutex = new Mutex(false, mutexId);

            MutexAccessRule allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            MutexSecurity securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);
            mutex.SetAccessControl(securitySettings);

            bool hasHandle = false;
            try
            {
                try
                {
                    hasHandle = mutex.WaitOne(100, false);
                    if (hasHandle == false)
                    {
                        mutex = null;
                        return false;
                    }
                }
                catch (AbandonedMutexException)
                {
                    hasHandle = true;
                }

                uploadOrganizer = new UploadOrganizer();
                screenshotManager = new ScreenshotManager(uploadOrganizer);

                if (filename != null)
                {
                    LoadCredentials();
                    uploadOrganizer.AddFileUpload(filename);
                }

                mainFrm = new frmMain();
                Application.Run(mainFrm);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                if (!hasHandle)
                {
                    mutex = null;
                }
                Stop();
            }

            return true;
        }

        public static void Stop()
        {
            if(uploadOrganizer != null)
                uploadOrganizer.Dispose();

            if (mutex != null)
                mutex.ReleaseMutex();

            Application.Exit();

            Process.GetCurrentProcess().Kill();
        }

        public static void SetCredentials(string username, string password)
        {
            File.WriteAllText("config.cfg", username + Environment.NewLine + password);
            uploadOrganizer.SetCredentials(username, password);
        }

        public static void LoadCredentials()
        {
            try
            {
                string[] lines = File.ReadAllLines("config.cfg");
                username = lines[0];
                password = lines[1];
            }
            catch { }

            uploadOrganizer.SetCredentials(username, password);
        }

        public static frmMain mainFrm;
    }
}
