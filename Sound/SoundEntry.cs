using System;

namespace DeadBySounds.Sound
{
    public struct SoundEntry : IEquatable<SoundEntry>
    {
        public readonly string Id;
        public readonly string Path;

        public SoundEntry(string id, string path)
        {
            Id = id;
            Path = path;
        }

        public bool Equals(SoundEntry other)
        {
            return string.Equals(Id, other.Id) && string.Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SoundEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}