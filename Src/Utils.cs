using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8WPF
{
    public class AppLogHelper
    {
        public static string LOGFILE = "M3U8WPF.log";

        public static void InitLog()
        {
            string file = LOGFILE;
            string now = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

            File.WriteAllText(file, now + "\r\n", Encoding.UTF8);
        }

        public static void WriteToFile(string InLevel, string InFormat, params object[] InArgs)
        {
            if (!File.Exists(LOGFILE))
                return;

            try
            {
                string file = LOGFILE;
                using (StreamWriter sw = File.AppendText(file))
                {
                    sw.WriteLine(format: DateTime.Now.ToString("HH:mm:ss.fff") + " / (" + InLevel + ") " + string.Format(InFormat, InArgs), Encoding.UTF8);
                }
            }
            catch (Exception)
            {

            }
        }

        public static void Log(string InFormat, params object[] InArgs)
        {
            Trace.TraceInformation(InFormat, InArgs);

            WriteToFile("Log", InFormat, InArgs);
        }
        public static void Warnig(string InFormat, params object[] InArgs)
        {
            Trace.TraceWarning(InFormat, InArgs);

            WriteToFile("Warnig", InFormat, InArgs);
        }
        public static void Error(string InFormat, params object[] InArgs)
        {
            Trace.TraceError(InFormat, InArgs);

            WriteToFile("Error", InFormat, InArgs);
        }
    }

    public class AppConfigHelper
    {
        public static string GetValue(string InKey)
        {
            try
            {
                return ConfigurationManager.AppSettings[InKey];
            }
            catch (Exception e)
            {
                AppLogHelper.Warnig("AppConfigHelper GetValue Exception Key={0} e={1}", InKey, e.ToString());
            }
            return "";
        }

        public static void SetValue(string InKey, string InValue)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings.AllKeys.Contains(InKey))
                {
                    config.AppSettings.Settings[InKey].Value = InValue;
                    AppLogHelper.Log("AppConfigHelper SetValue Update Key={0} Value={1}", InKey, InValue);
                }
                else
                {
                    config.AppSettings.Settings.Add(InKey, InValue);
                    AppLogHelper.Log("AppConfigHelper SetValue Add Key={0} Value={1}", InKey, InValue);
                }

                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                AppLogHelper.Warnig("AppConfigHelper SetValue Exception Key={0} Value={1}", InKey, InValue);
            }
        }
    }
}
