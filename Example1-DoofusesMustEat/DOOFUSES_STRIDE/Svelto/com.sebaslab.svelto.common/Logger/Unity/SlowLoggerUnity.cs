#if UNITY_5_3_OR_NEWER || UNITY_5
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Svelto.Utilities
{
    public class SlowUnityLogger : ILogger
    {
#if UNITY_2018_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
#endif
        static void Init()
        {
            StringBuilder ValueFactory() => new StringBuilder();

            stringBuilder = new ThreadLocal<StringBuilder>(ValueFactory);

            Console.SetLogger(new SlowUnityLogger());
        }

        public void Log(string txt, LogType type = LogType.Log, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            var    currentManagedThreadId = Environment.CurrentManagedThreadId;
             var frame = $"[{DateTime.UtcNow.ToString("HH:mm:ss.fff")}] thread: {Environment.CurrentManagedThreadId}";
            try
            {
                if (MAINTHREADID == currentManagedThreadId)
                {
                    frame += $" frame: {Time.frameCount}";
                }
            }
            catch
            {
                //there is something wrong with  Environment.CurrentManagedThreadId
            }
#if UNITY_EDITOR
            var stackTrace = new StackTrace(3, true);
#else
            StackTrace stackTrace = null;
            
            if (type == LogType.Error || type == LogType.Exception)
                stackTrace = new StackTrace(3, true);
#endif

            switch (type)
            {
                case LogType.Log:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
                        
                        Debug.Log(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                        
                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, log);
                    }
                    else
                        Debug.Log(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                    break;
                }
                case LogType.LogDebug:
                {
#if UNITY_EDITOR                    
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
                        
                        Debug.Log(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                        
                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, log);
                    }
                    else
                        Debug.Log(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
#endif                    
                    break;
                }
                case LogType.Warning:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
                        
                        Debug.LogWarning(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                        
                        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, log);
                    }
                    else
                        Debug.LogWarning(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                    break;
                }
                case LogType.Error:
                case LogType.Exception:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
                        
                        UnityEngine.Debug.LogError(LogFormatter(txt, type, showLogStack, e, frame, dataString, stackTrace));
                        
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, log);
                    }
                    else
                        Debug.LogError($"{frame} <b><color=teal>".FastConcat(txt, "</color></b> ", Environment.NewLine)
                           .FastConcat(dataString));
                    break;
                }
            }
        }

        public static string LogFormatter(string txt, LogType type, bool showLogStack, Exception e, string frame,
            string dataString, StackTrace stackTrace)
        {
            string stack;

            switch (type)
            {
                case LogType.Log:
                {
#if UNITY_EDITOR
                    stack = showLogStack ? ExtractFormattedStackTrace(stackTrace) : string.Empty;

                    return ($"{frame} <b><color=teal> ".FastConcat(txt, " </color></b> ", Environment.NewLine, stack)
                       .FastConcat(Environment.NewLine, dataString));
#else
                    return ($"{frame} ".FastConcat(txt).FastConcat(Environment.NewLine, dataString));
#endif
                }
                case LogType.LogDebug:
                {
#if UNITY_EDITOR
                    stack = showLogStack ? ExtractFormattedStackTrace(stackTrace) : string.Empty;
                    
                    stack = ($"{frame} <b><color=yellow> ".FastConcat(txt, " </color></b> ", Environment.NewLine, stack)
                       .FastConcat(Environment.NewLine, dataString));
#else
                    return "";
#endif
                    return stack;
                }
                case LogType.Warning:
                {
#if UNITY_EDITOR
                    stack = showLogStack ? ExtractFormattedStackTrace(stackTrace) : string.Empty;
                    
                    stack = ($"{frame} <b><color=orange> "
                       .FastConcat(txt, " </color></b> ", Environment.NewLine, stack)
                       .FastConcat(Environment.NewLine, dataString));

                    return stack;
#else
                   return ($"!! {frame} ".FastConcat(txt).FastConcat(Environment.NewLine, dataString));
#endif
                }
                case LogType.Error:
                case LogType.Exception:
                {
                    if (e != null)
                    {
                        txt   = txt.FastConcat(" ", e.Message);
                        stack = ExtractFormattedStackTrace(new StackTrace(e, true), stackTrace);
                    }
                    else
                        stack = showLogStack ? ExtractFormattedStackTrace(stackTrace) : string.Empty;

#if UNITY_EDITOR
                   stack = ($"{frame} ".FastConcat(txt, " ", Environment.NewLine, stack)
                           .FastConcat(Environment.NewLine, dataString));
#else
                   return($"!!! {frame} "
                           .FastConcat(txt, Environment.NewLine, stack)
                           .FastConcat(dataString));
#endif
                    return stack;
                }
            }

            throw new NotImplementedException();
        }

        public void OnLoggerAdded()
        {
            MAINTHREADID  = Environment.CurrentManagedThreadId;
            projectFolder = Application.dataPath.Replace("Assets", "");
#if !UNITY_EDITOR
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
#endif
            Console.Log("Slow Unity Logger added");
        }

        /// <summary>
        ///     copied from Unity source code, whatever....
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        static string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            var builder = _stringBuilder;

            builder.Length = 0;
            
            PrintStack(stackTrace, builder);

            return builder.ToString();
        }
        
        static string ExtractFormattedStackTrace(StackTrace stackTrace, StackTrace stackTrace1)
        {
            return ExtractFormattedStackTrace(stackTrace).FastConcat(Environment.NewLine, "---------------", Environment.NewLine)
               .FastConcat(ExtractFormattedStackTrace(stackTrace1));
        }

        static void PrintStack(StackTrace stackTrace, StringBuilder builder)
        {
            var frameCount = Math.Min(MAX_NUMBER_OF_STACK_LINES, stackTrace.FrameCount);
            
            for (var index1 = 0; index1 < frameCount; ++index1)
            {
                FormatStack(stackTrace.GetFrame(index1), builder);
            }
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

            sb.Append(")");

            // Add path name and line number - unless it is a Debug.Log call, then we are only interested
            // in the calling frame.
            string path = frame.GetFileName();
            if (path != null)
            {
                bool shouldStripLineNumbers = (classType.Name == "Debug" && classType.Namespace == "UnityEngine") ||
                    (classType.Name == "Logger" && classType.Namespace == "UnityEngine") ||
                    (classType.Name == "DebugLogHandler" && classType.Namespace == "UnityEngine") ||
                    (classType.Name == "Assert" && classType.Namespace == "UnityEngine.Assertions") ||
                    (mb.Name == "print" && classType.Name == "MonoBehaviour" && classType.Namespace == "UnityEngine");

                if (!shouldStripLineNumbers)
                {
                    sb.Append(" (at");

                    if (!string.IsNullOrEmpty(projectFolder))
                    {
                        if (path.Replace("\\", "/").StartsWith(projectFolder))
                        {
                            path = path.Substring(projectFolder.Length, path.Length - projectFolder.Length);
                        }
                    }

#if UNITY_EDITOR
                    sb.Append($" <a href=\"{path}\" line=\"{frame.GetFileLineNumber()}\">");
#endif                    
                    sb.Append(path);
                    sb.Append(":");
                    sb.Append(frame.GetFileLineNumber().ToString());
#if UNITY_EDITOR                    
                    sb.Append("</a>)");
#else                    
                    sb.Append(")");
#endif                    
                }
            }

            sb.Append("\n");
        }

        static StringBuilder _stringBuilder
        {
            get
            {
                try
                {
                    return stringBuilder.Value;
                }
                catch
                {
                    return
                        new StringBuilder(); //this is just to handle finalizer that could be called after the _threadSafeStrings is finalized. So pretty rare
                }
            }
        }

        static ThreadLocal<StringBuilder> stringBuilder;

        static string projectFolder;
        static int    MAINTHREADID;
        static int    MAX_NUMBER_OF_STACK_LINES = 15;

    }
}
#endif