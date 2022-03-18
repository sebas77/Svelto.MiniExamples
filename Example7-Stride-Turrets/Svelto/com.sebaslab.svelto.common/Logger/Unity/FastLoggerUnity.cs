#if UNITY_5_3_OR_NEWER || UNITY_5
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Svelto.DataStructures;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using ThreadPriority = System.Threading.ThreadPriority;
using Volatile = System.Threading.Volatile;

// Scene, LoadSceneMode

namespace Svelto.Utilities
{
#if !UNITY_EDITOR
    static class FasterUnityLoggerUtility
    {
        const string CUSTOM_NAME_FLAG = "-customLogName";
        const int    CYCLE_SIZE = 10;

        static string _baseName = "outputLog";

        static System.IO.StreamWriter _consoleOut;
        static System.IO.TextWriter   _originalConsoleOutput;

        internal static void Init()
        {
            //overwrites if commandline found
            GetFilenameFromCommandLine(ref _baseName);

            SetupLogger();
        }

        internal static void Close()
        {
            _consoleOut.Flush();
            _consoleOut.Close();

            System.Console.SetOut(_originalConsoleOutput);
        }

        internal static void ForceFlush()
        {
            _consoleOut.Flush();
        }

        static void SetupLogger()
        {
            _originalConsoleOutput = System.Console.Out;

            _consoleOut = System.IO.File.CreateText(GetFileNameToUse());

            System.Console.SetOut(_consoleOut);
        }

        static void GetFilenameFromCommandLine(ref string fileName)
        {
            bool     foundFlag = false;
            string[] args = Environment.GetCommandLineArgs();
            for (int iArg = 0; iArg < args.Length; ++iArg)
            {
                if (foundFlag)
                {
                    fileName = args[iArg];
                    return;
                }

                if (args[iArg].CompareTo(CUSTOM_NAME_FLAG) == 0)
                    foundFlag = true;
            }
        }

        static string GetFileNameToUse()
        {
            int i = 0;
            for (; i < CYCLE_SIZE + 1; ++i)
            {
                if (System.IO.File.Exists(GenerateName(i)) == false)
                    break;
            }

            int    nextFileId = (i + 1) % (CYCLE_SIZE + 1);
            string nextFileName = GenerateName(nextFileId);

            //use the fact that the file doesn't exist to determine which file to write in the cycle
            if (System.IO.File.Exists(nextFileName))
                System.IO.File.Delete(nextFileName);

            return GenerateName(i);
        }

        static string GenerateName(int i)
        {
            return _baseName + i + ".txt";
        }
    }
#endif

    public static class FasterLog
    {
        public static void Use()
        {
#if !DEBUG || PROFILE_SVELTO            
#if !UNITY_EDITOR
            FasterUnityLoggerUtility.Init();
#endif
            Console.SetLogger(new FasterUnityLogger());
#endif
        }
    }

    class FasterUnityLogger : ILogger
    {
        const uint SEED = 8938176;

        static bool _quitThread;

        static readonly ThreadSafeDictionary<uint, ErrorLogObject> _batchedErrorLogs;
        static readonly IComparer<ErrorLogObject>                      _comparison;
        static readonly ConcurrentQueue<ErrorLogObject>                _notBatchedQueue;

        static readonly Thread _lowPrioThread;

        static          int                        MAINTHREADID;
        static readonly FasterList<ErrorLogObject> _logs;

        static FasterUnityLogger()
        {
            var gameObject = new GameObject("FastMonoLogger");
            gameObject.AddComponent<FastMonoLogger>();

            _comparison       = new ErrorComparer();
            _batchedErrorLogs = new ThreadSafeDictionary<uint, ErrorLogObject>();

            _lowPrioThread          = new Thread(StartQueue) { IsBackground = true };
            _notBatchedQueue        = new ConcurrentQueue<ErrorLogObject>();
            _lowPrioThread.Priority = ThreadPriority.BelowNormal;
            _lowPrioThread.Start();
            
            _logs = new FasterList<ErrorLogObject>(_batchedErrorLogs.count + _notBatchedQueue.Count);
        }

        public void OnLoggerAdded()
        {
            MAINTHREADID = Environment.CurrentManagedThreadId;

            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Assert, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

            Debug.Log("Fast Unity Logger added");
        }

        public void Log(string txt, LogType type = LogType.Log, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);
            
            var frame = $"[{DateTime.UtcNow.ToString("HH:mm:ss.fff")}] thread: {Environment.CurrentManagedThreadId}";
            try
            {
                if (MAINTHREADID == Environment.CurrentManagedThreadId) frame += $" frame: {Time.frameCount}";
            }
            catch
            {
                //there is something wrong with  Environment.CurrentManagedThreadId
            }

            StackTrace stack = null;
#if UNITY_EDITOR
            stack = new StackTrace(3, true);
#else
            if (type == LogType.Error || type == LogType.Exception)
                stack = new StackTrace(3, true);
#endif

