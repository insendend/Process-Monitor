using System.IO;

namespace ProcessMonitorV2.Models.Serializing
{
    interface ISerializer
    {
        void Serialize<T>(Stream stream, T serializingObject);

        T Deserialize<T>(Stream stream);
    }
}
