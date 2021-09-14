using System.IO;
using System.Text;
using UETools.Core;

namespace DBDToolbox.Assets
{
    public class TextAsset : IAsset
    {
        public AssetPath Path { get; }

        private readonly string _content;

        public TextAsset(AssetPath path, string content)
        {
            Path = path;
            _content = content;
        }

        public void Extract(AssetPath path)
        {
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var streamWriter = new StreamWriter(fileStream);
            streamWriter.Write(_content);
        }

        public static string ReadArchive(FArchive archive)
        {
            var bytes = BinaryAsset.ReadArchive(archive);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}