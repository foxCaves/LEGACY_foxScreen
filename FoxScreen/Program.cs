using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FoxScreen
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string mutexId = "Global\\foxScreenMutex";

            using (var mutex = new Mutex(false, mutexId))
            {
                var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                mutex.SetAccessControl(securitySettings);

                var hasHandle = false;
                try
                {
                    try
                    {
                        hasHandle = mutex.WaitOne(1000, false);
                        if (hasHandle == false)
                        {
                            MessageBox.Show("foxScreen is already running!", "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }   
                    }
                    catch (AbandonedMutexException)
                    {
                        hasHandle = true;
                    }

                    ActualMain();
                }
                finally
                {
                    if (hasHandle)
                        mutex.ReleaseMutex();
                }
            }
        }

        private static void ActualMain()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            string launchDir = Environment.CurrentDirectory;

            Environment.CurrentDirectory = GetAppDir();
            SelfUpdate();

            byte[] assembly;
            if (Debugger.IsAttached)
            {
                assembly = File.ReadAllBytes(launchDir + "/foxScreenMain.dll");
            }
            else
            {
                assembly = File.ReadAllBytes("foxScreenMain.dll");
            }

            Assembly launcher = Assembly.Load(assembly);
            Type mainClass = launcher.GetType("FoxScreen.Main");
            MethodInfo method = mainClass.GetMethod("Launch", BindingFlags.Static | BindingFlags.Public);
            method.Invoke(null, new object[] { });
        }

        public static void SelfUpdate()
        {
            if (Debugger.IsAttached) return;
            FormSelfUpdater frmSU = new FormSelfUpdater();
            frmSU.ShowDialog();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.ToString(), "Unhandled Thread Exception");
            // here you can log the exception ...
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception).ToString(), "Unhandled UI Exception");
            // here you can log the exception ...
        }

        #region foxScreen Folder Discovery
        private static string foxscreenDir = null;

        public static string GetAppDir()
        {
            if (foxscreenDir == null)
            {
                foxscreenDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/.foxScreen/";
                if (!Directory.Exists(foxscreenDir))
                {
                    Directory.CreateDirectory(foxscreenDir);
                }
                if (!Directory.Exists(foxscreenDir))
                {
                    MessageBox.Show("Could not create .foxScreen folder!", "foxScreen", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            return foxscreenDir;
        }
        #endregion
    }
}
