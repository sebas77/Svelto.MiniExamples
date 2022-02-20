#if !DEBUG || PROFILE_SVELTO
#define DISABLE_DEBUG
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
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
        static readonly ThreadLocal<StringBuilder> _threadSafeStrings;
        static readonly FasterList<ILogger>        _loggers;

        public static bool batchLog = false;

        static Console()
        {
            _threadSafeStrings = new ThreadLocal<StringBuilder>(() => new StringBuilder(256));
            _loggers           = new FasterList<ILogger>();

            AddLogger(new SimpleLogger());
        }

        static StringBuilder _stringBuilder
        {
            get
            {
                try
                {
                    return _threadSafeStrings.Value;
                }
                catch
                {
                    return
                        new StringBuilder(); //this is just to handle finalizer that could be called after the _threadSafeStrings is finalized. So pretty rare
                }
            }
        }

        public static void AddLogger(ILogger log)
        {
            _loggers.Add(log);

            log.OnLoggerAdded();
        }

        public static void Log(string txt)
        {
            InternalLog(txt, LogType.Log);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string txt)
        {
            InternalLog(txt, LogType.LogDebug);
        }

        [Conditional("DEBUG")]
        public static void LogDebug<T>(string txt, T extradebug)
        {
            InternalLog(txt.FastConcat(extradebug.ToString()), LogType.LogDebug);
        }

        [Conditional("DEBUG")]
        public static void LogDebugWarning(string txt)
        {
            InternalLog(txt, LogType.Warning);
        }

        [Conditional("DEBUG")]
        public static void LogDebugWarning(bool assertion, string txt)
        {
            if (assertion == false)
                InternalLog(txt, LogType.Warning);
        }

        public static void LogError(string txt, Dictionary<string, string> extraData = null)
        {
            var builder = _stringBuilder;

            builder.Length = 0;
            builder.Append("-!!!!!!-> ").Append(txt);

            var toPrint = builder.ToString();

            InternalLog(toPrint, LogType.Error, true, null, extraData);
        }

        public static void LogException(Exception exception, string message = null,
            Dictionary<string, string> extraData = null)
        {
            if (extraData == null)
                extraData = new Dictionary<string, string>();

            string toPrint = "-!!!!!!-> ";

            Exception tracingE = exception;
            while (tracingE.InnerException != null)
            {
                tracingE = tracingE.InnerException;

                InternalLog("-!!!!!!-> ", LogType.Error, true, tracingE);
            }

            if (message != null)
            {
                var builder = _stringBuilder;
                builder.Length = 0;
                builder.Append(toPrint).Append(exception.Message).Append(" -- ").Append(message);

                toPrint = builder.ToString();
            }

            //the goal of this is to show the stack from the real error
            InternalLog(toPrint, LogType.Exception, true, exception, extraData);
        }

        public static void LogWarning(string txt)
        {
            var builder = _stringBuilder;
            builder.Length = 0;
            builder.Append("------> ").Append(txt);

            var toPrint = builder.ToString();

            InternalLog(toPrint, LogType.Warning);
        }

        public static void LogStackTrace(string str, StackTrace stack)
        {
            var builder = _stringBuilder;
            builder.Length = 0;

            _stringBuilder.Append(str).Append("\n");

            for (var index1 = 0; index1 < stack.FrameCount; ++index1)
            {
                FormatStack(stack.GetFrame(index1), builder);
            }

            var toPrint = builder.ToString();

            InternalLog(toPrint, LogType.Error, false);
        }

        static void FormatStack(StackFrame frame, StringBuilder sb)
        {
            MethodBase mb = frame.GetMethod();
            if (mb == null)
                return;

            Type classType = mb.DeclaringType;
            if (classType == null)
                return;

            // Add namespace.classname:MethodName
            String ns = classType.Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                sb.Append(ns);
                sb.Append(".");
            }

            sb.Append(classType.Name);
            sb.Append(":");
            sb.Append(mb.Name);
            sb.Append("(");

            // Add parameters
            int             j           = 0;
            ParameterInfo[] pi          = mb.GetParameters();
            bool            fFirstParam = true;
            while (j < pi.Length)
            {
                if (fFirstParam == false)
                    sb.Append(", ");
                else
                    fFirstParam = false;

                sb.Append(pi[j].ParameterType.Name);
                j++;
            }

            sb.AppendLine(")");
        }

        public static void SetLogger(ILogger log)
        {
            log.OnLoggerAdded();

            _loggers[0] = log;
        }

        /// <summary>
        /// this class methods can use only InternalLog to log and cannot use the public methods, otherwise the
        /// stack depth will break 
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="type"></param>
        /// <param name="b"></param>
        /// <param name="e"></param>
        /// <param name="extraData"></param>
        static void InternalLog(string txt, LogType type, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> extraData = null)
        {
            for (int i = 0; i < _loggers.count; i++)
            {
                _loggers[i].Log(txt, type, showLogStack, e, extraData);
            }
        }
    }
}