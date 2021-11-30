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
            Thread.VolatileWrite(ref MAINTHREADID, Environment.CurrentManagedThreadId);

            StringBuilder ValueFactory() => new StringBuilder();

            stringBuilder = new ThreadLocal<StringBuilder>(ValueFactory);

            Console.SetLogger(new SlowUnityLogger());
        }

        public void Log
            (string txt, LogType type = LogType.Log, Exception e = null, Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            string stack;

#if UNITY_EDITOR
            string frame = $"thread: {Environment.CurrentManagedThreadId}";
            try
            {
                if (MAINTHREADID == Environment.CurrentManagedThreadId)
                {
                    frame += $" frame: {Time.frameCount}";
                }
            }
            catch
            {
                //there is something wrong with  Environment.CurrentManagedThreadId
            }
#endif

            switch (type)
            {
                case LogType.Log:
                {
#if UNITY_EDITOR
                    stack = ExtractFormattedStackTrace();

                    Debug.Log($"{frame} <b><color=teal>".FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                                                        .FastConcat(Environment.NewLine, dataString));
#else
                    Debug.Log(txt);
#endif
                    break;
                }
                case LogType.LogDebug:
                {
#if UNITY_EDITOR
                    stack = ExtractFormattedStackTrace();

                    if (MAINTHREADID == Environment.CurrentManagedThreadId)
                    {
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
                        Debug.Log($"{frame} <b><color=orange>"
                                 .FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                                 .FastConcat(Environment.NewLine, dataString));
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, log);
                    }
                    else
                        Debug.Log(txt);

#else
                    Debug.Log(txt);
#endif
                    break;
                }
                case LogType.Warning:
                {
#if UNITY_EDITOR
                    stack = ExtractFormattedStackTrace();

                    if (MAINTHREADID == Environment.CurrentManagedThreadId)
                    {
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Warning);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
                        Debug.LogWarning($"{frame} <b><color=yellow>"
                                        .FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                                        .FastConcat(Environment.NewLine, dataString));
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, log);
                    }
                    else
                        Debug.LogWarning(txt);
#else
                    Debug.LogWarning(txt);
#endif
                    break;
                }
                case LogType.Error:
                case LogType.Exception:
                {
                    if (e != null)
                    {
                        txt   = txt.FastConcat(e.Message);
                        stack = ExtractFormattedStackTrace(new StackTrace(e, true));
                    }
                    else
                        stack = ExtractFormattedStackTrace();

#if UNITY_EDITOR
                    if (MAINTHREADID == Environment.CurrentManagedThreadId)
                    {
                        var error = Application.GetStackTraceLogType(UnityEngine.LogType.Error);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
                        Debug.LogError($"{frame} <b><color=red>"
                                      .FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                                      .FastConcat(Environment.NewLine, dataString));
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, error);
                    }
                    else
                        Debug.LogError(txt);
#else
                    if (type == LogType.Error)
                        Debug.LogError(txt);
                    else
                    if (e != null)
                        Debug.LogException(e);
#endif
                    break;
                }
            }
        }

        public void OnLoggerAdded()
        {
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
        string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            var builder = _stringBuilder;
            
            builder.Length = 0;

            var frame = new StackTrace(4, true);

            for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
            {
                FormatStack(stackTrace, index1, builder);
            }

            for (var index1 = 0; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, builder);
            }

            return builder.ToString();
        }

        string ExtractFormattedStackTrace()
        {
            var builder = _stringBuilder;
            
            builder.Length = 0;

            var frame = new StackTrace(4, true);

            for (var index1 = 0; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, builder);
            }

            return builder.ToString();
        }

        void FormatStack(StackTrace stackTrace, int iIndex, StringBuilder sb)
        {
            StackFrame frame = stackTrace.GetFrame(iIndex);

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
                bool shouldStripLineNumbers = (classType.Name == "Debug" && classType.Namespace == "UnityEngine")
                                           || (classType.Name == "Logger" && classType.Namespace == "UnityEngine")
                                           || (classType.Name == "DebugLogHandler"
                                            && classType.Namespace == "UnityEngine")
                                           || (classType.Name == "Assert"
                                            && classType.Namespace == "UnityEngine.Assertions") || (mb.Name == "print"
                                               && classType.Name == "MonoBehaviour"
                                               && classType.Namespace == "UnityEngine");

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

                    sb.Append($" <a href=\"{path}\" line=\"{frame.GetFileLineNumber()}\">");
                    sb.Append(path);
                    sb.Append(":");
                    sb.Append(frame.GetFileLineNumber().ToString());
                    sb.Append("</a>)");
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
                    return new StringBuilder(); //this is just to handle finalizer that could be called after the _threadSafeStrings is finalized. So pretty rare
                }
            }
        }

        static ThreadLocal<StringBuilder> stringBuilder;

        static string projectFolder;
        static int    MAINTHREADID;
    }
}
#endif