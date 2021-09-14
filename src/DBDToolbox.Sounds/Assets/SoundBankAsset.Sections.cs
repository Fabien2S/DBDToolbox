using System;
using System.Collections.Generic;
using System.Globalization;
using UETools.Core;
using UETools.Core.Interfaces;

namespace DBDToolbox.Sounds.Assets
{
    public partial class SoundBankAsset
    {
        private bool TryCreateSection(
            string magic, long offset, long length,
            out SoundBankSection section
        )
        {
            if (_sections.ContainsKey(magic))
                throw new InvalidOperationException($"Duplicates section {magic}");

            if (magic.Equals(SoundBankHeaderSection.Identifier, StringComparison.Ordinal))
            {
                section = new SoundBankHeaderSection(offset, length);
                return true;
            }

            if (magic.Equals(SoundBankDataIndexSection.Identifier, StringComparison.Ordinal))
            {
                section = new SoundBankDataIndexSection(offset, length);
                return true;
            }

            if (magic.Equals(SoundBankDataSection.Identifier, StringComparison.Ordinal))
            {
                section = new SoundBankDataSection(offset, length);
                return true;
            }

            section = default;
            return false;
        }

        private abstract class SoundBankSection : IUnrealSerializable
        {
            public long Offset { get; }
            public long Length { get; }

            protected SoundBankSection(long offset, long length)
            {
                Offset = offset;
                Length = length;
            }

            public abstract FArchive Serialize(FArchive archive);
        }

        private class SoundBankHeaderSection : SoundBankSection
        {
            public const string Identifier = "BKHD";

            public uint Id => _id;

            private uint _version;
            private uint _id;

            public SoundBankHeaderSection(long offset, long length) : base(offset, length)
            {
            }

            public override FArchive Serialize(FArchive archive)
            {
                return archive
                    .Read(ref _version)
                    .Read(ref _id);
            }
        }

        private class SoundBankDataIndexSection : SoundBankSection
        {
            public const string Identifier = "DIDX";

            public IReadOnlyList<FileIndex> FileIndices => _fileIndices;

            private FileIndex[] _fileIndices;

            public SoundBankDataIndexSection(long offset, long length) : base(offset, length)
            {
            }

            public override FArchive Serialize(FArchive archive)
            {
                const int fileIndexSize = 0xC;
                var trackCount = Length / fileIndexSize;
                _fileIndices = new FileIndex[trackCount];
                for (var i = 0; i < _fileIndices.Length; i++)
                {
                    var id = 0U;
                    var offset = 0U;
                    var length = 0U;

                    archive
                        .Read(ref id)
                        .Read(ref offset)
                        .Read(ref length);

                    _fileIndices[i] = new FileIndex(id, offset, length);
                }

                return archive;
            }

            public class FileIndex
            {
                public string FileName => Id.ToString("D4", CultureInfo.InvariantCulture) + ".wem";

                public uint Id { get; }
                public uint Offset { get; }
                public uint Length { get; }

                public FileIndex(uint id, uint offset, uint length)
                {
                    Id = id;
                    Offset = offset;
                    Length = length;
                }
            }
        }

        private class SoundBankDataSection : SoundBankSection
        {
            public const string Identifier = "DATA";

            private byte[] _buffer;

            public SoundBankDataSection(long offset, long length) : base(offset, length)
            {
            }

            public override FArchive Serialize(FArchive archive)
            {
                return archive.Read(ref _buffer, (int)Length);
            }

            public ReadOnlySpan<byte> GetFile(uint offset, uint length)
            {
                return _buffer.AsSpan((int)offset, (int)length);
            }
        }
    }
}