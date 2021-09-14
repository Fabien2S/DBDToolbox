using DBDToolbox.Assets;

namespace DBDToolbox.Postprocessors
{
    public interface IPostProcessor
    {
        bool CanProcess(AssetPath path);
        void Process(string outputPath, AssetPath path, IAsset asset);
    }
}