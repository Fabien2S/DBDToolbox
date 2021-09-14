using UETools.Core;
using UETools.Pak;

namespace DBDToolbox.Assets
{
    public interface IAssetExtractor
    {
        bool CanExtract(AssetPath path);

        IAsset Extract(AssetPath path, PakEntry entry, FArchive archive);
    }
}