using DBDToolbox.Assets;

namespace DBDToolbox.Postprocessors
{
    public interface IPostProcessor
    {
        bool CanProcess(AssetPath path);
        void Process(AssetPath path, IAsset asset);
    }
}