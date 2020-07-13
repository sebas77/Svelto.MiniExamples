#if UNITY_5_3_OR_NEWER || UNITY_5
using System.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            _stringBuilder = new ThreadLocal<StringBuilder>(ValueFactory);

            Console.SetLogger(new SlowUnityLogger());
        }

        public void Log(string txt, LogType type = LogType.Log, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            string stack;

            switch (type)
            {
                case LogType.Log:
                {
#if UNITY_EDITOR
                    stack = ExtractFormattedStackTrace();

                    Debug.Log("<b><color=teal>".FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                        .FastConcat(Environment.NewLine, dataString));
#else
                    Debug.Log(txt);
#endif
                    break;
                }
                case LogType.Warning:
                {
#if UNITY_EDITOR
                    stack = ExtractFormattedStackTrace();

                    Debug.LogWarning("<b><color=yellow>".FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                        .FastConcat(Environment.NewLine, dataString));
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
                        txt = txt.FastConcat(e.Message);
                        stack = ExtractFormattedStackTrace(new StackTrace(e, true));
                    }
                    else
                        stack = ExtractFormattedStackTrace();

#if UNITY_EDITOR                    
                    var fastConcat = "<b><color=red>".FastConcat(txt, "</color></b> ", Environment.NewLine, stack)
                        .FastConcat(Environment.NewLine, dataString);

                    var error = Application.GetStackTraceLogType(UnityEngine.LogType.Error);
                    Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
                    Debug.LogError(fastConcat);
                    Application.SetStackTraceLogType(UnityEngine.LogType.Error, error);
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

            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

            Console.Log("Slow Unity Logger added");
        }

        /// <summary>
        ///     copied from Unity source code, whatever....
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            _stringBuilder.Value.Length = 0;

            var frame = new StackTrace(4, true);

            for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
            {
                FormatStack(stackTrace, index1, _stringBuilder.Value);
            }

            for (var index1 = 0; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, _stringBuilder.Value);
            }

            return _stringBuilder.ToString();
        }

        string ExtractFormattedStackTrace()
        {
            _stringBuilder.Value.Length = 0;

            var frame = new StackTrace(4, true);

            for (var index1 = 0; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, _stringBuilder.Value);
            }

            return _stringBuilder.ToString();
        }

        void FormatStack(StackTrace stackTrace, int index1, StringBuilder stringBuilder)
        {
            var frame = stackTrace.GetFrame(index1);
            var method = frame.GetMethod();
            if (method != null)
            {
                var declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    var str1 = declaringType.Namespace;
                    if (!string.IsNullOrEmpty(str1))
                    {
                        stringBuilder.Append(str1);
                        stringBuilder.Append(".");
                    }

                    stringBuilder.Append(declaringType.Name);
                    stringBuilder.Append(":");
                    stringBuilder.Append(method.Name);
                    stringBuilder.Append("(");
                    var index2 = 0;
                    var parameters = method.GetParameters();
                    var flag = true;
                    for (; index2 < parameters.Length; ++index2)
                    {
                        if (!flag)
                            stringBuilder.Append(", ");
                        else
                            flag = false;
                        stringBuilder.Append(parameters[index2].ParameterType.Name);
                    }

                    stringBuilder.Append(")");
                    var str2 = frame.GetFileName();
                    if (str2 != null &&
                        (!(declaringType.Name == "Debug") || !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "Logger") || !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "DebugLogHandler") ||
                         !(declaringType.Namespace == "UnityEngine")) &&
                        (!(declaringType.Name == "Assert") ||
                         !(declaringType.Namespace == "UnityEngine.Assertions")) && (!(method.Name == "print") ||
                                                                                     !(declaringType.Name ==
                                                                                       "MonoBehaviour") ||
                                                                                     !(declaringType.Namespace ==
                                                                                       "UnityEngine")))
                    {
                        stringBuilder.Append(" (at ");
#if UNITY_EDITOR
                        str2 = str2.Replace(@"\", "/");
                        if (!string.IsNullOrEmpty(projectFolder) && str2.StartsWith(projectFolder))

                            str2 = str2.Substring(projectFolder.Length, str2.Length - projectFolder.Length);
#endif
                        stringBuilder.Append(str2);
                        stringBuilder.Append(":");
                        stringBuilder.Append(frame.GetFileLineNumber().ToString());
                        stringBuilder.Append(")");
                    }

                    stringBuilder.Append("\n");
                }
            }
        }

        static ThreadLocal<StringBuilder> _stringBuilder;

        static string projectFolder;
    }
}
#endif