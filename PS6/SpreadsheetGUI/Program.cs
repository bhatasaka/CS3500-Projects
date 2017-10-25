using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Keeps track of how many top-level forms are running
    /// </summary>
    class PS6ApplicationContext : ApplicationContext
    {
        // Number of open forms
        private int formCount = 0;

        // Singleton ApplicationContext
        private static PS6ApplicationContext appContext;

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private PS6ApplicationContext()
        {
        }

        /// <summary>
        /// Returns the one DemoApplicationContext.
        /// </summary>
        public static PS6ApplicationContext getAppContext()
        {
            if (appContext == null)
            {
                appContext = new PS6ApplicationContext();
            }
            return appContext;
        }

        /// <summary>
        /// Runs the form
        /// </summary>
        public void RunForm(Form form)
        {
            // One more form is running
            formCount++;

            // When this form closes, we want to find out
            form.FormClosed += (o, e) => { if (--formCount <= 0) ExitThread(); };

            // Run the form
            form.Show();
        }

    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //args[1] is the filename/path
            if(args.Count() == 2)
            {
                //load the app with the file
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PS6ApplicationContext appContext = PS6ApplicationContext.getAppContext();
            appContext.RunForm(new PS6());
            Application.Run(appContext);
        }
    }
}
