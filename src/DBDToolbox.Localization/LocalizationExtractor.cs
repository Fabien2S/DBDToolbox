using System.CodeDom.Compiler;
using System.IO;
using DBDToolbox.Assets;
using UETools.Assets;
using UETools.Core;
using UETools.Pak;

namespace DBDToolbox.Localization
{
    public class LocalizationExtractor : IAssetExtractor
    {
        private const string LocalizationAssetExtension = ".locres";

        public bool CanExtract(AssetPath path)
        {
            return path.EndsWith(LocalizationAssetExtension);
        }

        public IAsset Extract(AssetPath path, PakEntry entry, FArchive archive)
        {
            var asset = new LocResAsset();
            archive.Read(ref asset);

            var stringWriter = new StringWriter();
            var indentedWriter = new IndentedTextWriter(stringWriter);
            asset.ReadTo(indentedWriter);

            var localizationContent = stringWriter.ToString();
            return new LocalizationAsset(path, localizationContent);
        }
    }
}