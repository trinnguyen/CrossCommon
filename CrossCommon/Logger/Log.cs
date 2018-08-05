using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrossCommon
{
    public static class Log
    {
        private static List<ILogger> _lstLoggers = new List<ILogger>();

        static Log()
        {
            _lstLoggers = new List<ILogger>
            {
                new DefaultDebugConsoleLogger()
            };
        }

        /// <summary>
        /// Registers the loggers.
        /// </summary>
        /// <param name="loggers">Loggers.</param>
        public static void RegisterLoggers(params ILogger[] loggers)
        {
            if (loggers != null && loggers.Length > 0)
            {
                _lstLoggers.Clear();
                _lstLoggers.AddRange(loggers);
            }
        }

        [Conditional("DEBUG")]
        public static void Debug(string message, [CallerFilePath] string filePath = "")
        {
            InternalWriteLog(LoggerCategory.Debug, GetMessage(message, filePath));
        }

        [Conditional("DEBUG")]
        public static void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public static void Info(string message, [CallerFilePath] string filePath = "")
        {
            InternalWriteLog(LoggerCategory.Info, GetMessage(message, filePath));
        }

        public static void Info(string message, params object[] args)
        {
            Info(string.Format(message, args));
        }

        public static void Error(string message, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            InternalWriteLog(LoggerCategory.Error, GetMessage(message, filePath, memberName));
        }

        public static void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public static void Exception(Exception exception, [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "")
        {
            while (exception != null && exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            if (exception != null)
            {
                string message = string.Format("Exception: {0} \n {1}", exception.Message, exception.StackTrace);
                Error(message, filePath, memberName);
            }
        }

        /// <summary>
        /// Log the specified category and message.
        /// Example: 2017-05-24 19:40:55.025 +07:00 [Info] message
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="category">Category.</param>
        /// <param name="message">Message.</param>
        private static void InternalWriteLog(LoggerCategory category, string message)
        {
            //string logMessage = string.Format("{0} [{1}] {2}", GetCurrentDateTimePrefix(), category.ToString(), message);
            //System.Diagnostics.Debug.WriteLine(logMessage);
            foreach (var logger in _lstLoggers)
            {
                logger.WriteLog(category, message);
            }
        }

        private static string GetMessage(string message, string filePath, string memberName = "")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return message;

            string prefix = System.IO.Path.GetFileNameWithoutExtension(filePath);
            if (!string.IsNullOrWhiteSpace(memberName))
            {
                prefix += $"->{memberName}";
            }
            return $"{prefix}: {message}";
        }
    }
}