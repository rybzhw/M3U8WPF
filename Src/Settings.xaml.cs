using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace M3U8WPF
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {

        }

        private void Button_SelectDir_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_MuxJson_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void TextBox_MuxJson_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void TextBox_MuxJson_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void TextBox_Key_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void TextBox_Key_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {

        }

        private void TextBox_Key_PreviewDrop(object sender, System.Windows.DragEventArgs e)
        {

        }


        //寻找cookie字符串中的value
        public string FindCookie(string key, string cookie)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //string  to  base64
            Encoding encode = Encoding.UTF8;
            byte[] bytedata = encode.GetBytes(TextBox_EXE.Text);
            string exePath = Convert.ToBase64String(bytedata, 0, bytedata.Length);
            bytedata = encode.GetBytes(TextBox_WorkDir.Text);
            string saveDir = Convert.ToBase64String(bytedata, 0, bytedata.Length);
            bytedata = encode.GetBytes(TextBox_Proxy.Text);
            string proxy = Convert.ToBase64String(bytedata, 0, bytedata.Length);
            bytedata = encode.GetBytes(TextBox_Headers.Text);
            string headers = Convert.ToBase64String(bytedata, 0, bytedata.Length);

            string config = "程序路径=" + exePath
                + ";保存路径=" + saveDir
                + ";代理=" + proxy
                + ";请求头=" + headers
                + ";删除临时文件=" + (CheckBox_Del.IsChecked == true ? "1" : "0")
                + ";MP4混流边下边看=" + (CheckBox_FastStart.IsChecked == true ? "1" : "0")
                + ";二进制合并=" + (CheckBox_BinaryMerge.IsChecked == true ? "1" : "0")
                + ";仅解析模式=" + (CheckBox_ParserOnly.IsChecked == true ? "1" : "0")
                + ";不写入日期=" + (CheckBox_DisableDate.IsChecked == true ? "1" : "0")
                + ";最大线程=" + TextBox_Max.Text
                + ";最小线程=" + TextBox_Min.Text
                + ";重试次数=" + TextBox_Retry.Text
                + ";超时秒数=" + TextBox_Timeout.Text
                + ";停止速度=" + TextBox_StopSpeed.Text
                + ";最大速度=" + TextBox_MaxSpeed.Text
                + ";不合并=" + (CheckBox_DisableMerge.IsChecked == true ? "1" : "0")
                + ";不使用系统代理=" + (CheckBox_DisableProxy.IsChecked == true ? "1" : "0")
                + ";仅合并音频=" + (CheckBox_AudioOnly.IsChecked == true ? "1" : "0");
            File.WriteAllText("config.txt", config);

            SettingConfigHelper.UpdateCommonTaskParam();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            //读取配置
            if (File.Exists("config.txt"))
            {
                string config = File.ReadAllText("config.txt");

                TextBox_EXE.Text = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("程序路径", config)));
                TextBox_WorkDir.Text = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("保存路径", config)));
                try
                {
                    TextBox_Proxy.Text = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("代理", config)));
                }
                catch (Exception) {; }
                try
                {
                    TextBox_Headers.Text = Encoding.UTF8.GetString(Convert.FromBase64String(FindCookie("请求头", config)));
                }
                catch (Exception) {; }
                if (FindCookie("删除临时文件", config) == "1")
                    CheckBox_Del.IsChecked = true;
                if (FindCookie("MP4混流边下边看", config) == "1")
                    CheckBox_FastStart.IsChecked = true;
                if (FindCookie("二进制合并", config) == "1")
                    CheckBox_BinaryMerge.IsChecked = true;
                if (FindCookie("仅解析模式", config) == "1")
                    CheckBox_ParserOnly.IsChecked = true;
                if (FindCookie("不写入日期", config) == "1")
                    CheckBox_DisableDate.IsChecked = true;
                TextBox_Max.Text = FindCookie("最大线程", config);
                TextBox_Min.Text = FindCookie("最小线程", config);
                TextBox_Retry.Text = FindCookie("重试次数", config);
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("超时秒数", config)))
                        TextBox_Timeout.Text = FindCookie("超时秒数", config);
                }
                catch (Exception) {; }
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("停止速度", config)))
                        TextBox_StopSpeed.Text = FindCookie("停止速度", config);
                }
                catch (Exception) {; }
                try
                {
                    if (!string.IsNullOrEmpty(FindCookie("最大速度", config)))
                        TextBox_MaxSpeed.Text = FindCookie("最大速度", config);
                }
                catch (Exception) {; }
                try
                {
                    if (FindCookie("不合并", config) == "1")
                        CheckBox_DisableMerge.IsChecked = true;
                    if (FindCookie("不使用系统代理", config) == "1")
                        CheckBox_DisableProxy.IsChecked = true;
                }
                catch (Exception) {; }
                try
                {
                    if (FindCookie("仅合并音频", config) == "1")
                        CheckBox_AudioOnly.IsChecked = true;
                }
                catch (Exception) {; }
            }

            if (!File.Exists(TextBox_EXE.Text))//尝试寻找主程序
            {
                DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory);
                foreach (FileInfo fi in d.GetFiles().Reverse())
                {
                    if (fi.Extension.ToUpper() == ".exe".ToUpper() && fi.Name.StartsWith("N_m3u8DL-CLI_"))
                    {
                        TextBox_EXE.Text = fi.Name;
                    }
                }
            }
        }

        private void Button_SelectExe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Environment.CurrentDirectory;
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "下载器(*.exe)|*.exe";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_EXE.Text = dialog.FileName;
            }
        }
    }
}
