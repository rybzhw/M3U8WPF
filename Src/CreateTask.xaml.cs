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

        private string GetTaskBaseInfo()
        {
            var info = new UniqueTaskParam();
            info.URL = TextBox_URL.Text;
            info.SavePath = TextBox_SavePath.Text;
            info.Filename = TextBox_Filename.Text;

            return JsonSerializer.Serialize(info);
        }

        private void StartDownload(object sender, RoutedEventArgs e)
        {
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
            string FullPath = System.IO.Path.Combine(TextBox_SavePath.Text, TextBox_Filename.Text);
            if (TextBox_Filename.Text == "" || File.Exists(FullPath))
            {
                TextBox_Filename.Text = GetTitleFromURL("");
                FullPath = System.IO.Path.Combine(TextBox_SavePath.Text, TextBox_Filename.Text);
                if (File.Exists(FullPath))
                {
                    System.Windows.MessageBox.Show("{0} is Exists!", FullPath);
                    return;
                }
            }

            Button_GO.IsEnabled = false;
            mainWindow.StartDownload(GetTaskBaseInfo());
            Button_GO.IsEnabled = true;
            this.Close();
        }

        private void Button_SavePath(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox_SavePath.Text = dialog.SelectedPath;
            }
        }
        public static string GetUrlFileName(string url)
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
                return DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss");
            }
        }
        public static string GetValidFileName(string input, string re = ".")
        {
            string title = input;
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                title = title.Replace(invalidChar.ToString(), re);
            }
            return title;
        }
        private string GetTitleFromURL(string url)
        {
            try
            {
                if (File.Exists(url))
                    return System.IO.Path.GetFileNameWithoutExtension(url);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                var ext = System.IO.Path.GetExtension(Environment.GetCommandLineArgs()[1]);
                if (ext == ".m3u8" || ext == ".json" || ext == ".txt" || Directory.Exists(Environment.GetCommandLineArgs()[1]))
                    TextBox_URL.Text = Environment.GetCommandLineArgs()[1];
                if (TextBox_URL.Text != "")
                {
                    FlashTextBox(TextBox_URL);
                    if (!Directory.Exists(TextBox_URL.Text))
                        TextBox_Filename.Text = GetTitleFromURL(TextBox_URL.Text);
                }
            }
            else
            {
                //从剪切板读取url
                Regex url = new Regex(@"(https?)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]", RegexOptions.Compiled | RegexOptions.Singleline);
                string str = url.Match(System.Windows.Clipboard.GetText()).Value;
                TextBox_URL.Text = str;
                if (TextBox_URL.Text != "")
                {
                    FlashTextBox(TextBox_URL);
                    TextBox_Filename.Text = GetTitleFromURL(TextBox_URL.Text);
                }
            }
        }

        private void TextBox_URL_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //从剪切板读取url
            Regex url = new Regex(@"(https?)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]", RegexOptions.Compiled | RegexOptions.Singleline);//取已下载大小
            string str = url.Match(System.Windows.Clipboard.GetText()).Value;
            if (str != "" && str != TextBox_URL.Text)
            {
                TextBox_URL.Text = str;
                FlashTextBox(TextBox_URL);
                TextBox_Filename.Text = GetTitleFromURL(TextBox_URL.Text);
            }
        }
    }
}
