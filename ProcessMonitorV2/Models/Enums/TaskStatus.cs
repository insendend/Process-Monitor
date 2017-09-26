using System.Runtime.Serialization;

namespace ProcessMonitorV2.Models.Enums
{
    public enum TaskStatus
    {
        InProcess,
        Unknown,
        Success,
        Error,
        Canceled
    }
}
