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
using System.Threading;
using Svelto.DataStructures;
using Svelto.Utilities;
using ILogger = Svelto.Utilities.ILogger;
using LogType = Svelto.Utilities.LogType;

namespace Svelto
{
    public static class Console
    {
        static readonly ThreadLocal<StringBuilder> _stringBuilder =
            new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

        static readonly FasterList<DataStructures.WeakReference<ILogger>> _loggers;

        static ILogger _standardLogger;

        static Console()
        {
            _loggers = new FasterList<DataStructures.WeakReference<ILogger>>();

            AddLogger(new SimpleLogger());
        }

        public static void SetLogger(ILogger log)
        {
            _loggers[0] = new DataStructures.WeakReference<ILogger>(log);
            log.OnLoggerAdded();
        }

        public static void AddLogger(ILogger log)
        {
            _loggers.Add(new DataStructures.WeakReference<ILogger>(log));
            log.OnLoggerAdded();
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
            _stringBuilder.Value.Length = 0;
            _stringBuilder.Value.Append("-!!!!!!-> ");
            _stringBuilder.Value.Append(txt);

            var toPrint = _stringBuilder.ToString();

            Log(toPrint, LogType.Error, null, extraData);
        }

        public static void LogException(Exception e, Dictionary<string, string> extraData = null)
        {
            LogException(String.Empty, e, extraData);
        }

        public static void LogException(string message, Exception exception,
            Dictionary<string, string> extraData = null)
        {
            if (extraData == null)
                extraData = new Dictionary<string, string>();

            Exception tracingE = exception;
            while (tracingE.InnerException != null)
            {
                tracingE = tracingE.InnerException;

                Log(message, LogType.Exception, tracingE, extraData);
            }

            throw exception;
        }

        public static void LogWarning(string txt)
        {
            _stringBuilder.Value.Length = 0;
            _stringBuilder.Value.Append("------> ");
            _stringBuilder.Value.Append(txt);

            var toPrint = _stringBuilder.ToString();

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

#if NETFX_CORE
                string currentTimeString = DateTime.UtcNow.ToString("dd/mm/yy hh:ii:ss");
                string processTimeString = (DateTime.UtcNow - ProcessDiagnosticInfo.
                                                GetForCurrentProcess().ProcessStartTime.DateTime.ToUniversalTime()).ToString();
#else
            string currentTimeString = DateTime.UtcNow.ToLongTimeString(); //ensure includes seconds
            string processTimeString =
                (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).ToString();
#endif

            _stringBuilder.Value.Length = 0;
            _stringBuilder.Value.Append("[").Append(currentTimeString);
            _stringBuilder.Value.Append("][").Append(processTimeString);
            _stringBuilder.Value.Length =
                _stringBuilder.Value.Length - 3; //remove some precision that we don't need
            _stringBuilder.Value.Append("] ").AppendLine(txt);

            toPrint = _stringBuilder.ToString();

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