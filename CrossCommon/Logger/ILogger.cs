using System;
using System.Collections.Generic;

namespace CrossCommon
{
    public interface ILogger
    {
        void WriteLog(LoggerCategory category, string message);
    }
    
}
