using System.Runtime.Serialization;
using System.Xml.Serialization;
using ProcessMonitorV2.ViewModels;

namespace ProcessMonitorV2.Models
{
    [XmlRoot("Process")]
    [DataContract]
    public class ShortProcessInfo : ViewModelBase
    {
        [XmlAttribute]
        [DataMember(Name = "ProcessId")]
        public int Id { get; set; }

        [XmlAttribute]
        [DataMember(Name = "ProcessName")]
        public string Name { get; set; }

        private int threadCount;
        [XmlIgnore]
        [IgnoreDataMember]
        public int ThreadCount
        {
            get => threadCount;
            set
            {
                if (threadCount != value)
                {
                    threadCount = value;
                    OnPropertyChanged("ThreadCount");
                }
            }
        }

        private int handleCount;
        [XmlIgnore]
        [IgnoreDataMember]
        public int HandleCount
        {
            get => handleCount;
            set
            {
                if (handleCount != value)
                {
                    handleCount = value;
                    OnPropertyChanged("HandleCount");
                }
            }
        }

        private string title;
        [XmlIgnore]
        [IgnoreDataMember]
        public string WindowTitle
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged("WindowTitle");
                }
            }
        }

        private long memoryUsage;
        [XmlIgnore]
        [IgnoreDataMember]
        public long MemoryUsage
        {
            get => memoryUsage;
            set
            {
                if (memoryUsage != value)
                {
                    memoryUsage = value;
                    OnPropertyChanged("MemoryUsage");
                }
            }
        }
    }
}
