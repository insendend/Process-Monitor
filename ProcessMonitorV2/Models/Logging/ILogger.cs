namespace ProcessMonitorV2.Models.Logging
{
    interface ILogger
    {
        void Save(object obj);

        void LoadIn(object obj);

        void Update(object obj);
    }
}
