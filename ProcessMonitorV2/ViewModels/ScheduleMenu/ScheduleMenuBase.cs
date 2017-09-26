using System;
using System.Windows.Input;
using ProcessMonitorV2.Models;
using ProcessMonitorV2.ViewModels.Commands;

namespace ProcessMonitorV2.ViewModels.ScheduleMenu
{
    abstract class ScheduleMenuBase : ViewModelBase
    {
        #region Properties

        private string path;
        public string Path
        {
            get => path;
            set
            {
                if (path != value)
                {
                    path = value;
                    OnPropertyChanged("Path");
                }
            }
        }

        public DateTime Date { get; set; }
        public DateTime Time { get; set; }

        public ICommand SubmitCommand { get; }

        public TaskInfo Task { get; set; }

        #endregion

        #region Constructors

        protected ScheduleMenuBase()
        {
            Date = DateTime.Now;
            Time = DateTime.Now + TimeSpan.FromMinutes(1d);
            SubmitCommand = new SimpleCommand(CreateTask);
        }

        #endregion

        #region Methods

        protected bool IsValidTime(DateTime datetime) => datetime > DateTime.Now;

        public abstract void CreateTask();

        #endregion
    }
}
