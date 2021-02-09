using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;

namespace ClassNotify {
    class Program {

        #region Hide Console

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr itp, int show);

        const int CONSOLE_HIDE = 0;
        const int CONSOLE_SHOW = 1;

        #endregion

        public Form mainForm = new Form();
        public bool isRunning = true;

        private string[,] classes;
        private TimeSpan[] times = new TimeSpan[4] { 
            new TimeSpan(19, 0, 0),
            new TimeSpan(19, 50, 0),
            new TimeSpan(20, 50, 0),
            new TimeSpan(21, 40, 0)
        };

        public Program() {
            Application.EnableVisualStyles();
            mainForm.WindowState = FormWindowState.Minimized;
            mainForm.ShowInTaskbar = false;
            NotifyIcon tray = new NotifyIcon();
            tray.Icon = SystemIcons.Question;
            tray.Visible = true;
            tray.Text = "vai se fuder";

            LoadClasses();
            Thread clock = new Thread(ClockTracker);
            clock.Start();
            Application.Run(mainForm);
        }

        #region Notification Systems

        public void Notify(string title, string message) {
            NotifyIcon notify = new NotifyIcon();
            notify.Icon = SystemIcons.Exclamation;
            notify.Visible = true;
            notify.ShowBalloonTip(10000, title, message, ToolTipIcon.Info);
        }

        public void ClockTracker() {
            Console.WriteLine("Clock started");
            while (isRunning) {
                Thread.Sleep(10000);
                for (int i = 0; i < times.Length; i++) {
                    TimeSpan tempTimeLimiter = new TimeSpan(times[i].Hours, times[i].Minutes, times[i].Seconds + 30);
                    if (DateTime.Now.TimeOfDay >= times[i] && DateTime.Now.TimeOfDay < tempTimeLimiter) {
                        int weekDay = (int)DateTime.Now.DayOfWeek - 1; //minus one because we need monday to be zero, since sunday is not used by default, come no make it easy fuck you
                        string title = "Aula das " + times[i];
                        string message = "Aula de " + classes[weekDay, i];
                        Notify(title, message);
                        Thread.Sleep(1000 * 60);
                        return;
                    }
                }
            }
        }

        #endregion

        #region Xml Handler
        public void LoadClasses() {
            XmlDocument classes = new XmlDocument();
            try {
                classes.Load("../../data/aulas.xml");
                XmlNodeList days = classes.SelectNodes("/semana/dia");
                PopulateClass(days);
            } catch (System.IO.FileNotFoundException) {
                Console.WriteLine("File not found");
            }
        }

        public void PopulateClass(XmlNodeList days) {
            int dayLength = days.Count;
            int classLength = 0;

            for (int i = 0; i < dayLength; i++) {
                XmlNodeList tempClass = days[i].ChildNodes;
                if (tempClass.Count > classLength) {
                    classLength = tempClass.Count;
                }
            }

            classes = new string[dayLength, classLength];
            for (int d = 0; d < dayLength; d++) {
                XmlNodeList dayClasses = days[d].ChildNodes;
                for (int c = 0; c < classLength; c++) {
                    classes[d, c] = dayClasses[c].InnerText;
                }
            }
        }
        #endregion

        static void Main(string[] args) {
            var handle = GetConsoleWindow();
            ShowWindow(handle, CONSOLE_HIDE);
            new Program();
        }
    }
}
