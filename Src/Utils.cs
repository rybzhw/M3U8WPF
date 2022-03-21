using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8WPF
{
    public class AppLogHelper
    {
        public static void Log(string InFormat, params object[] InArgs)
        {
            Console.WriteLine(InFormat, InArgs);
            Trace.TraceInformation(InFormat, InArgs);
        }
        public static void Warnig(string InFormat, params object[] InArgs)
        {
            Console.WriteLine(InFormat, InArgs);
            Trace.TraceWarning(InFormat, InArgs);
        }
        public static void Error(string InFormat, params object[] InArgs)
        {
            Console.WriteLine(InFormat, InArgs);
            Trace.TraceError(InFormat, InArgs);
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
            catch (Exception)
            {
                AppLogHelper.Warnig("AppConfigHelper GetValue Exception Key={0}", InKey);
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
