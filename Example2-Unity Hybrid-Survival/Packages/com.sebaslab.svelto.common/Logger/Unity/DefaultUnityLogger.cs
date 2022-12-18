#if UNITY_5_3_OR_NEWER || UNITY_5
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Svelto.Utilities
{
    public class DefaultUnityLogger : ILogger
    {
        public static void Init()
        {
            Console.SetLogger(new DefaultUnityLogger());
        }

        public void Log(string txt, LogType type = LogType.Log, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            var currentManagedThreadId = Environment.CurrentManagedThreadId;
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

            StackTrace stackTrace = null;
            if (showLogStack)
            {
#if UNITY_EDITOR
                stackTrace = new StackTrace(Console.StackDepth, true);
#else
                if (type == LogType.Error || type == LogType.Exception)
                    stackTrace = new StackTrace(Console.StackDepth, true);
#endif
            }

            switch (type)
            {
                case LogType.Log:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Log, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, log);
                    }
                    else
                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Log, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

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

                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Log, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                        Application.SetStackTraceLogType(UnityEngine.LogType.Log, log);
                    }
                    else
                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Log, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));
#endif
                    break;
                }
                case LogType.Warning:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Warning);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Warning, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                        Application.SetStackTraceLogType(UnityEngine.LogType.Warning, log);
                    }
                    else
                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Warning, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                    break;
                }
                case LogType.Error:
                case LogType.Exception:
                {
                    if (MAINTHREADID == currentManagedThreadId)
                    {
                        //SetStacktrace can be called only in the editor and it's enable for LOG only in the editor
                        var log = Application.GetStackTraceLogType(UnityEngine.LogType.Error);
                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);

                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Error, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                        Application.SetStackTraceLogType(UnityEngine.LogType.Error, log);
                    }
                    else
                        ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Error, null,
                            ConsoleUtilityForUnity.LogFormatter(txt, type, showLogStack, e, frame, dataString,
                                stackTrace));

                    break;
                }
            }
        }

        public void OnLoggerAdded()
        {
            MAINTHREADID = Environment.CurrentManagedThreadId;
            //We want to keep the stack for not Svelto.Console log.
            //SlowLogger will disable the stack for Svelto.Console log, as Svelto.Console has it's own stack generator
            //If CatchEmAll is used, the external unity stack trace is passed instead
#if !UNITY_EDITOR
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
#endif
            Console.Log("Svelto Default Unity Logger added");
        }

        public void CompressLogsToZipAndShow(string zipName)
        {
            
        }

        static int MAINTHREADID;
    }
}
#endif