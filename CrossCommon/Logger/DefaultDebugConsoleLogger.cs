using System;
using System.Collections.Generic;

namespace CrossCommon
{

    public class DefaultDebugConsoleLogger : ILogger
    {
        public void WriteLog(LoggerCategory category, string message)
        {
            string logMessage = string.Format("{0} [{1}] {2}", GetCurrentDateTimePrefix(), category.ToString(), message);
            System.Diagnostics.Debug.WriteLine(logMessage);
        }

        private static string GetCurrentDateTimePrefix()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zz");
        }
    }
    
}