            if (Volatile.Read(ref Console.batchLog) == true)
            {
                var stackString = stack == null ? string.Empty : stack.GetFrame(0).ToString();
                var strArray    = Encoding.UTF8.GetBytes(txt.FastConcat(stackString));
                var logHash     = Murmur3.MurmurHash3_x86_32(strArray, SEED);

                if (_batchedErrorLogs.ContainsKey(logHash))
                {
                    _batchedErrorLogs[logHash] = new ErrorLogObject(_batchedErrorLogs[logHash]);
                }
                else
                {
#if !UNITY_EDITOR
                    if (type == LogType.Error) stack = new StackTrace(3, true);
#endif

                    _batchedErrorLogs[logHash] =
                        new ErrorLogObject(txt, stack, type, e, frame, showLogStack, dataString);
                }
            }
            else
            {
#if !UNITY_EDITOR
                    if (type == LogType.Error) stack = new StackTrace(3, true);
#endif

                _notBatchedQueue.Enqueue(new ErrorLogObject(txt, stack, type, e, frame, showLogStack, dataString,
                    0));
            }
        }

        static void OtherThreadFlushLogger()
        {
            if (_batchedErrorLogs.count + _notBatchedQueue.Count == 0)
                return;
            
            _logs.FastClear();
            
            using (var recycledPoolsGetValues = _batchedErrorLogs.GetValues)
            {
                var values = recycledPoolsGetValues.GetValues(out var logsCount);
                for (int i = 0; i < logsCount; i++)                     
                {
                    _logs.Add(values[i]);
                }
            }

            _batchedErrorLogs.Clear();

            while (_notBatchedQueue.TryDequeue(out var value)) _logs.Add(value);

            Array.Sort(_logs.ToArrayFast(out var count), 0, count, _comparison);

            for (var i = 0; i < count; i++)
            {
                var instance = _logs[i];
                var intCount = instance.count;

                var log = SlowUnityLogger.LogFormatter(instance.msg, instance.logType, instance.showStack,
                    instance.exception, instance.frame, instance.dataString, instance.stackT);

                if (intCount != 0)
                    LOG("Hit count: ".FastConcat(intCount.ToString(), " ", log), instance.logType);
                else
                    LOG(log, instance.logType);
            }
        }

        static void LOG(string str, LogType instanceLOGType)
        {
#if !UNITY_EDITOR
            System.Console.Write(str);
#else
            switch (instanceLOGType)
            {
                case LogType.Error:
                case LogType.Exception:
                    Debug.LogError(str);
                    break;
                case LogType.Log:
                case LogType.LogDebug:
                    Debug.Log(str);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(str);
                    break;
            }
#endif
        }

        static void StartQueue(object state)
        {
            Thread.Sleep(1000); //let's be sure that the first iteration doesn't happen right at the begin

            Thread.MemoryBarrier();
            while (_quitThread == false)
            {
                OtherThreadFlushLogger();
#if !UNITY_EDITOR
                FasterUnityLoggerUtility.ForceFlush();
#endif
                Thread.Sleep(1000);
            }
        }

        class FastMonoLogger : MonoBehaviour
        {
            void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }

            // `OnEnable` is called after `Awake`
            void OnEnable()
            {
                SceneManager.sceneLoaded += FlushLogger;
            }

            // called when the game is terminated
            void OnDisable()
            {
                SceneManager.sceneLoaded -= FlushLogger;
            }

            void OnApplicationQuit()
            {
                Volatile.Write(ref _quitThread, true);

                OtherThreadFlushLogger();
            }

            static void FlushLogger(Scene arg0, LoadSceneMode arg1)
            {
                OtherThreadFlushLogger();
            }
        }

        class ErrorComparer : IComparer<ErrorLogObject>
        {
            public int Compare(ErrorLogObject x, ErrorLogObject y)
            {
                return x.index.CompareTo(y.index);
            }
        }

        public readonly struct ErrorLogObject
        {
            static int errorIndex;

            public string msg { get; }

            public          StackTrace stackT   { get; }
            public readonly Exception  exception;

            public ErrorLogObject(string msg, StackTrace stack, LogType logType, Exception exc,
                string frm, bool sstack, string dataString, ushort initialCount = 1)
            {
                count           = initialCount;
                this.msg        = msg;
                index           = Interlocked.Increment(ref errorIndex);
                exception       = exc;
                frame           = frm;
                stackT          = stack;
                this.logType    = logType;
                showStack       = sstack;
                this.dataString = dataString;
            }

            public ErrorLogObject(ErrorLogObject obj)
            {
                count = obj.count;
                count++;
                msg        = obj.msg;
                stackT     = obj.stackT;
                index      = obj.index;
                exception  = obj.exception;
                frame      = obj.frame;
                stackT     = obj.stackT;
                logType    = obj.logType;
                showStack  = obj.showStack;
                dataString = obj.dataString;
            }

            public readonly int     index;
            public readonly ushort  count;
            public readonly LogType logType;
            public readonly bool    showStack;
            public readonly string  frame;
            public readonly string  dataString;
        }
    }
}
#endif