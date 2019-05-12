using System;
using System.Collections.Generic;

namespace Svelto.Utilities
{
    public class SimpleLogger : ILogger
    {
        public void Log(string txt, LogType type = LogType.Log, Exception e = null, Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;

            string stack = null;
            
            if (e != null)
                stack = e.StackTrace;

            if (data != null)
                dataString = DataToString.DetailString(data);
            
            switch (type)
            {
                case LogType.Log:
                    Console.SystemLog(txt);
                    break;
                case LogType.Warning:
                    Console.SystemLog(txt);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    Console.SystemLog("<color=cyan> ".FastConcat(txt, "</color> ", Environment.NewLine, stack)
                                                     .FastConcat(Environment.NewLine, dataString));
                    break;
            }
        }
    }
}