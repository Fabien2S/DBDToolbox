using System;
using Microsoft.Extensions.Logging;

namespace DBDToolbox.Logging
{
    public static class LogManager
    {
        private static ILoggerFactory _factory;

        public static void Initialize(ILoggerFactory factory)
        {
            if (_factory != null) throw new InvalidOperationException("Logger factory already initialized");
            _factory = factory;
        }

        public static ILogger Create(Type type)
        {
            return _factory.CreateLogger(type);
        }

        public static ILogger<T> Create<T>()
        {
            return _factory.CreateLogger<T>();
        }

        public static void Dispose()
        {
            _factory.Dispose();
        }
    }
}