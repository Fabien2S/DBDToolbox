using System;
using System.IO;
using System.Text;

namespace DeadBySounds
{
    public static class FileHelper
    {
        private static readonly Random Random = new Random();

        private static readonly char[] Characters =
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
            'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5'
        };

        private static string RandomFileName(int len = 10)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < len; i++)
            {
                var index = Random.Next(Characters.Length);
                var c = Characters[index];
                builder.Append(c);
            }

            return builder.ToString();
        }

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
                    var randomFileName = RandomFileName() + extension;
                    destination = Path.Combine(destinationFolder, randomFileName);
                }
            }
            else
                Directory.CreateDirectory(destinationFolder);


            File.Move(file, destination);
        }
    }
}