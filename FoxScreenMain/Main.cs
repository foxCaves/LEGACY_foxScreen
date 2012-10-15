using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FoxScreen
{
    public static class Main
    {
        public static void Launch()
        {
            mainFrm = new frmMain();
            Application.Run(mainFrm);
        }

        public static frmMain mainFrm;
    }
}
