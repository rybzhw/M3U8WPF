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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppConfigHelper.SetValue("Exe",         TextBox_EXE.Text);
            AppConfigHelper.SetValue("SavePath",    TextBox_WorkDir.Text);
            AppConfigHelper.SetValue("Proxy",       TextBox_Proxy.Text);
            AppConfigHelper.SetValue("Headers",     TextBox_Headers.Text);
            AppConfigHelper.SetValue("Exe",         TextBox_EXE.Text);

            AppConfigHelper.SetValue("Del",         CheckBox_Del.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("FastStart",   CheckBox_FastStart.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("BinaryMerge", CheckBox_BinaryMerge.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("ParserOnly",  CheckBox_ParserOnly.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("DisableDate", CheckBox_DisableDate.IsChecked == true ? "1" : "0");

            AppConfigHelper.SetValue("MaxThreads",  TextBox_Max.Text);
            AppConfigHelper.SetValue("MinThreads",  TextBox_Min.Text);
            AppConfigHelper.SetValue("Retry",       TextBox_Retry.Text);
            AppConfigHelper.SetValue("Timeout",     TextBox_Timeout.Text);
            AppConfigHelper.SetValue("StopSpeed",   TextBox_StopSpeed.Text);
            AppConfigHelper.SetValue("MaxSpeed",    TextBox_MaxSpeed.Text);

            AppConfigHelper.SetValue("DisableMerge", CheckBox_DisableMerge.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("DisableProxy", CheckBox_DisableProxy.IsChecked == true ? "1" : "0");
            AppConfigHelper.SetValue("AudioOnly",    CheckBox_AudioOnly.IsChecked == true ? "1" : "0");

            SettingConfigHelper.UpdateCommonTaskParam();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_EXE.Text = SettingConfigHelper.GetCommonTaskParam().Exe;
            TextBox_WorkDir.Text = SettingConfigHelper.GetCommonTaskParam().SavePath;
            TextBox_Proxy.Text = SettingConfigHelper.GetCommonTaskParam().Proxy;
            TextBox_Headers.Text = SettingConfigHelper.GetCommonTaskParam().Headers;

            CheckBox_Del.IsChecked = SettingConfigHelper.GetCommonTaskParam().Del;
            CheckBox_FastStart.IsChecked = SettingConfigHelper.GetCommonTaskParam().FastStart;
            CheckBox_BinaryMerge.IsChecked = SettingConfigHelper.GetCommonTaskParam().BinaryMerge;
            CheckBox_ParserOnly.IsChecked = SettingConfigHelper.GetCommonTaskParam().ParserOnly;
            CheckBox_DisableDate.IsChecked = SettingConfigHelper.GetCommonTaskParam().DisableDate;

            TextBox_Max.Text = SettingConfigHelper.GetCommonTaskParam().MaxThreads;
            TextBox_Min.Text = SettingConfigHelper.GetCommonTaskParam().MinThreads;
            TextBox_Retry.Text = SettingConfigHelper.GetCommonTaskParam().Retry;
            TextBox_Timeout.Text = SettingConfigHelper.GetCommonTaskParam().Timeout;
            TextBox_StopSpeed.Text = SettingConfigHelper.GetCommonTaskParam().StopSpeed;
            TextBox_MaxSpeed.Text = SettingConfigHelper.GetCommonTaskParam().MaxSpeed;

            CheckBox_DisableMerge.IsChecked = SettingConfigHelper.GetCommonTaskParam().DisableMerge;
            CheckBox_DisableProxy.IsChecked = SettingConfigHelper.GetCommonTaskParam().DisableProxy;
            CheckBox_AudioOnly.IsChecked = SettingConfigHelper.GetCommonTaskParam().AudioOnly;

            if (string.IsNullOrEmpty(TextBox_WorkDir.Text))
            {
                TextBox_WorkDir.Text = System.IO.Directory.GetCurrentDirectory();
            }
        }

        private void Button_SelectDir_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_WorkDir.Text = dialog.SelectedPath;
            }

            AppConfigHelper.SetValue("SavePath", TextBox_WorkDir.Text);
        }
        private void Button_SelectExe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Environment.CurrentDirectory;
            dialog.Multiselect = false;
            dialog.Title = "请选择文件夹";
            dialog.Filter = "下载器(*.exe)|*.exe";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_EXE.Text = dialog.FileName;
            }

            AppConfigHelper.SetValue("Exe", TextBox_EXE.Text);
        }
    }
}
