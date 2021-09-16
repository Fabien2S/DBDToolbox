using System.IO;

namespace DBDToolbox.IO
{
    public static class FileHelper
    {
        public static void DeleteSafely(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static void MoveSafely(string file, string destination)
        {
            if (!File.Exists(file))
                return;

            var destinationFolder = Path.GetDirectoryName(destination);
            if (destinationFolder == null)
                return;

            if (Directory.Exists(destinationFolder))
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var extension = Path.GetExtension(destination);

                var id = 0;
                while (File.Exists(destination))
                {
                    var uniqueFileName = fileName + "_" + id + extension;
                    destination = Path.Combine(destinationFolder, uniqueFileName);
                    id++;
                }
            }
            else
                Directory.CreateDirectory(destinationFolder);


            File.Move(file, destination, true);
        }
    }
}