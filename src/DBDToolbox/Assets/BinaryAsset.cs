using System;
using System.IO;
using UETools.Core;

namespace DBDToolbox.Assets
{
    public class BinaryAsset : IAsset
    {
        public AssetPath Path { get; }
        public ReadOnlySpan<byte> Content => _content.AsSpan();

        private readonly byte[] _content;

        public BinaryAsset(AssetPath path, byte[] content)
        {
            Path = path;

            _content = content;
        }

        public void Extract(AssetPath path)
        {
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            fileStream.Write(_content);
        }

        public static byte[] ReadArchive(FArchive archive)
        {
            var length = archive.Length();
            var buffer = new byte[length];
            archive.Read(ref buffer, (int)length);
            if (buffer == null)
                throw new IOException("Failed to read archive content");
            return buffer;
        }
    }
}