using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace M3U8WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<TaskState> AllTaskStates { get; set; }
        private TaskStateEnum FilterTaskState;
        private System.Timers.Timer TaskStateUpdateTimer;

        public MainWindow()
        {
            InitializeComponent();

            InitCommonTaskParam();

            TaskStateUpdateTimer = new System.Timers.Timer();
            TaskStateUpdateTimer.Enabled = true;
            TaskStateUpdateTimer.Interval = 100;
            TaskStateUpdateTimer.AutoReset = true;
            TaskStateUpdateTimer.Start();
            TaskStateUpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);

            AllTaskStates = new List<TaskState>();

            FilterTaskState = TaskStateEnum.TSE_None;

            DataContext = this;

            ListView_TaskState.ItemsSource = AllTaskStates;
        }
        ~MainWindow()
        {
            TaskStateUpdateTimer.Stop();
        }

        private string GetPrompt(TaskStateEnum InState)
        {
            switch (InState)
            {
                case TaskStateEnum.TSE_Prepare:
                    return "准备中";
                case TaskStateEnum.TSE_Downloading:
                    return "正在下载";
                case TaskStateEnum.TSE_Done:
                    return "已完成";
            }
            return "准备中";
        }
        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            for (int i = 0; i < AllTaskStates.Count; i++)
            {
                var State = AllTaskStates[i];

                if (State.CmdProcess == null)
                    continue;
                if (State.CmdProcess.HasExited)
                {
                    TaskStateEnumUpdate(ref State, TaskStateEnum.TSE_Done);
                    State.CmdProcess = null;
                    continue;
                }

                string OutputStr = "";

                try
                {
                    if (!State.CmdProcess.StandardOutput.EndOfStream)
                        OutputStr = State.CmdProcess.StandardOutput.ReadLine();

                    if (!string.IsNullOrEmpty(OutputStr))
                    {
                        ParseProcess(ref State, OutputStr);
                    }
                }
                catch (Exception)
                {
                }
            }
            RefreshListView();
        }
        private void ParseProcess(ref TaskState InState, string InProcess)
        {
            string Pattern = @"(?<Chunk>\d+/\d+).*\((?<Progress>\d+.\d+%).* (?<FileSize>\d+.*[A-Z]{2}) ";
            Pattern += @"\((?<Speed>.*) @ (?<LeftTime>.*)\)";

            if (Regex.IsMatch(InProcess, Pattern))
            {
                foreach (Match match in Regex.Matches(InProcess, Pattern))
                {
                    InState.Chunk = match.Groups["Chunk"].Value;
                    InState.Progress = match.Groups["Progress"].Value;
                    InState.FileSize = match.Groups["FileSize"].Value;
                    InState.Speed = match.Groups["Speed"].Value;
                    InState.LeftTime = match.Groups["LeftTime"].Value;
                    TaskStateEnumUpdate(ref InState, TaskStateEnum.TSE_Downloading);
                    break;
                }
            }
            else
            {
                Pattern = @"时长.(?<VideoLength>.*)";
                if (Regex.IsMatch(InProcess, Pattern))
                {
                    foreach (Match match in Regex.Matches(InProcess, Pattern))
                    {
                        InState.VideoLength = match.Groups["VideoLength"].Value;
                        break;
                    }
                }
                TaskStateEnumUpdate(ref InState, TaskStateEnum.TSE_Prepare);
            }

            Pattern = @" .*";
            if (Regex.IsMatch(InProcess, Pattern))
                InState.StateDetail = Regex.Match(InProcess, Pattern).Value;
            else
                InState.StateDetail = InProcess;
        }

        public void Btn_CreateTask_Click(object sender, RoutedEventArgs e)
        {
            CreateTask createTask = new CreateTask(this);
            createTask.Show();
        }

        private void InitCommonTaskParam()
        {
            SettingConfigHelper.InitCommonTaskParam();
        }

        private string GetCommonTaskParam()
        {
            //commonTaskParam.Exe = @"C:\GitSSD\CSharpOutput\bin\Debug\net5.0\CSharpOutput.exe";

            return JsonSerializer.Serialize(SettingConfigHelper.GetCommonTaskParam());
        }

        private string GetFinalTaskParam(CommonTaskParam InCommon, UniqueTaskParam InUnique)
        {
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

            {
                if (!string.IsNullOrEmpty(InCommon.Headers))
                    sb.Append("--headers \"" + InCommon.Headers + "\" ");
                if (!string.IsNullOrEmpty(InCommon.BaseUrl))
                    sb.Append("--baseUrl \"" + InCommon.BaseUrl + "\" ");
                if (!string.IsNullOrEmpty(InCommon.MuxSetJson))
                    sb.Append("--muxSetJson \"" + InCommon.MuxSetJson + "\" ");
                if (InCommon.MaxThreads != "32")
                    sb.Append("--maxThreads \"" + InCommon.MaxThreads + "\" ");
                if (InCommon.MinThreads != "16")
                    sb.Append("--minThreads \"" + InCommon.MinThreads + "\" ");
                if (InCommon.RetryCount != "15")
                    sb.Append("--retryCount \"" + InCommon.RetryCount + "\" ");
                if (InCommon.TimeOut != "10")
                    sb.Append("--timeOut \"" + InCommon.TimeOut + "\" ");
                if (InCommon.StopSpeed != "0")
                    sb.Append("--stopSpeed \"" + InCommon.StopSpeed + "\" ");
                if (InCommon.MaxSpeed != "0")
                    sb.Append("--maxSpeed \"" + InCommon.MaxSpeed + "\" ");
                if (InCommon.Key != "")
                {
                    if (File.Exists(InCommon.Key))
                        sb.Append("--useKeyFile \"" + InCommon.Key + "\" ");
                    else
                        sb.Append("--useKeyBase64 \"" + InCommon.Key + "\" ");
                }
                if (InCommon.IV != "")
                {
                    sb.Append("--useKeyIV \"" + InCommon.IV + "\" ");
                }
                if (InCommon.Proxy != "")
                {
                    sb.Append("--proxyAddress \"" + InCommon.Proxy.Trim() + "\" ");
                }
                if (InCommon.Del == true)
                    sb.Append("--enableDelAfterDone ");
                if (InCommon.FastStart == true)
                    sb.Append("--enableMuxFastStart ");
                if (InCommon.BinaryMerge == true)
                    sb.Append("--enableBinaryMerge ");
                if (InCommon.ParserOnly == true)
                    sb.Append("--enableParseOnly ");
                if (InCommon.DisableDate == true)
                    sb.Append("--disableDateInfo ");
                if (InCommon.DisableMerge == true)
                    sb.Append("--noMerge ");
                if (InCommon.DisableProxy == true)
                    sb.Append("--noProxy ");
                if (InCommon.DisableCheck == true)
                    sb.Append("--disableIntegrityCheck ");
                if (InCommon.AudioOnly == true)
                    sb.Append("--enableAudioOnly ");
                if (InCommon.RangeStart != "00:00:00" || InCommon.RangeEnd != "00:00:00")
                {
                    sb.Append($"--downloadRange \"{InCommon.RangeStart}-{InCommon.RangeEnd}\"");
                }
            }

            return sb.ToString();
        }

        public void StartDownload(string InParam)
        {
            var uniqueTaskParam = JsonSerializer.Deserialize<UniqueTaskParam>(InParam);

            string ExePath = SettingConfigHelper.GetCommonTaskParam().Exe;
            string Arguments = GetFinalTaskParam(SettingConfigHelper.GetCommonTaskParam(), uniqueTaskParam);

            if (AllTaskStates.Exists(x => x.Filename == uniqueTaskParam.Filename))
            {
                MessageBox.Show("uniqueTaskParam.Filename exists!");
                return;
            }

            var NewTaskState = new TaskState();
            NewTaskState.Filename = uniqueTaskParam.Filename;
            NewTaskState.SavePath = uniqueTaskParam.SavePath;

            NewTaskState.CmdProcess = new Process();
            NewTaskState.CmdProcess.StartInfo.FileName = ExePath;
            NewTaskState.CmdProcess.StartInfo.Arguments = Arguments;
            NewTaskState.CmdProcess.StartInfo.UseShellExecute = false;
            NewTaskState.CmdProcess.StartInfo.CreateNoWindow = true;
            NewTaskState.CmdProcess.StartInfo.RedirectStandardOutput = true;
            NewTaskState.CmdProcess.Start();

            AllTaskStates.Add(NewTaskState);
            ChildProcessTracker.AddProcess(NewTaskState.CmdProcess);
        }

        private void CmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Btn_Setting_Click(object sender, RoutedEventArgs e)
        {
            var createSettings = new Settings();
            createSettings.Show();
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ListView_TaskState.SelectedItems.Count <= 0)
                return;

            List<TaskState> Items = ListView_TaskState.SelectedItems as List<TaskState>;
            for (int i = AllTaskStates.Count - 1; i >= 0; --i)
            {
                foreach (Object Item in ListView_TaskState.SelectedItems)
                {
                    TaskState taskState = Item as TaskState;
                    if (taskState == null || !(taskState is TaskState))
                        continue;

                    if (taskState.Filename == AllTaskStates[i].Filename)
                    {
                        DeleteTask(i);
                    }
                }
            }

            RefreshListView();
        }

        public void DeleteTask(int InIndex)
        {
            if (InIndex < 0 || InIndex >= AllTaskStates.Count)
                return;

            TaskState DeleteTask = AllTaskStates[InIndex];
            // Kill process 
            if (DeleteTask.CmdProcess != null)
                DeleteTask.CmdProcess.Kill();

            // Remove file and directory
            string DeletePath = System.IO.Path.Combine(DeleteTask.SavePath, DeleteTask.Filename);
            if (Directory.Exists(DeletePath))
                Directory.Delete(DeletePath, true);

            string DeleteFile = DeletePath + ".mp4";
            if (File.Exists(DeleteFile))
                File.Delete(DeleteFile);

            // Other
            AllTaskStates.RemoveAt(InIndex);
        }

        private void RefreshListView()
        {
            try
            {
                ListView_TaskState.Dispatcher.Invoke(
                    new Action(delegate
                    {
                        ListView_TaskState.Items.Refresh();
                    }));
            }
            catch (Exception)
            {

            }
        }

        private void Btn_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (AllTaskStates == null || AllTaskStates.Count <= 0)
                return;
            for (int i = AllTaskStates.Count - 1; i >= 0; --i)
            {
                DeleteTask(i);
            }
            RefreshListView();
        }
        private void TaskStateEnumUpdate(ref TaskState InOrg, TaskStateEnum InNewEnum)
        {
            if (InOrg.taskStateEnum == InNewEnum)
            {
                return;
            }
            InOrg.taskStateEnum = InNewEnum;
            InOrg.StatePrompt = GetPrompt(InOrg.taskStateEnum);

            if (ListView_TaskState != null)
            {
                ListView_TaskState.Dispatcher.Invoke(
                    new Action(delegate
                    {
                        ListView_TaskState.Items.Filter = TaskStateFilter;
                    }));
            }
        }

        private bool TaskStateFilter(object obj)
        {
            if (obj is TaskState taskState)
            {
                switch (FilterTaskState)
                {
                    case TaskStateEnum.TSE_None:
                        return true;
                    case TaskStateEnum.TSE_Downloading:
                        return taskState.taskStateEnum == FilterTaskState 
                            || taskState.taskStateEnum == TaskStateEnum.TSE_Prepare;
                    case TaskStateEnum.TSE_Done:
                        return taskState.taskStateEnum == FilterTaskState;
                }
            }
            return true;
        }

        private void Filter_None_Selected(object sender, RoutedEventArgs e)
        {
            FilterTaskState = TaskStateEnum.TSE_None;
            if (ListView_TaskState != null)
            {
                ListView_TaskState.Items.Filter = TaskStateFilter;
            }
        }

        private void Filter_Downloading_Selected(object sender, RoutedEventArgs e)
        {
            FilterTaskState = TaskStateEnum.TSE_Downloading;
            if (ListView_TaskState != null)
            {
                ListView_TaskState.Items.Filter = TaskStateFilter;
            }
        }

        private void Filter_Done_Selected(object sender, RoutedEventArgs e)
        {
            FilterTaskState = TaskStateEnum.TSE_Done;
            if (ListView_TaskState != null)
            {
                ListView_TaskState.Items.Filter = TaskStateFilter;
            }
        }
    }
}
