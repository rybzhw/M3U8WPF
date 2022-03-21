using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.IO;

namespace M3U8WPF
{
    /// <summary>
    /// CreateTask.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTask : Window
    {
        private MainWindow mainWindow;

        public CreateTask(MainWindow InMainWindow)
        {
            InitializeComponent();

            mainWindow = InMainWindow;
        }

        private void StartDownload(object sender, RoutedEventArgs e)
        {
            Button_GO.IsEnabled = false;

            if (TextBox_URL.Text == "")
            {
                System.Windows.MessageBox.Show("URL is empty!");
                return;
            }
            if (TextBox_SavePath.Text == "")
            {
                System.Windows.MessageBox.Show("SavePath is empty!");
                return;
            }

            UniqueTaskParam uniqueTaskParam = new UniqueTaskParam();
            uniqueTaskParam.URL = TextBox_URL.Text;
            uniqueTaskParam.SavePath = TextBox_SavePath.Text;
            uniqueTaskParam.Filename = TextBox_Filename.Text;

            mainWindow.StartDownload(uniqueTaskParam);
            Button_GO.IsEnabled = true;

            Close();
        }

        private void TextBox_URL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetUrl();
        }
        private void TextBox_URL_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilename();
        }
        private void Button_SavePath(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_SavePath.Text = dialog.SelectedPath;
            }
            AppConfigHelper.SetValue("LastSavePath", TextBox_SavePath.Text);
        }
        private void TextBox_SavePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetFilename();
        }

        private string GetUrlFileName(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return "None";
            }
            try
            {
                string[] strs1 = url.Split(new char[] { '/' });
                return GetValidFileName(System.Web.HttpUtility.UrlDecode(strs1[strs1.Length - 1].Split(new char[] { '?' })[0].Replace(".m3u8", "")));
            }
            catch (Exception)
            {
                return GetUniqueFilename();
            }
        }
        private string GetValidFileName(string input, string re = ".")
        {
            string title = input;
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                title = title.Replace(invalidChar.ToString(), re);
            }
            return title;
        }
        private string GetUniqueFilename()
        {
            return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
        }
        private string GetTitleFromURL(string url)
        {
            try
            {
                if (url.StartsWith("http"))
                    url = url.Replace("http://", "").Replace("https://", "");
                return GetUrlFileName(url);
            }
            catch (Exception)
            {
                return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
            }
        }

        private void FlashTextBox(System.Windows.Controls.TextBox textBox)
        {
            var orgColor = textBox.Background;
            SolidColorBrush myBrush = new SolidColorBrush();
            ColorAnimation myColorAnimation = new ColorAnimation();
            myColorAnimation.To = (Color)ColorConverter.ConvertFromString("#2ecc71");
            myColorAnimation.Duration = TimeSpan.FromMilliseconds(300);
            myBrush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation, HandoffBehavior.Compose);
            textBox.Background = myBrush;

            myColorAnimation.To = (Color)ColorConverter.ConvertFromString(orgColor.ToString());
            myColorAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            myBrush.BeginAnimation(SolidColorBrush.ColorProperty, myColorAnimation, HandoffBehavior.Compose);
            textBox.Background = myBrush;
        }

        private void SetUrl()
        {
            //从剪切板读取url
            Regex url = new Regex(@"(https?)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]", RegexOptions.Compiled | RegexOptions.Singleline);
            string str = url.Match(System.Windows.Clipboard.GetText()).Value;
            if (TextBox_URL != null)
            {
                if (str != "" && str != TextBox_URL.Text)
                {
                    TextBox_URL.Text = str;
                    FlashTextBox(TextBox_URL);
                }
            }
        }
        private void SetSavePath()
        {
            string Result = AppConfigHelper.GetValue("LastSavePath");
            if (string.IsNullOrEmpty(Result))
            {
                Result = AppConfigHelper.GetValue("SavePath");
            }

            if (TextBox_Filename != null)
            {
                TextBox_SavePath.Text = Result;
            }
        }
        private void SetFilename()
        {
            if (TextBox_Filename != null && TextBox_SavePath != null)
            {
                string Filename = GetTitleFromURL(TextBox_URL.Text);

                string FilePath = System.IO.Path.Combine(TextBox_SavePath.Text, Filename);
                string FullFilename = Filename + AppConfigHelper.GetValue("DefaultFileFormat");

                if (File.Exists(FullFilename) || Directory.Exists(FilePath))
                {
                    Filename = GetUniqueFilename();
                }

                TextBox_Filename.Text = Filename;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetUrl();
            SetSavePath();
        }
    }
}
