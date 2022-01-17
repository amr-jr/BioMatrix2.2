using System;
using System.Text;
using System.IO;
using System.Reflection;

namespace BioMetrixCore
{
    class LogClass
    {
        private string m_exePath = string.Empty;
        private string logFileName = "";
        private string logFilePath = "";
        private static string LastDateOfLogFile = "";
        private static bool IsDailyLog;

        public LogClass() : this("log", true)
        { }

        public LogClass(string LogFileName = "log", bool isDailyLog = true)
        {

            logFileName = LogFileName;

            IsDailyLog = isDailyLog;

            if (IsDailyLog)
            {
                LastDateOfLogFile = DateTime.Now.ToString("dd_MM_yyyy");
                logFileName = logFileName + "__" + LastDateOfLogFile;
            }

            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!logFileName.EndsWith(".txt"))
                logFileName = logFileName + ".txt";

            if (!File.Exists(m_exePath + "\\" + logFileName))
                File.Create(m_exePath + "\\" + logFileName).Dispose();


            logFilePath = m_exePath + "\\" + logFileName;
        }

        public LogClass(string logMessage, string LogFileName = "log", bool isDailyLog = true) : this(LogFileName, isDailyLog)
        {
            LogWrite(logMessage);
        }



        public void LogWrite(string logMessage)
        {
            try
            {
                AppendLog(logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AppendLog(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  : {0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AppendLog(string logMessage)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("\r\nLog Entry : ");
                sb.AppendLine(($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}"));
                sb.AppendLine("  :");
                sb.AppendLine("  : " + ($"{logMessage}"));
                sb.AppendLine("-------------------------------");

                File.AppendAllText(logFilePath, sb.ToString());

                sb.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void LogRead()
        {
            try
            {
                using (StreamReader r = File.OpenText(logFilePath))
                {
                    DumpLog(r);
                    r.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\nERROR!! On Read : \n> MSG : \n " + ex.Message + " \n\n> STACKTRACE : \n " + ex.StackTrace);
            }
        }

        private void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

        private static LogClass log = null;
        public static void Write(string logMessage)
        {
            if (IsDailyLog && !LastDateOfLogFile.Equals(DateTime.Now.ToString("dd_MM_yyyy")))
            {
                log = null;
            }

            if (log == null)
            {
                log = new LogClass();
            }

            log.LogWrite(logMessage);
        }

        public static void ReadAndPrintOnConsole()
        {
            if (log == null)
            {
                log = new LogClass();
            }

            log.LogRead();
        }

    }


}

