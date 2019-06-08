using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeadBySounds.Debug
{
    public class Logger
    {
        private static readonly ConsoleColor[] ColorByLevel =
        {
            ConsoleColor.DarkGray,
            ConsoleColor.White,
            ConsoleColor.Green,
            ConsoleColor.DarkYellow,
            ConsoleColor.Red
        };

        private static readonly Dictionary<Type, Logger> _loggers = new Dictionary<Type, Logger>();

        private readonly string _name;
        private LogLevel _level;

        private Logger(string name)
        {
            _name = name;
        }

        private void Log(LogLevel level, string message, params object[] args)
        {
            var numericalLevel = (int) level;
            if (numericalLevel < (int) _level)
                return;

            var levelType = typeof(LogLevel);
            var levelName = Enum.GetName(levelType, level);
            var date = DateTime.Now;

            var builder = new StringBuilder()
                .Append('[')
                .Append(date.ToString(CultureInfo.InvariantCulture))
                .Append(' ')
                .Append(levelName)
                .Append('/')
                .Append(_name)
                .Append(']')
                .Append(' ');

            if (args.Length > 0)
            {
                var startArg = -1;
                var argIndex = 0;

                for (var i = 0; i < message.Length; i++)
                {
                    var c = message[i];
                    if (startArg != -1)
                    {
                        if (char.IsDigit(c))
                        {
                            argIndex *= 10;
                            argIndex += (int) char.GetNumericValue(c);
                        }
                        else if (c == '}')
                        {
                            if (argIndex >= args.Length)
                                throw new ArgumentOutOfRangeException(nameof(args), argIndex,
                                    "Invalid logger argument index (" + argIndex + " >= " + args.Length + ")");

                            var arg = args[argIndex];
                            var stringArg = "null";

                            if (arg != null)
                            {
                                var argType = arg.GetType();
                                stringArg = (argType.IsArray ? string.Join(", ", (Array) arg) : arg) as string;
                            }

                            builder.Append(stringArg);

                            startArg = -1;
                            argIndex = 0;
                        }
                        else
                        {
                            i = startArg + 1;
                            startArg = -1;
                            argIndex = 0;

                            builder.Append('{');
                        }
                    }
                    else if (c == '{')
                        startArg = i;
                    else
                        builder.Append(c);
                }

                if (startArg != -1)
                    builder.Append(message.Substring(startArg));
            }
            else
                builder.Append(message);

            Console.ForegroundColor = ColorByLevel[numericalLevel];
            Console.WriteLine(builder.ToString());
            Console.ResetColor();
        }

        public void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        public void Success(string message, params object[] args)
        {
            Log(LogLevel.Success, message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Log(LogLevel.Warn, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        public static Logger GetLogger<T>(LogLevel level = LogLevel.Debug)
        {
            var type = typeof(T);
            if (_loggers.ContainsKey(type))
                return _loggers[type];

            return _loggers[type] = new Logger(type.Name)
            {
                _level = level
            };
        }
    }
}