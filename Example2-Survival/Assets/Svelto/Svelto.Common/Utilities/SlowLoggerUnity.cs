#if UNITY_5_3_OR_NEWER || UNITY_5
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
        static SlowUnityLogger()
        {
            Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
            projectFolder = Application.dataPath.Replace("Assets", "");
        }
        
        public void Log(string txt, LogType type = LogType.Log, Exception e = null,
                        Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;

            string stack = null;
            if (e != null)
                stack = ExtractFormattedStackTrace(new StackTrace(e, true));

            if (data != null)
                dataString = DataToString.DetailString(data);

            switch (type)
            {
                case LogType.Log:
                    Debug.Log(txt);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(txt);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    Debug.LogError("<color=cyan> ".FastConcat(txt, "</color> ", Environment.NewLine, stack).FastConcat(Environment.NewLine, dataString));
                    break;
            }
        }

        /// <summary>
        ///     copied from Unity source code, whatever....
        /// </summary>
        /// <param name="stackTrace"></param>
        /// <returns></returns>
        internal string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            _stringBuilder.Length = 0;
            
            var frame = new StackTrace(true);
            
            for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
            {
                FormatStack(stackTrace, index1, _stringBuilder);
            }
            for (var index1 = 4; index1 < frame.FrameCount; ++index1)
            {
                FormatStack(frame, index1, _stringBuilder);
            }

            return _stringBuilder.ToString();
        }

        void FormatStack(StackTrace stackTrace, int index1, StringBuilder stringBuilder)
        {
            var frame  = stackTrace.GetFrame(index1);
            var method = frame.GetMethod();
            if (method != null)
            {
                var declaringType = method.DeclaringType;
                if (declaringType != null)
                {
                    var str1 = declaringType.Namespace;
                    if (str1 != null && str1.Length != 0)
                    {
                        stringBuilder.Append(str1);
                        stringBuilder.Append(".");
                    }

                    stringBuilder.Append(declaringType.Name);
                    stringBuilder.Append(":");
                    stringBuilder.Append(method.Name);
                    stringBuilder.Append("(");
                    var index2     = 0;
                    var parameters = method.GetParameters();
                    var flag       = true;
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
        readonly StringBuilder _stringBuilder = new StringBuilder(Byte.MaxValue);
        
        static readonly string projectFolder;
    }
   
}
#endif