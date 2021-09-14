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
                var extension = Path.GetExtension(destination);
                while (File.Exists(destination))
                {
                    var randomFileName = Path.GetRandomFileName();
                    var possibleFileName = Path.ChangeExtension(randomFileName, extension);
                    destination = Path.Combine(destinationFolder, possibleFileName);
                }
            }
            else
                Directory.CreateDirectory(destinationFolder);


            File.Move(file, destination, true);
        }
    }
}