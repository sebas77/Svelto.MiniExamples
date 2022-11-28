#if UNITY_5_3_OR_NEWER || UNITY_5
using System;
using Svelto.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using LogType = UnityEngine.LogType;

namespace Svelto
{
    public static partial class Console
    {
        public class SveltoConsoleLogHandler : ILogHandler
        {
            public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
            {
                var message = string.Format(format, args);

                switch (logType)
                {
                    case LogType.Error:
                        LogError(message);
                        break;
                    case LogType.Assert:
                        LogError(message);
                        break;
                    case LogType.Warning:
                        LogWarning(message);
                        break;
                    case LogType.Log:
                        Log(message);
                        break;
                    case LogType.Exception:
                        LogError(message);
                        break;
                }
            }

            public void LogException(Exception exception, UnityEngine.Object context)
            {
                Console.LogException(exception);
            }
        }

        public class SveltoSystemOutInterceptor : System.IO.TextWriter
        {
            public override void Write(string value)
            {
                Log(value);
            }

            public override System.Text.Encoding Encoding => System.Text.Encoding.Default;
        }

        public static void CatchEmAll()
        {
            if (_initialized == false)
            {
                _originalConsoleOutput = System.Console.Out;
                System.Console.SetOut(new SveltoSystemOutInterceptor());
#if UNITY_EDITOR
                _originals = (Application.GetStackTraceLogType(LogType.Warning),
                    Application.GetStackTraceLogType(LogType.Assert), Application.GetStackTraceLogType(LogType.Error),
                    Application.GetStackTraceLogType(LogType.Log));
#endif
                Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

                ConsoleUtilityForUnity.defaultLogHandler = Debug.unityLogger.logHandler;
                Debug.unityLogger.logHandler             = new SveltoConsoleLogHandler();
                StackDepth                               = 5;

#if UNITY_EDITOR
                EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
                EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
#endif
                _initialized = true;
            }
        }

        public static class FasterLog
        {
            public static void Use(bool catchEmAll)
            {
                DefaultUnityLogger.Init(); //first set to the Default Logger to avoid stack overflow with SimpleLogger due to SveltoSystemOutInterceptor
                
                if (catchEmAll)
                    Console.CatchEmAll(); //this must happen first otherwise it will override the set out console of FasterUnityLogger

                FasterUnityLogger.Init();
            }
        }
#if UNITY_EDITOR
        static void OnEditorChangedMode(PlayModeStateChange mode)
        {
            if (mode == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.unityLogger.logHandler = defaultLogHandler;

                Application.SetStackTraceLogType(LogType.Warning, _originals.warning);
                Application.SetStackTraceLogType(LogType.Assert, _originals.assert);
                Application.SetStackTraceLogType(LogType.Error, _originals.error);
                Application.SetStackTraceLogType(LogType.Log, _originals.log);

                System.Console.SetOut(_originalConsoleOutput);
            }
        }
#endif
#if UNITY_EDITOR
        static readonly Action<PlayModeStateChange> EditorApplicationOnplayModeStateChanged = OnEditorChangedMode;
#endif
        private static readonly ILogHandler defaultLogHandler = Debug.unityLogger.logHandler;
#if UNITY_EDITOR
        static (StackTraceLogType warning, StackTraceLogType assert, StackTraceLogType error, StackTraceLogType log)
            _originals;
#endif        

        static System.IO.TextWriter _originalConsoleOutput;
        static bool _initialized;
    }
}
#endif