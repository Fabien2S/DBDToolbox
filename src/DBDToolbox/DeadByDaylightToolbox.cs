using System;
using System.Collections.Generic;
using System.IO;
using DBDToolbox.Assets;
using DBDToolbox.Logging;
using DBDToolbox.Postprocessors;
using Microsoft.Extensions.Logging;
using UETools.Core;
using UETools.Core.Enums;
using UETools.Pak;

namespace DBDToolbox
{
    public class DeadByDaylightToolbox : IDisposable
    {
        private static readonly ILogger<DeadByDaylightToolbox> Logger = LogManager.Create<DeadByDaylightToolbox>();

        private readonly string _inputPath;
        private readonly string _outputPath;

        private readonly List<IAssetExtractor> _extractors;
        private readonly List<IPostProcessor> _processors;
        private readonly PakVFS _vfs;


        private bool _disposed;

        public DeadByDaylightToolbox(string inputPath, string outputPath)
        {
            _inputPath = inputPath ?? throw new ArgumentNullException(nameof(inputPath), "Input path is null");
            _outputPath = outputPath ?? throw new ArgumentNullException(nameof(outputPath), "Output path is null");

            _extractors = new List<IAssetExtractor>();
            _processors = new List<IPostProcessor>();
            _vfs = PakVFS.OpenAt(inputPath);
        }

        private void EnsureValid()
        {
            if (_disposed) throw new InvalidOperationException("Object is disposed");
        }

        public void RegisterExtractor<T>() where T : IAssetExtractor, new()
        {
            EnsureValid();
            _extractors.Add(new T());
        }

        public void RegisterPostProcessor<T>() where T : IPostProcessor, new()
        {
            EnsureValid();
            _processors.Add(new T());
        }

        public void ExtractAssets()
        {
            EnsureValid();
            var assets = new List<IAsset>();

            Logger.LogInformation("Reading assets from path \"{0}\"", _inputPath);
            foreach (var (path, entry) in _vfs.AbsoluteIndex)
            {
                var assetPath = new AssetPath(path);
                var archive = (FArchive)null;

                foreach (var extractor in _extractors)
                {
                    if (!extractor.CanExtract(assetPath)) continue;

                    Logger.LogDebug("Reading asset \"{0}\"", path);
                    archive ??= entry.Read();
                    archive.Version = UE4Version.VER_UE4_AUTOMATIC_VERSION;

                    Logger.LogInformation("Extracting asset \"{0}\" with {1}", path, extractor);
                    var asset = extractor.Extract(assetPath, entry, archive);
                    if (asset != null)
                        assets.Add(asset);
                }

                if (archive != null)
                    archive.Dispose();
                else
                    Logger.LogDebug("No extractor found for asset \"{0}\"", path);
            }

            Logger.LogInformation("Extracting assets to path \"{0}\"", _outputPath);
            foreach (var asset in assets)
            {
                var assetPath = new AssetPath(_outputPath, asset.Path);

                var assetDirectory = Path.GetDirectoryName(assetPath);
                if (assetDirectory != null)
                    Directory.CreateDirectory(assetDirectory);

                asset.Extract(assetPath);

                foreach (var postProcessor in _processors)
                {
                    if (!postProcessor.CanProcess(asset.Path))
                        continue;

                    Logger.LogInformation("Processing asset \"{0}\" with {1}", asset.Path, postProcessor);
                    postProcessor.Process(assetPath, asset);
                }
            }

            Logger.LogInformation("Extracted {0} assets", assets.Count);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            GC.SuppressFinalize(this);

            _vfs?.Dispose();
        }
    }
}