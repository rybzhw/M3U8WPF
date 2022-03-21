using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M3U8WPF
{
    public class UniqueTaskParam
    {
        public string URL { get; set; }
        public string SavePath { get; set; }
        public string Filename { get; set; }
    }

    public class CommonTaskParam
    {
        public string Exe { get; set; }
        public string SavePath { get; set; }
        public string Headers { get; set; }
        public string BaseUrl { get; set; }
        public string MuxSetJson { get; set; }
        public string MaxThreads { get; set; }
        public string MinThreads { get; set; }
        public string Retry { get; set; }
        public string Timeout{ get; set; }
        public string StopSpeed { get; set; }
        public string MaxSpeed { get; set; }
        public string Key { get; set; }
        public string IV { get; set; }
        public string Proxy { get; set; }
        public bool Del { get; set; }
        public bool FastStart { get; set; }
        public bool BinaryMerge { get; set; }
        public bool ParserOnly { get; set; }
        public bool DisableDate { get; set; }
        public bool DisableMerge { get; set; }
        public bool DisableProxy { get; set; }
        public bool DisableCheck { get; set; }
        public bool AudioOnly { get; set; }
        public string RangeStart { get; set; }
        public string RangeEnd { get; set; }
    }

    public class SettingConfigHelper
    {
        private static CommonTaskParam commonTaskParam;

        public static CommonTaskParam GetCommonTaskParam()
        {
            return commonTaskParam;
        }
        public static void InitCommonTaskParam()
        {
            commonTaskParam = new CommonTaskParam();

            UpdateCommonTaskParam();
        }
        public static void UpdateCommonTaskParam()
        {
            //读取配置
            {
                commonTaskParam.Exe = AppConfigHelper.GetValue("Exe");
                commonTaskParam.SavePath = AppConfigHelper.GetValue("SavePath");
                commonTaskParam.Proxy = AppConfigHelper.GetValue("Proxy");
                commonTaskParam.Headers = AppConfigHelper.GetValue("Headers");

                if (AppConfigHelper.GetValue("Del") == "1")
                    commonTaskParam.Del = true;
                if (AppConfigHelper.GetValue("FastStart") == "1")
                    commonTaskParam.FastStart = true;
                if (AppConfigHelper.GetValue("BinaryMerge") == "1")
                    commonTaskParam.BinaryMerge = true;
                if (AppConfigHelper.GetValue("ParserOnly") == "1")
                    commonTaskParam.ParserOnly = true;
                if (AppConfigHelper.GetValue("DisableDate") == "1")
                    commonTaskParam.DisableDate = true;

                commonTaskParam.MaxThreads = AppConfigHelper.GetValue("MaxThreads");
                commonTaskParam.MinThreads = AppConfigHelper.GetValue("MinThreads");
                commonTaskParam.Retry = AppConfigHelper.GetValue("Retry");
                commonTaskParam.Timeout = AppConfigHelper.GetValue("Timeout");
                commonTaskParam.StopSpeed = AppConfigHelper.GetValue("StopSpeed");
                commonTaskParam.MaxSpeed = AppConfigHelper.GetValue("MaxSpeed");

                if (AppConfigHelper.GetValue("DisableMerge") == "1")
                    commonTaskParam.DisableMerge = true;
                if (AppConfigHelper.GetValue("DisableProxy") == "1")
                    commonTaskParam.DisableProxy = true;
                if (AppConfigHelper.GetValue("AudioOnly") == "1")
                    commonTaskParam.AudioOnly = true;
            }

            if (!File.Exists(commonTaskParam.Exe))//尝试寻找主程序
            {
                Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory);
                foreach (FileInfo fi in d.GetFiles().Reverse())
                {
                    if (fi.Extension.ToUpper() == ".exe".ToUpper() 
                        && fi.Name.StartsWith(AppConfigHelper.GetValue("DefaultExeKey")))
                    {
                        commonTaskParam.Exe = fi.Name;
                    }
                }
            }
        }
        public static string GetFinalTaskParam(UniqueTaskParam InUnique)
        {
            // Unique
            StringBuilder sb = new StringBuilder();
            sb.Append("\"" + InUnique.URL + "\" ");
            if (!string.IsNullOrEmpty(InUnique.SavePath))
            {
                if (InUnique.SavePath.Trim('\\').EndsWith(":")) //根目录
                {
                    sb.Append("--workDir \"" + InUnique.SavePath.Trim('\\') + "\\\\" + "\" ");
                }
                else
                {
                    sb.Append("--workDir \"" + InUnique.SavePath.Trim('\\') + "\" ");
                }
            }
            if (!string.IsNullOrEmpty(InUnique.Filename))
                sb.Append("--saveName \"" + InUnique.Filename + "\" ");

            // Common
            {
                if (!string.IsNullOrEmpty(commonTaskParam.Headers))
                    sb.Append("--headers \"" + commonTaskParam.Headers + "\" ");
                if (!string.IsNullOrEmpty(commonTaskParam.BaseUrl))
                    sb.Append("--baseUrl \"" + commonTaskParam.BaseUrl + "\" ");
                if (!string.IsNullOrEmpty(commonTaskParam.MuxSetJson))
                    sb.Append("--muxSetJson \"" + commonTaskParam.MuxSetJson + "\" ");
                if (commonTaskParam.MaxThreads != "32")
                    sb.Append("--maxThreads \"" + commonTaskParam.MaxThreads + "\" ");
                if (commonTaskParam.MinThreads != "16")
                    sb.Append("--minThreads \"" + commonTaskParam.MinThreads + "\" ");
                if (commonTaskParam.Retry != "15")
                    sb.Append("--retryCount \"" + commonTaskParam.Retry + "\" ");
                if (commonTaskParam.Timeout != "10")
                    sb.Append("--timeOut \"" + commonTaskParam.Timeout + "\" ");
                if (commonTaskParam.StopSpeed != "0")
                    sb.Append("--stopSpeed \"" + commonTaskParam.StopSpeed + "\" ");
                if (commonTaskParam.MaxSpeed != "0")
                    sb.Append("--maxSpeed \"" + commonTaskParam.MaxSpeed + "\" ");
                if (!string.IsNullOrEmpty(commonTaskParam.Key))
                {
                    if (File.Exists(commonTaskParam.Key))
                        sb.Append("--useKeyFile \"" + commonTaskParam.Key + "\" ");
                    else
                        sb.Append("--useKeyBase64 \"" + commonTaskParam.Key + "\" ");
                }
                if (!string.IsNullOrEmpty(commonTaskParam.IV))
                {
                    sb.Append("--useKeyIV \"" + commonTaskParam.IV + "\" ");
                }
                if (commonTaskParam.Proxy != "")
                {
                    sb.Append("--proxyAddress \"" + commonTaskParam.Proxy.Trim() + "\" ");
                }
                if (commonTaskParam.Del == true)
                    sb.Append("--enableDelAfterDone ");
                if (commonTaskParam.FastStart == true)
                    sb.Append("--enableMuxFastStart ");
                if (commonTaskParam.BinaryMerge == true)
                    sb.Append("--enableBinaryMerge ");
                if (commonTaskParam.ParserOnly == true)
                    sb.Append("--enableParseOnly ");
                if (commonTaskParam.DisableDate == true)
                    sb.Append("--disableDateInfo ");
                if (commonTaskParam.DisableMerge == true)
                    sb.Append("--noMerge ");
                if (commonTaskParam.DisableProxy == true)
                    sb.Append("--noProxy ");
                if (commonTaskParam.DisableCheck == true)
                    sb.Append("--disableIntegrityCheck ");
                if (commonTaskParam.AudioOnly == true)
                    sb.Append("--enableAudioOnly ");
                if ((!string.IsNullOrEmpty(commonTaskParam.Key) && commonTaskParam.RangeStart != "00:00:00") 
                    || (!string.IsNullOrEmpty(commonTaskParam.Key) && commonTaskParam.RangeEnd != "00:00:00"))
                {
                    sb.Append($"--downloadRange \"{commonTaskParam.RangeStart}-{commonTaskParam.RangeEnd}\"");
                }
            }

            return sb.ToString();
        }
    }
}
