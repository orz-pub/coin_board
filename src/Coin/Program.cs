using System;
using System.Windows.Forms;
using System.IO;
using log4net;

namespace Coin
{
    enum LogType
    {
        Debug,
        Error,
        Fatal,
        Info,
        Warn,
    }

    static class Program
    {
        public static ILog Logger = null;

        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                //Set Logger
                String logConfigName = "./res/LogConfig.xml";
                if (File.Exists(logConfigName) == true)
                {
                    log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(logConfigName));
                    Logger = log4net.LogManager.GetLogger("Logger");
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMain());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        public static void Log(LogType type, String message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
            if (Logger == null) return;

            switch (type)
            {
                case LogType.Debug: Logger.Debug(message); break;
                case LogType.Error: Logger.Error(message); break;
                case LogType.Fatal: Logger.Fatal(message); break;
                case LogType.Info: Logger.Info(message); break;
                case LogType.Warn: Logger.Warn(message); break;
            }
        }
    }
}
