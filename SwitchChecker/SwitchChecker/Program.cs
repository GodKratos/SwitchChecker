using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SwitchChecker
{
    static class Program
    {
        public static string ConfigurationFileLocation = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"SwitchChecker.config");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
