#if !DEBUG || PROFILE_SVELTO
#define DISABLE_DEBUG
#endif
using System;
using System.Collections.Generic;
#if NETFX_CORE
using Windows.System.Diagnostics;
#else
using System.Diagnostics;
#endif
using System.Text;
using System.Threading;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto
{
    public static class Console
    {
        static readonly ThreadLocal<StringBuilder> _stringBuilder = new ThreadLocal<StringBuilder>
            (() => new StringBuilder(256));

        static readonly FasterList<ILogger> _loggers = new FasterList<ILogger>();

        static readonly ILogger _standardLogger = new SimpleLogger();

        static Console() { AddLogger(_standardLogger); }

        public static void SetLogger(ILogger log)
        {
            _loggers[0] = log;

            log.OnLoggerAdded();
        }

        public static void AddLogger(ILogger log)
        {
            _loggers.Add(log);

            log.OnLoggerAdded();
        }

        public static void Log(string txt) { InternalLog(txt, LogType.Log); }

        public static void LogError(string txt, Dictionary<string, string> extraData = null)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Value.Length = 0;
                _stringBuilder.Value.Append("-!!!!!!-> ").Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            InternalLog(toPrint, LogType.Error, null, extraData);
        }

        public static void LogException(Exception exception, string message = null
                                      , Dictionary<string, string> extraData = null)
        {
            if (extraData == null)
                extraData = new Dictionary<string, string>();

            string toPrint = "-!!!!!!-> ";

            Exception tracingE = exception;
            while (tracingE.InnerException != null)
            {
                tracingE = tracingE.InnerException;

                InternalLog("-!!!!!!-> ", LogType.Error, tracingE);
            }

            if (message != null)
            {
                lock (_stringBuilder)
                {
                    _stringBuilder.Value.Length = 0;
                    _stringBuilder.Value.Append(toPrint).Append(message);

                    toPrint = _stringBuilder.ToString();
                }
            }

            //the goal of this is to show the stack from the real error
            InternalLog(toPrint, LogType.Exception, exception, extraData);
        }

        public static void LogWarning(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Value.Length = 0;
                _stringBuilder.Value.Append("------> ").Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            InternalLog(toPrint, LogType.Warning);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string txt) { InternalLog(txt, LogType.Log); }

        [Conditional("DEBUG")]
        public static void LogDebug<T>(string txt, T extradebug)
        {
            InternalLog(txt.FastConcat(extradebug.ToString()), LogType.Log);
        }

        /// <summary>
        /// this class methods can use only InternalLog to log and cannot use the public methods, otherwise the
        /// stack depth will break 
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="type"></param>
        /// <param name="e"></param>
        /// <param name="extraData"></param>
        static void InternalLog(string txt, LogType type, Exception e = null
                              , Dictionary<string, string> extraData = null)
        {
            for (int i = 0; i < _loggers.count; i++)
            {
                _loggers[i].Log(txt, type, e, extraData);
            }
        }
    }
}