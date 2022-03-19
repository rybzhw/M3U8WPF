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
        public string RetryCount { get; set; }
        public string TimeOut{ get; set; }
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

    public enum TaskStateEnum
    {
        TSE_None,
        TSE_Prepare,
        TSE_Downloading,
        TSE_Done
    }

    public class TaskState
    {
        // 必须有get 和set，否则xaml绑定数据获得不了值
        public string Filename { get; set; }
        public string VideoLength { get; set; }
        public string Chunk { get; set; }
        public string FileSize { get; set; }
        public string Progress { get; set; }
        public string Speed { get; set; }
        public string LeftTime { get; set; }
        public string StatePrompt { get; set; }
        public string StateDetail { get; set; }

        public string SavePath;
        public Process CmdProcess;
        public TaskStateEnum taskStateEnum { get; set; }

        public TaskState()
        {
            VideoLength = "-";
            Chunk = "-/-";
            FileSize = "-/-";
            Progress = "-";
            Speed = "-";
            LeftTime = "-";
            StatePrompt = "";
            StateDetail = "";
            taskStateEnum = TaskStateEnum.TSE_None;
        }
    }

    public class SettingConfigHelper
    {
        public static CommonTaskParam commonTaskParam;

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
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            //读取配置
            if (File.Exists("config.txt"))
            {
                string config = File.ReadAllText("config.txt");

                commonTaskParam.Exe = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("程序路径", config)));
                commonTaskParam.SavePath = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("保存路径", config)));
                try
                {
                    commonTaskParam.Proxy = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("代理", config)));
                }
                catch (Exception) {; }
                try
                {
                    commonTaskParam.Headers = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("请求头", config)));
                }
                catch (Exception) {; }
                if (FindCookie("删除临时文件", config) == "1")
                    commonTaskParam.Del = true;
                if (FindCookie("MP4混流边下边看", config) == "1")
                    commonTaskParam.FastStart = true;
                if (FindCookie("二进制合并", config) == "1")
                    commonTaskParam.BinaryMerge = true;
                if (FindCookie("仅解析模式", config) == "1")
                    commonTaskParam.ParserOnly = true;
                if (FindCookie("不写入日期", config) == "1")
                    commonTaskParam.DisableDate = true;
                commonTaskParam.MaxThreads = FindCookie("最大线程", config);
                commonTaskParam.MinThreads = FindCookie("最小线程", config);
                commonTaskParam.RetryCount = FindCookie("重试次数", config);
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("超时秒数", config)))
                        commonTaskParam.TimeOut = FindCookie("超时秒数", config);
                }
                catch (Exception) {; }
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("停止速度", config)))
                        commonTaskParam.StopSpeed = FindCookie("停止速度", config);
                }
                catch (Exception) {; }
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("最大速度", config)))
                        commonTaskParam.MaxSpeed = FindCookie("最大速度", config);
                }
                catch (Exception) {; }
                try
                {
                    if (FindCookie("不合并", config) == "1")
                        commonTaskParam.DisableMerge = true;
                    if (FindCookie("不使用系统代理", config) == "1")
                        commonTaskParam.DisableProxy = true;
                }
                catch (Exception) {; }
                try
                {
                    if (FindCookie("仅合并音频", config) == "1")
                        commonTaskParam.AudioOnly = true;
                }
                catch (Exception) {; }
            }

            if (!File.Exists(commonTaskParam.Exe))//尝试寻找主程序
            {
                DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory);
                foreach (FileInfo fi in d.GetFiles().Reverse())
                {
                    if (fi.Extension.ToUpper() == ".exe".ToUpper() && fi.Name.StartsWith("N_m3u8DL-CLI_"))
                    {
                        commonTaskParam.Exe = fi.Name;
                    }
                }
            }
        }
        //寻找cookie字符串中的value
        public static string FindCookie(string key, string cookie)
        {
            string[] values = cookie.Split(';');
            string value = "";
            foreach (var v in values)
            {
                if (v.Trim().StartsWith(key + "="))
                    value = v.Remove(0, v.IndexOf('=') + 1).Trim();
            }
            return value;
        }
    }
}
