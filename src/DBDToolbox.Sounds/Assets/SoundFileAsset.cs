using System.IO;
using DBDToolbox.Assets;

namespace DBDToolbox.Sounds.Assets
{
    public class SoundFileAsset : IAsset
    {
        public AssetPath Path { get; }

        private readonly byte[] _content;

        public SoundFileAsset(AssetPath path, byte[] content)
        {
            Path = path;
            _content = content;
        }

        public void Extract(AssetPath path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            var fileDirectory = System.IO.Path.GetDirectoryName(path);

            var outputDirectory = System.IO.Path.Join(fileDirectory, "_StreamedFiles");
            Directory.CreateDirectory(outputDirectory);

            var filePath = System.IO.Path.Join(outputDirectory, fileName);
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            fileStream.Write(_content);
        }
    }
}