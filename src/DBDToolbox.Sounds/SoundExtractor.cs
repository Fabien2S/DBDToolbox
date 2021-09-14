using DBDToolbox.Assets;
using DBDToolbox.Logging;
using DBDToolbox.Sounds.Assets;
using Microsoft.Extensions.Logging;
using UETools.Core;
using UETools.Pak;

namespace DBDToolbox.Sounds
{
    public class SoundExtractor : IAssetExtractor
    {
        // Big thanks to https://wiki.xentax.com/index.php/Wwise_SoundBank_(*.bnk)

        private static readonly ILogger<SoundExtractor> Logger = LogManager.Create<SoundExtractor>();

        private const string SoundPartialPath = "WwiseAudio";

        private const string SoundBankExtension = ".bnk";
        private const string SoundBankInfoExtension = ".xml";

        public bool CanExtract(AssetPath path)
        {
            return path.Contains(SoundPartialPath) &&
                   (
                       path.EndsWith(SoundBankExtension) ||
                       path.EndsWith(SoundBankInfoExtension)
                   );
        }

        public IAsset Extract(AssetPath path, PakEntry entry, FArchive archive)
        {
            if (path.EndsWith(SoundBankExtension))
            {
                var asset = new SoundBankAsset(path);
                asset.Serialize(archive);
                return asset;
            }

            if (path.EndsWith(SoundBankInfoExtension))
            {
                var content = TextAsset.ReadArchive(archive);
                return new SoundBankInfoAsset(path, content);
            }

            Logger.LogError("Failed to extract sound asset \"{0}\"", path);
            return null;
        }
    }
}