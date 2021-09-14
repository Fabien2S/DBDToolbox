using System;
using System.IO;
using System.Threading;
using DBDToolbox.Localization;
using DBDToolbox.Logging;
using DBDToolbox.Runtime.Platforms.Windows;
using DBDToolbox.Sounds;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DBDToolbox.Runtime
{
    internal static class Program
    {
        private const string DefaultInputPath = "C:/Program Files (x86)/Steam/steamapps/common/Dead by Daylight/DeadByDaylight/Content/Paks";
        private const string DefaultOutputPath = "./output";

        private static int Main(string[] args)
        {
            var currentThread = Thread.CurrentThread;
            currentThread.Name = nameof(DBDToolbox);

            var supportColors = Environment.GetEnvironmentVariable("NO_COLOR") == null;
            InitializeLogging(LogLevel.Information, supportColors);

            var inputPath = args.Length > 0 ? args[0] : DefaultInputPath;
            var outputPath = args.Length > 1 ? args[1] : DefaultOutputPath;

            if (Directory.Exists(outputPath))
            {
                if (Ask("Output directory already exists. Would you like to delete it?"))
                    Directory.Delete(outputPath, true);
                else
                    return -1;
            }

            using var toolbox = new DeadByDaylightToolbox(inputPath, outputPath);

            // Register extractors
            toolbox.RegisterExtractor<LocalizationExtractor>();
            toolbox.RegisterExtractor<SoundExtractor>();

            // Register post processors
            toolbox.RegisterPostProcessor<SoundPostProcessor>();

            toolbox.ExtractAssets();

            return 0;
        }

        private static bool Ask(string question)
        {
            Console.Write("[?]" );
            Console.WriteLine(question);
            Console.Write("(yes/no): ");
            var line = Console.ReadLine();
            return string.Equals(line, "y", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(line, "yes", StringComparison.OrdinalIgnoreCase);
        }

        private static void InitializeLogging(LogLevel minLevel, bool color)
        {
            if (OperatingSystem.IsWindows())
            {
                if (color)
                    WinConsole.DisableColors();
                else
                    WinConsole.EnableColors();
            }

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(minLevel);
                builder.AddSimpleConsole(options =>
                {
                    options.SingleLine = false;
                    options.IncludeScopes = true;
                    options.ColorBehavior = color ? LoggerColorBehavior.Default : LoggerColorBehavior.Disabled;
                    options.UseUtcTimestamp = false;
                    options.TimestampFormat = "hh:mm:ss ";
                });
            });

            LogManager.Initialize(loggerFactory);
        }
    }
}