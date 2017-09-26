using System;
using System.Diagnostics;
using ProcessMonitorV2.Models.Enums;
using ProcessMonitorV2.ViewModels;

namespace ProcessMonitorV2.Models
{
    class TaskInfo : ViewModelBase
    {
        public TaskMode Mode { get; set; }

        public string Path { get; set; }

        public string Params { get; set; }

        public DateTime Time { get; set; }

        public int DeleteId { get; set; }

        private TaskStatus status;
        public TaskStatus Status
        {
            get => status;
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        private string desc;
        public string StatusDescription
        {
            get => desc;
            set
            {
                if (desc != value)
                {
                    desc = value;
                    OnPropertyChanged("StatusDescription");
                }
            }
        }

        public void Execute()
        {
            switch (Mode)
            {
                case TaskMode.Run:
                    StartTask();
                    break;

                case TaskMode.Finish:
                    FinishTask();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }        
        }

        private void StartTask()
        {
            try
            {
                Process.Start(Path, Params);
                StatusDescription = "Started succesfully";
                Status = TaskStatus.Success;
            }
            catch (Exception ex)
            {
                Status = TaskStatus.Error;
                StatusDescription = ex.Message;
            }
        }

        private void FinishTask()
        {
            try
            {
                Process.GetProcessById(DeleteId).Kill();
                StatusDescription = "Finished succesfully";
                Status = TaskStatus.Success;
            }
            catch (Exception ex)
            {
                Status = TaskStatus.Error;
                StatusDescription = ex.Message;
            }
        }
    }
}
