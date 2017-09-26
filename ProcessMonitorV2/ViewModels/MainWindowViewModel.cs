using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using ProcessMonitorV2.Models;
using ProcessMonitorV2.Models.Enums;
using ProcessMonitorV2.Models.Logging;
using ProcessMonitorV2.ViewModels.Commands;
using ProcessMonitorV2.Views;
using ProcessMonitorV2.ViewModels.ScheduleMenu;

namespace ProcessMonitorV2.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private const string logpath = "log_tasks.xml";
        private static readonly object locker;
        private static readonly ILogger taskLogger;
        private static readonly ILogger procLogger;

        #endregion

        #region Properties

        // content of processes table
        public ObservableCollection<ShortProcessInfo> ShortProcessesInfo { get; set; }
        
        // selected item in processes table
        public ShortProcessInfo SelectedProcess { get; set; }

        // selected item in processes table
        public TaskInfo SelectedTask { get; set; }

        // content of schedule tasks table
        public ObservableCollection<TaskInfo> ScheduleTasks { get; set; }

        // commands
        public ICommand SaveToFileCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand FinishCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        #region Constructors

        static MainWindowViewModel()
        {
            locker = new object();
            taskLogger = new TaskLogger(logpath);
            procLogger = new ProcessLogger();
        }

        public MainWindowViewModel()
        {
            SaveToFileCommand = new SimpleCommand(SaveToFile);
            StartCommand = new SimpleCommand(ShowStartMenu);
            FinishCommand = new SimpleCommand(ShowFinishMenu, () => SelectedProcess != null);
            CancelCommand = new SimpleCommand(CancelTask, () => SelectedTask?.Status == TaskStatus.InProcess);

            InitScheduleTasks();
            InitProcesses();
            InitTimer();
        }

        #endregion

        #region Methods

        private void InitScheduleTasks()
        {
            ScheduleTasks = new ObservableCollection<TaskInfo>();
            BindingOperations.EnableCollectionSynchronization(ScheduleTasks, locker);

            taskLogger.LoadIn(ScheduleTasks);
        }

        private void InitProcesses()
        {
            ShortProcessesInfo = new ObservableCollection<ShortProcessInfo>();
            BindingOperations.EnableCollectionSynchronization(ShortProcessesInfo, locker);

            foreach (var proc in Process.GetProcesses())      
                ShortProcessesInfo.Add(
                    new ShortProcessInfo
                    {
                        Id = proc.Id,
                        Name = proc.ProcessName,
                        ThreadCount = proc.Threads.Count,
                        HandleCount = proc.HandleCount,
                        WindowTitle = proc.MainWindowTitle,
                        MemoryUsage = proc.PagedMemorySize64 / 1024
                    });         
        }

        private void InitTimer()
        {
            // init and start timer which will update property with info every 1 sec
            var timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        // TODO comment if no need (one iteration ~20-30ms)
        //private static readonly Stopwatch sw = new Stopwatch();
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //sw.Start();
            UpdateProcesses();
            UpdateScheduleTasks();
            //sw.Stop();
            //Debug.WriteLine(sw.Elapsed.Milliseconds + " ms");
            //sw.Reset();
        }

        private void UpdateProcesses()
        {
            var procs = Process.GetProcesses();
            foreach (var proc in procs)
            {
                // found process with similar Id
                var myProc = ShortProcessesInfo.FirstOrDefault(x => x.Id == proc.Id);

                if (myProc is null)
                {
                    // new process, need to add
                    Application.Current.Dispatcher
                        .Invoke(() => ShortProcessesInfo.Add(new ShortProcessInfo
                    {
                        Id = proc.Id,
                        Name = proc.ProcessName,
                        ThreadCount = proc.Threads.Count,
                        HandleCount = proc.HandleCount,
                        WindowTitle = proc.MainWindowTitle,
                        MemoryUsage = proc.PagedMemorySize64 / 1024
                    }));
                }
                else
                {
                    // check for each properties which changed
                    myProc.Id = proc.Id;
                    myProc.Name = proc.ProcessName;
                    myProc.ThreadCount = proc.Threads.Count;
                    myProc.HandleCount = proc.HandleCount;
                    myProc.WindowTitle = proc.MainWindowTitle;
                    myProc.MemoryUsage = proc.PagedMemorySize64 / 1024;
                }
            }

            // delete old processes
            var procsIds = procs.Select(proc => proc.Id);
            var oldProcs = ShortProcessesInfo.Where(myProc => !procsIds.Contains(myProc.Id)).ToList();

            foreach (var oldProc in oldProcs)
                Application.Current.Dispatcher
                    .Invoke(() => ShortProcessesInfo.Remove(oldProc)); 
        }

        private void UpdateScheduleTasks()
        {
            if (ScheduleTasks is null || ScheduleTasks.Count == 0)
                return;

            foreach (var task in ScheduleTasks)
            {
                // finished, canselled, unknown tasks
                if (task.Status != TaskStatus.InProcess || task.Time > DateTime.Now)
                    continue;

                // tasks will not be run in time (while app has been closed)
                if (task.Status == TaskStatus.InProcess && task.Time < DateTime.Now - TimeSpan.FromSeconds(3d))
                {
                    task.Status = TaskStatus.Unknown;
                    task.StatusDescription = "Program has been offline, time expired";
                }
                else
                    task.Execute();

                taskLogger.Update(task);
            }
        }

        private void ShowStartMenu()
        {
            // show window of starting new program
            var menuStart = new ScheduleMenuStart();
            var vmStart = menuStart.DataContext as ScheduleMenuStartViewModel;
            if (vmStart is null) return;

            try
            {
                // timepicker (Extended.Wpf.Toolkit nuget)
                // throw exception when select a ':' in time value
                menuStart.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(
                    "wpfToolkit:TimePicker throw exception.\r\n" +
                    "Don't select ':' while changing value of time.\r\n" +
                    "Window will be closed. Try again...",
                    "TimePicker error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);

                // close window
                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(window => window.Title == "Start process")
                    ?.Close();
            }

            // get info about task back
            if (vmStart.Task is null) return;

            ScheduleTasks.Add(vmStart.Task);
            taskLogger.Save(vmStart.Task);
        }

        private void ShowFinishMenu()
        {
            // show window of finishing selected process
            var menuFinish = new ScheduleMenuFinish();
            var vmFinish = menuFinish.DataContext as ScheduleMenuFinishViewModel;
            if (vmFinish is null) return;

            vmFinish.Path = $"{SelectedProcess.Name} will be finished: Warining: undefined behavior!";
            vmFinish.Id = SelectedProcess.Id;

            try
            {
                // timepicker (Extended.Wpf.Toolkit nuget)
                // throw exception when select a ':' in time value
                menuFinish.ShowDialog();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(
                    "wpfToolkit:TimePicker throw exception.\r\n" +
                    "Don't select ':' while changing value of time.\r\n" +
                    "Window will be closed. Try again...",
                    "TimePicker error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);

                // close window
                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(window => window.Title == "Start process")
                    ?.Close();
            }

            // get info about task back
            if (vmFinish.Task is null) return;

            ScheduleTasks.Add(vmFinish.Task);
            taskLogger.Save(vmFinish.Task);
        }

        private void CancelTask()
        {
            if (SelectedTask is null || SelectedTask.Status != TaskStatus.InProcess)
                return;

            // change status
            SelectedTask.Status = TaskStatus.Canceled;
            SelectedTask.StatusDescription = "Canceled by user";

            // change status in log
            taskLogger.Update(SelectedTask);
        }

        private void SaveToFile()
        {
            // save processes into the file (xml, json)
            procLogger.Save(ShortProcessesInfo);
        }

        #endregion
    }
}
