using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace ClassNotify {
    class Program {

        #region Hide Console

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr itp, int show);

        public const int CONSOLE_HIDE = 0;
        public const int CONSOLE_SHOW = 1;

        #endregion

        public bool isRunning = true;
        public IntPtr handle = GetConsoleWindow();
        public Form mainForm = new Form();
        //this just represents where the hell the windows looks up to the startup programs
        public RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        NotifyIcon tray;

        private string[,] classes;
        private TimeSpan[] times = new TimeSpan[4] { 
            new TimeSpan(19, 0, 0),
            new TimeSpan(19, 50, 0),
            new TimeSpan(20, 50, 0),
            new TimeSpan(21, 40, 0)
        };

        public Program() {
            Console.WriteLine("Welcome to the ClassNotify v1");
            if (registryKey.GetValue("ClassNotify") == null) {
                registryKey.SetValue("ClassNotify", Application.ExecutablePath);
            }
            Thread.Sleep(2000);
            ShowWindow(handle, CONSOLE_HIDE);
            Application.EnableVisualStyles();
            mainForm.WindowState = FormWindowState.Minimized;
            mainForm.ShowInTaskbar = false;
            tray = new NotifyIcon();
            tray.Icon = SystemIcons.Question;
            tray.Visible = true;
            tray.Text = "ClassNotify v1";

            LoadClasses();
            Thread clock = new Thread(ClockTracker);
            clock.Start();
            Notify("Bem vindo", "Olá vádia, já iniciei nessa merda, cê não larga desse computador puta que pariu");
            Application.Run(mainForm);
        }

        #region Notification Systems

        public void Notify(string title, string message) {
            tray.Icon = SystemIcons.Exclamation;
            tray.Visible = true;
            tray.ShowBalloonTip(10000, title, message, ToolTipIcon.Info);
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
            new Program();
        }
    }
}
