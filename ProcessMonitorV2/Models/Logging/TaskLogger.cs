using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ProcessMonitorV2.Models.Enums;

namespace ProcessMonitorV2.Models.Logging
{
    class TaskLogger : ILogger
    {
        private readonly string path;

        public bool IsExists => File.Exists(path);

        public TaskLogger(string path)
        {
            this.path = path;
        }

        public void Save(object obj)
        {
            var taskinfo = obj as TaskInfo;

            if (taskinfo is null)
                throw new ArgumentException(nameof(obj));

            var task = new XElement("task",
                new XAttribute("mode", taskinfo.Mode),
                new XAttribute("time", taskinfo.Time),
                new XAttribute("status", taskinfo.Status),
                new XElement("path", taskinfo.Path),
                new XElement("args", taskinfo.Params),
                new XElement("description", taskinfo.StatusDescription));

            XDocument document;

            if (IsExists)
            {
                document = XDocument.Load(path);
                var tasks = document.Element("tasks");
                tasks?.Add(task);
            }
            else
            {
                document = new XDocument();
                var tasks = new XElement("tasks");
                tasks.Add(task);
                document.Add(tasks);
            }

            document.Save(path);
        }

        public void LoadIn(object obj)
        {
            var tasks = obj as ObservableCollection<TaskInfo>;

            if (tasks is null)
                throw new ArgumentException(nameof(obj));

            if (!IsExists) return;

            var xdoc = XDocument.Load(path);
            var root = xdoc.Element("tasks");

            if (root is null) return;

            foreach (var task in root.Elements("task").ToList())
            {
                TaskMode mode;
                Enum.TryParse(task.Attribute("mode")?.Value, out mode);

                DateTime.TryParse(task.Attribute("time")?.Value, out var time);

                TaskStatus status;
                Enum.TryParse(task.Attribute("status")?.Value, out status);

                tasks.Add(new TaskInfo
                {
                    Mode = mode,
                    Time = time,
                    Status = status,
                    Path = task.Element("path")?.Value,
                    Params = task.Element("args")?.Value,
                    StatusDescription = task.Element("description")?.Value
                });
            }
        }

        public void Update(object obj)
        {
            var taskinfo = obj as TaskInfo;

            if (taskinfo is null)
                throw new ArgumentException(nameof(obj));

            if (!IsExists) return;

            var xdoc = XDocument.Load(path);
            var root = xdoc.Element("tasks");

            if (root is null) return;

            foreach (var task in root.Elements("task").ToList())
            {
                if (DateTime.Parse(task.Attribute("time").Value) != taskinfo.Time)
                    continue;

                task.Attribute("status").Value = taskinfo.Status.ToString();
                task.Element("description").Value = taskinfo.StatusDescription;
                break;
            }

            xdoc.Save(path);
        }
    }
}
