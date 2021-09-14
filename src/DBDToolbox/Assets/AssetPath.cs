using System;
using System.IO;

namespace DBDToolbox.Assets
{
    public readonly struct AssetPath
    {
        private readonly string _path;

        public AssetPath(string path)
        {
            _path = path;
        }

        public AssetPath(string root, string path)
        {
            _path = Path.Join(root, path);
        }

        public bool Contains(string value)
        {
            return _path.Contains(value, StringComparison.Ordinal);
        }

        public bool EndsWith(string value)
        {
            return _path.EndsWith(value, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return _path;
        }

        public static implicit operator string(AssetPath path)
        {
            return path._path;
        }
    }
}