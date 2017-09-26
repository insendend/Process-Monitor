using System.Linq;
using System.Windows;
using ProcessMonitorV2.Models;
using ProcessMonitorV2.Models.Enums;

namespace ProcessMonitorV2.ViewModels.ScheduleMenu
{
    class ScheduleMenuFinishViewModel : ScheduleMenuBase
    {
        public int Id { get; set; }

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
                DeleteId = Id,
                Path = Path,
                Mode = TaskMode.Finish,
                Time = date,
                Status = TaskStatus.InProcess,
                StatusDescription = "Waiting for execute..."
            };

            // close window
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(window => window.Title == "Finish process")
                ?.Close();
        }
    }
}
