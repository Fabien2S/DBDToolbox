using System.Collections.Generic;

namespace DeadBySounds.Sound
{
    public struct SoundBank
    {
        public readonly string Path;
        public readonly HashSet<SoundEntry> Entries;

        public SoundBank(string path)
        {
            Path = path;
            Entries = new HashSet<SoundEntry>();
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}