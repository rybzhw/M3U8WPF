using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
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
        public string Url;

        public ReadProcessThread ReadCmdProcess;
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
        public override string ToString()
        {
            return string.Format("URL:{0} Filename:{1} StatePrompt:{2} StateDetail:{3}", 
                Url, Filename, StatePrompt, StateDetail);
        }
    }

    public class ReadProcessThread
    {
        private string Output;
        private Thread thread;
        public ReadProcessThread(Process InProcess)
        {
            thread = new Thread(() => 
            {
                while (InProcess != null)
                {
                    if (!InProcess.StandardOutput.EndOfStream)
                        Output = InProcess.StandardOutput.ReadLine();
                    Thread.Sleep(100);
                }
            });
            thread.Start();
        }

        ~ReadProcessThread()
        {
            try
            {
                thread.Interrupt();
            }
            catch (Exception)
            {
            }
        }
        public string GetOutput()
        {
            return Output;
        }
    }

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

            AppLogHelper.InitLog();
            SettingConfigHelper.InitCommonTaskParam();

            TaskStateUpdateTimer = new System.Timers.Timer();
            TaskStateUpdateTimer.Enabled = true;
            TaskStateUpdateTimer.Interval = 100;
            TaskStateUpdateTimer.AutoReset = true;
            TaskStateUpdateTimer.Start();
            TaskStateUpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);

            AllTaskStates = new List<TaskState>();

            FilterTaskState = TaskStateEnum.TSE_None;

            DataContext = this;
        }
        ~MainWindow()
        {
            TaskStateUpdateTimer.Stop();
        }

        public void StartDownload(UniqueTaskParam InUniqueTaskParam)
        {
            if (AllTaskStates.Exists(x => x.Url == InUniqueTaskParam.URL))
            {
                MessageBox.Show("Task exists!");
                return;
            }

            string ExePath = SettingConfigHelper.GetCommonTaskParam().Exe;
            string Arguments = SettingConfigHelper.GetFinalTaskParam(InUniqueTaskParam);

            TaskState NewTaskState = new TaskState();
            NewTaskState.Url = InUniqueTaskParam.URL;
            NewTaskState.Filename = InUniqueTaskParam.Filename;
            NewTaskState.SavePath = InUniqueTaskParam.SavePath;
            TaskStateEnumUpdate(ref NewTaskState, TaskStateEnum.TSE_Prepare);

            NewTaskState.CmdProcess = new Process();
            NewTaskState.CmdProcess.StartInfo.FileName = ExePath;
            NewTaskState.CmdProcess.StartInfo.Arguments = Arguments;
            NewTaskState.CmdProcess.StartInfo.UseShellExecute = false;
            NewTaskState.CmdProcess.StartInfo.CreateNoWindow = true;
            NewTaskState.CmdProcess.StartInfo.RedirectStandardOutput = true;
            NewTaskState.CmdProcess.Start();

            NewTaskState.ReadCmdProcess = new ReadProcessThread(NewTaskState.CmdProcess);
            AppLogHelper.Log("MainWindow StartDownload Process ExePath={0} Arguments={1}", ExePath, Arguments);

            AllTaskStates.Add(NewTaskState);
            ChildProcessTracker.AddProcess(NewTaskState.CmdProcess);

            AppLogHelper.Log("MainWindow StartDownload AllTaskStates {0} TaskState{1}",
                AllTaskStates.Count, NewTaskState.ToString());

            UpdateFilter();
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
                    State.Chunk = "";
                    State.Progress = "";
                    State.FileSize = "";
                    State.Speed = "";
                    State.LeftTime = "";
                    State.StateDetail = "";
                    TaskStateEnumUpdate(ref State, TaskStateEnum.TSE_Done);
                    State.CmdProcess = null;
                    State.ReadCmdProcess = null;
                    continue;
                }

                string OutputStr = "";

                try
                {
                    //if (!State.CmdProcess.StandardOutput.EndOfStream)
                    //    OutputStr = State.CmdProcess.StandardOutput.ReadLine();

                    OutputStr = State.ReadCmdProcess.GetOutput();
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
            }

            Pattern = @" .*";
            if (Regex.IsMatch(InProcess, Pattern))
                InState.StateDetail = Regex.Match(InProcess, Pattern).Value;
            else
                InState.StateDetail = InProcess;
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
        private void TaskStateEnumUpdate(ref TaskState InOrg, TaskStateEnum InNewEnum)
        {
            if (InOrg.taskStateEnum == InNewEnum)
            {
                return;
            }
            InOrg.taskStateEnum = InNewEnum;
            InOrg.StatePrompt = GetPrompt(InOrg.taskStateEnum);

            AppLogHelper.Log("MainWindow TaskStateEnumUpdate TaskState{0}", InOrg.ToString());

            UpdateFilter();
        }

        public void Btn_CreateTask_Click(object sender, RoutedEventArgs e)
        {
            CreateTask createTask = new CreateTask(this);
            createTask.Show();
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

            UpdateFilter();
            RefreshListView();
        }
        private void Btn_DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            if (AllTaskStates == null || AllTaskStates.Count <= 0)
                return;
            for (int i = AllTaskStates.Count - 1; i >= 0; --i)
            {
                DeleteTask(i);
            }
            UpdateFilter();
            RefreshListView();
        }

        private void UpdateFilter()
        {
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
            UpdateFilter();
        }
        private void Filter_Downloading_Selected(object sender, RoutedEventArgs e)
        {
            FilterTaskState = TaskStateEnum.TSE_Downloading;
            UpdateFilter();
        }
        private void Filter_Done_Selected(object sender, RoutedEventArgs e)
        {
            FilterTaskState = TaskStateEnum.TSE_Done;
            UpdateFilter();
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
            try
            {
                if (Directory.Exists(DeletePath))
                    Directory.Delete(DeletePath, true);

                string DeleteFile = DeletePath + AppConfigHelper.GetValue("DefaultFileFormat");
                if (File.Exists(DeleteFile))
                    File.Delete(DeleteFile);
            }
            catch (Exception e)
            {
                AppLogHelper.Error("DeleteTask delete failed. File:{0} Url:{1} \r\n {2}", 
                    DeletePath, DeleteTask.Url, e.ToString());
            }

            // Other
            AllTaskStates.RemoveAt(InIndex);
        }
    }
}
