using System;
using System.Collections.Generic;

namespace Svelto.Utilities
{
    public enum LogType
    {
        Log,
        Exception,
        Warning,
        Error,
        LogDebug
    }
    public interface ILogger
    {
        void Log(string txt, LogType type = LogType.Log, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> data = null);

        void OnLoggerAdded();
#if UNITY_2021_3_OR_NEWER        
        void CompressLogsToZipAndShow(string zipName);
#endif
    }
}