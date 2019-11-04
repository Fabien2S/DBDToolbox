using System.IO;

namespace DeadBySounds
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
                while (File.Exists(destination))
                    destination = destinationFolder + Path.GetRandomFileName();
            }
            else
                Directory.CreateDirectory(destinationFolder);


            File.Move(file, destination);
        }
    }
}