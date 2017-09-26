using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ProcessMonitorV2.Models.Serializing;

namespace ProcessMonitorV2.Models.Logging
{
    class ProcessLogger : ILogger
    {
        public void Save(object obj)
        {
            var procs = obj as ObservableCollection<ShortProcessInfo>;

            if (procs is null)
                throw new ArgumentException(nameof(obj));

            // init and show dialog window
            var sfd = new SaveFileDialog
            {
                Filter = "XML file (*.xml)|*.xml|JSON file (*.json)|*.json",
                RestoreDirectory = true
            };
            if (sfd.ShowDialog() != true) return;
            var filepath = sfd.FileName;

            try
            {
                using (var stream = sfd.OpenFile())
                {
                    // file extension which has been chosen by user
                    var mask = Path.GetExtension(filepath).ToLower();

                    // choosing a type of serialization (xml or json)
                    var serializer =
                        mask == ".json"
                            ? new JsonSimpleSerializer()
                            : mask == ".xml"
                                ? new XmlSimpleSerializer() as ISerializer
                                : null;

                    if (serializer == null)
                        throw new FileFormatException();

                    // saving to file
                    serializer.Serialize(stream, procs);

                    MessageBox.Show(
                        "Has been saved in " + filepath,
                        "Saving...",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (FileLoadException ex)
            {
                MessageBox.Show(ex.Message, "Error of openning file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FileFormatException ex)
            {
                MessageBox.Show(ex.Message, "Incorrect file extension", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error of saving file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void LoadIn(object obj)
        {
            // no need
            throw new NotImplementedException();
        }

        public void Update(object obj)
        {
            // no need
            throw new NotImplementedException();
        }
    }
}
