namespace DBDToolbox.Assets
{
    public interface IAsset
    {
        AssetPath Path { get; }

        void Extract(AssetPath path);
    }
}