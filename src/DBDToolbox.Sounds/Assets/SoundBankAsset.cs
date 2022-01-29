using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DBDToolbox.Assets;
using DBDToolbox.Logging;
using Microsoft.Extensions.Logging;
using UETools.Core;
using UETools.Core.Interfaces;

namespace DBDToolbox.Sounds.Assets
{
    public partial class SoundBankAsset : IAsset, IUnrealSerializable
    {
        private static readonly ILogger<SoundBankAsset> Logger = LogManager.Create<SoundBankAsset>();

        public uint BankId => ((SoundBankHeaderSection)_sections[SoundBankHeaderSection.Identifier]).Id;

        public AssetPath Path { get; }

        private readonly Dictionary<string, SoundBankSection> _sections = new();

        public SoundBankAsset(AssetPath path)
        {
            Path = path;
        }

        public FArchive Serialize(FArchive archive)
        {
            const int magicSize = 4;
            var magicBuffer = (Span<byte>)new byte[magicSize];

            while (!archive.EOF())
            {
                archive.Read(ref magicBuffer, magicSize);
                var magicId = Encoding.ASCII.GetString(magicBuffer);
                if (magicId.Length != magicSize)
                    throw new InvalidOperationException($"Magic ID is not {magicSize} char long!");

                var length = 0U;
                archive.Read(ref length);
                var offset = archive.Tell();

                if (TryCreateSection(magicId, offset, length, out var section))
                {
                    Logger.LogDebug("Reading section {section} from {archive}", section, archive);
                    section.Serialize(archive);

                    var left = offset + length - archive.Tell();
                    switch (left)
                    {
                        case < 0:
                            Logger.LogError("Section {section} read too many bytes", section);
                            break;
                        case > 0:
                            Logger.LogDebug("Section {section} left some bytes unread", section);
                            archive.Skip(left);
                            break;
                    }

                    _sections.Add(magicId, section);
                }
                else
                {
                    // Unable to read asset
                    Logger.LogDebug("Failed to read section at offset {offset} of length {length} from {archive}",
                        offset, length, archive
                    );
                    archive.Skip(length);
                }
            }

            return archive;
        }

        public void Extract(AssetPath path)
        {
            Directory.CreateDirectory(path);

            if (!_sections.ContainsKey(SoundBankDataIndexSection.Identifier))
                return;
            if (!_sections.ContainsKey(SoundBankDataSection.Identifier))
                return;

            var dataIndex = (SoundBankDataIndexSection) _sections[SoundBankDataIndexSection.Identifier];
            var data = (SoundBankDataSection)_sections[SoundBankDataSection.Identifier];

            foreach (var fileIndex in dataIndex.FileIndices)
            {
                var filePath = System.IO.Path.Join(path, fileIndex.FileName);
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                var fileContent = data.GetFile(fileIndex.Offset, fileIndex.Length);
                fileStream.Write(fileContent);
            }
        }
    }
}