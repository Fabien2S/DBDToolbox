using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using DeadBySounds.Debug;

namespace DeadBySounds.Sound
{
    public class SoundParser
    {
        private const string SoundBankFileName = "SoundbanksInfo.xml";

        private static readonly Logger Logger = Logger.GetLogger<SoundExtractor>();

        private XElement _soundBankInfo;

        public SoundParser(string path)
        {
            var soundBankInfoPath = Path.Combine(path, SoundBankFileName);
            if (!File.Exists(soundBankInfoPath))
            {
                Logger.Error(
                    "Missing sound banks file (Some DBD version doesn't have it, be patient and wait for the next update)");
                return;
            }

            Logger.Info("Parsing sound bank from \"{0}\"", soundBankInfoPath);
            _soundBankInfo = XElement.Load(soundBankInfoPath, LoadOptions.None);
        }

        private static bool TryParseEntry(XElement element, out SoundEntry entry)
        {
            var idAttribute = element.Attribute("Id");
            if (idAttribute == null)
            {
                Logger.Error("Missing Id attribute");
                entry = default(SoundEntry);
                return false;
            }

            var id = idAttribute.Value;

            var pathElement = element.Element("Path");
            if (pathElement == null || pathElement.IsEmpty)
            {
                Logger.Error("Missing Path element");
                entry = default(SoundEntry);
                return false;
            }

            var path = pathElement.Value;
            Logger.Info("Parsed file (Id: {0}, Path: {1})", id, path);

            entry = new SoundEntry(id, path);
            return true;
        }

        public HashSet<SoundEntry> ParseStreamedSounds()
        {
            var streamedFilesElement = _soundBankInfo.Element("StreamedFiles");
            if (streamedFilesElement == null)
            {
                Logger.Error("Missing \"StreamedFiles\" element");
                return null;
            }

            var soundEntries = new HashSet<SoundEntry>();
            foreach (var element in streamedFilesElement.Elements("File"))
            {
                if (TryParseEntry(element, out var entry))
                    soundEntries.Add(entry);
            }

            return soundEntries;
        }

        public HashSet<SoundBank> ParseBanks()
        {
            var soundBankElements = _soundBankInfo
                .Element("SoundBanks")
                ?.Elements("SoundBank");
            if (soundBankElements == null)
            {
                Logger.Error("Missing \"SoundBanks\" element");
                return null;
            }

            var soundBanks = new HashSet<SoundBank>();
            foreach (var soundBankElement in soundBankElements)
            {
                var pathElement = soundBankElement.Element("Path");
                if (pathElement == null)
                {
                    Logger.Error("Missing \"Path\" element");
                    continue;
                }

                var soundBank = new SoundBank(pathElement.Value);

                var files = soundBankElement
                    .Element("IncludedMemoryFiles")
                    ?.Elements("File");
                if (files == null)
                {
                    Logger.Error("Missing \"SoundBanks\" element");
                    continue;
                }

                foreach (var element in files)
                {
                    if (TryParseEntry(element, out var entry))
                        soundBank.Entries.Add(entry);
                }

                soundBanks.Add(soundBank);
            }

            return soundBanks;
        }
    }
}