using System;
using System.Collections.Generic;
using System.Linq;
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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainFrm = new frmMain();
            Application.Run(mainFrm);
        }

        public static frmMain mainFrm;
    }
}
