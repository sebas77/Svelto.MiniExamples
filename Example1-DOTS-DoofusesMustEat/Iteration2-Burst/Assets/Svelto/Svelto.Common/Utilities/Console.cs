#if !DEBUG || PROFILER
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
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto
{
    public static class Console
    {
        static readonly StringBuilder                                     _stringBuilder = new StringBuilder(256);
        static readonly FasterList<DataStructures.WeakReference<ILogger>> _loggers;

        static readonly ILogger _standardLogger;

        static Console()
        {
            _loggers = new FasterList<DataStructures.WeakReference<ILogger>>();

#if UNITY_5_3_OR_NEWER || UNITY_5
            _standardLogger = new SlowUnityLogger();
#else
            _standardLogger = new SimpleLogger();
#endif
            _standardLogger.OnLoggerAdded();

            _loggers.Add(new DataStructures.WeakReference<ILogger>(_standardLogger));
        }

        public static void SetLogger(ILogger log)
        {
            _loggers[0] = new DataStructures.WeakReference<ILogger>(log);
        }

        public static void AddLogger(ILogger log)
        {
            log.OnLoggerAdded();

            _loggers.Add(new DataStructures.WeakReference<ILogger>(log));
        }

        static void Log(string txt, LogType type, Exception e = null, Dictionary<string, string> extraData = null)
        {
            for (int i = 0; i < _loggers.Count; i++)
            {
                if (_loggers[i].IsValid == true)
                    _loggers[i].Target.Log(txt, type, e, extraData);
                else
                {
                    _loggers.UnorderedRemoveAt(i);
                    i--;
                }
            }
        }

        public static void Log(string txt)
        {
            Log(txt, LogType.Log);
        }

        public static void LogError(string txt, Dictionary<string, string> extraData = null)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            Log(toPrint, LogType.Error, null, extraData);
        }

        public static void LogException(Exception e, Dictionary<string, string> extraData = null)
        {
            LogException(String.Empty, e, extraData);
        }

        public static void LogException(string message, Exception exception, Dictionary<string, string> extraData = null)
        {
            if (extraData == null)
                extraData = new Dictionary<string, string>();

            lock (_stringBuilder)
            {
                Exception tracingE = exception;
                while (tracingE.InnerException != null)
                {
                    tracingE = tracingE.InnerException;

                    Log(message, LogType.Exception, tracingE, extraData);
                }
            }

            throw exception;
        }

        public static void LogWarning(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("------> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            Log(toPrint, LogType.Warning);
        }

#if DISABLE_DEBUG
		[Conditional("__NEVER_DEFINED__")]
#endif
        public static void LogDebug(string txt)
        {
            Log("<i><color=teal> ".FastConcat(txt, "</color></i>"), LogType.Log);
        }

#if DISABLE_DEBUG
        [Conditional("__NEVER_DEFINED__")]
#endif
        public static void LogDebug<T>(string txt, T extradebug)
        {
            Log("<i><color=teal> ".FastConcat(txt, " - ", extradebug.ToString(), "</color></i>"), LogType.Log);
        }

        /// <summary>
        /// Use this function if you don't want the message to be batched
        /// </summary>
        /// <param name="txt"></param>
        public static void SystemLog(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
#if NETFX_CORE
                string currentTimeString = DateTime.UtcNow.ToString("dd/mm/yy hh:ii:ss");
                string processTimeString = (DateTime.UtcNow - ProcessDiagnosticInfo.
                                                GetForCurrentProcess().ProcessStartTime.DateTime.ToUniversalTime()).ToString();
#else
                string currentTimeString = DateTime.UtcNow.ToLongTimeString(); //ensure includes seconds
                string processTimeString =
                    (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString();
#endif

                _stringBuilder.Length = 0;
                _stringBuilder.Append("[").Append(currentTimeString);
                _stringBuilder.Append("][").Append(processTimeString);
                _stringBuilder.Length = _stringBuilder.Length - 3; //remove some precision that we don't need
                _stringBuilder.Append("] ").AppendLine(txt);

                toPrint = _stringBuilder.ToString();
            }

#if !UNITY_EDITOR
#if !NETFX_CORE
            System.Console.WriteLine(toPrint);
#else
            //find a way to adopt a logger externally, if this is still needed
#endif
#else
            UnityEngine.Debug.Log(toPrint);
#endif
        }
    }
}