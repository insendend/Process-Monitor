using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ProcessMonitorV2.Models;
using ProcessMonitorV2.ViewModels.Commands;
using System.Linq;
using ProcessMonitorV2.Models.Enums;

namespace ProcessMonitorV2.ViewModels.ScheduleMenu
{
    class ScheduleMenuStartViewModel : ScheduleMenuBase
    {
        #region Properties

        public string CmdParam { get; set; }

        public ICommand OpenFileCommand { get; }

        #endregion

        #region Constructors

        public ScheduleMenuStartViewModel() : base()
        {
            OpenFileCommand = new SimpleCommand(FileBrowseDialog);
        }

        #endregion

        #region Methods

        private void FileBrowseDialog()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
                Path = ofd.FileName;
        }

        public override void CreateTask()
        {
            // check choosen time
            var date = Date.Date.Add(Time.TimeOfDay);
            if (!IsValidTime(date))
            {
                MessageBox.Show(
                    "The specified time has already gone, please try again...",
                    "Incorrect time",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // init task
            Task = new TaskInfo
            {
                Path = Path,
                Params = CmdParam,
                Mode = TaskMode.Run,
                Time = date,
                Status = TaskStatus.InProcess,
                StatusDescription = "Waiting for execute..."
            };

            // close window
            Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.Title == "Start process")
            ?.Close();
        }

        #endregion
    }
}
