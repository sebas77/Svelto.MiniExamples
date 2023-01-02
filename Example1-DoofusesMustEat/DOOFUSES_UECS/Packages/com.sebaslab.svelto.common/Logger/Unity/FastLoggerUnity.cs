#if UNITY_5_3_OR_NEWER || UNITY_5

#if UNITY_EDITOR
#define ISEDITOR
#endif
//#define DEBUG_FASTER

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || DEBUG_FASTER
#define REDIRECT_CONSOLE
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using Svelto.DataStructures;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using ThreadPriority = System.Threading.ThreadPriority;
using Volatile = System.Threading.Volatile;

namespace Svelto.Utilities
{
    static class FasterUnityLoggerUtility
    {
        const string CUSTOM_NAME_FLAG = "-customLogName";
        const int CYCLE_SIZE = 10;

        static string BASENAME = "outputLog";

        static readonly string FOLDER = Path.GetDirectoryName(Application.dataPath)
          + Path.DirectorySeparatorChar + "debuglogs";

        [Conditional("REDIRECT_CONSOLE")]
        internal static void Init()
        {
            //overwrites if commandline found
            GetFilenameFromCommandLine(ref BASENAME);

            _folder = Directory.CreateDirectory(FOLDER);

            SetupLogger();
        }

        [Conditional("REDIRECT_CONSOLE")]
        internal static void Close()
        {
            _consoleOut.Flush();
            _consoleOut.Close();

            System.Console.SetOut(_originalConsoleOutput);
        }

        [Conditional("REDIRECT_CONSOLE")]
        internal static void ForceFlush()
        {
            _consoleOut.Flush();
        }

        [Conditional("REDIRECT_CONSOLE")]
        static void SetupLogger()
        {
            _originalConsoleOutput = System.Console.Out;

            _consoleOut = File.CreateText(GetFileNameToUse());

            System.Console.SetOut(_consoleOut);
        }

        static void GetFilenameFromCommandLine(ref string fileName)
        {
            bool foundFlag = false;
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
            var directory = new DirectoryInfo(_folder.FullName);

            var fileInfos = directory.GetFiles(BASENAME + "*.txt");

            int nextFileId = 0;
            if (fileInfos.Length > 0)
            {
                nextFileId = -1;
                do
                {
                    var myFile = fileInfos
                           .OrderByDescending(f => f.LastWriteTime)
                           .First();
                    try
                    {
                        string index = new String(myFile.Name.Where(Char.IsDigit).ToArray());

                        var i = int.Parse(index);
                        
                        nextFileId = (i + 1) % CYCLE_SIZE;
                    }
                    catch
                    {
                        File.Delete(myFile.FullName);
                        fileInfos = directory.GetFiles(BASENAME + "*.txt");
                    }
                } while (nextFileId == -1 && fileInfos.Length > 0);
            }

            string nextFileName = GenerateName(nextFileId);

            //overwrite file if exists
            File.Delete(nextFileName);

            var consoleLogPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Player-prev.log";
            if (File.Exists(consoleLogPath))
            {
                var generateUnityLogName = GenerateNameForUnityLogCopy(nextFileId);
                File.Delete(generateUnityLogName);
                File.Copy(consoleLogPath, generateUnityLogName);
            }

            return nextFileName;
        }

        static string GenerateName(int i)
        {
            return _folder.FullName + Path.DirectorySeparatorChar + BASENAME + i + ".txt";
        }
        
        static string GenerateNameForUnityLogCopy(int i)
        {
            return _folder.FullName + Path.DirectorySeparatorChar + "PlayerLog" + i + ".txt";
        }

        public static void CompressLogsToZipAndShow(string zipName)
        {
            Close();
            
            var consoleLogPath = Application.persistentDataPath+ Path.DirectorySeparatorChar + "Player.log";
            if (File.Exists(consoleLogPath))
            {
                var generateUnityLogName = _folder.FullName + Path.DirectorySeparatorChar + "LastPlayerLog.txt";
                File.Delete(generateUnityLogName);
                File.Copy(consoleLogPath, generateUnityLogName);
            }

            var destinationArchiveFileName =
                Application.persistentDataPath + Path.DirectorySeparatorChar + zipName;
            
            File.Delete(destinationArchiveFileName);

            ZipFile.CreateFromDirectory(_folder.FullName, destinationArchiveFileName);

            Init();

            Application.OpenURL($"file://{destinationArchiveFileName}");
        }

        static StreamWriter _consoleOut;
        static TextWriter _originalConsoleOutput;
        static DirectoryInfo _folder;
    }

    class FasterUnityLogger: ILogger
    {
        const uint SEED = 8938176;

        static bool _quitThread;

        static readonly ThreadSafeDictionary<uint, ErrorLogObject> _batchedErrorLogs;

        static readonly IComparer<ErrorLogObject> _comparison;
        static readonly ConcurrentQueue<ErrorLogObject> _notBatchedQueue;

        static readonly Thread _lowPrioThread;

        static int MAINTHREADID;
        static readonly FasterList<ErrorLogObject> _logs;
        static bool _isPaused;

        static FasterUnityLogger()
        {
            FasterUnityLoggerUtility.Init();
            Console.batchLog = true;

            var gameObject = new GameObject("FastMonoLogger");
            gameObject.AddComponent<FastMonoLogger>();
            GameObject.DontDestroyOnLoad(gameObject);

            _comparison = new ErrorComparer();
            _batchedErrorLogs = new ThreadSafeDictionary<uint, ErrorLogObject>();
            _notBatchedQueue = new ConcurrentQueue<ErrorLogObject>();

            _logs = new FasterList<ErrorLogObject>(_batchedErrorLogs.count + _notBatchedQueue.Count);

            _lowPrioThread = new Thread(StartQueue)
            {
                IsBackground = true
            };
            _lowPrioThread.Priority = ThreadPriority.BelowNormal;
            _lowPrioThread.Start();
        }

        public void OnLoggerAdded()
        {
            MAINTHREADID = Environment.CurrentManagedThreadId;
            
            //FasterLog doesn't use Unity Debug Log so we don't need to disable the stack

            Debug.Log("Svelto Fast Unity Logger added");
        }
#if UNITY_2021_3_OR_NEWER
        public void CompressLogsToZipAndShow(string zipName)
        {
            Volatile.Write(ref _isPaused, true);
            
            FasterUnityLoggerUtility.CompressLogsToZipAndShow(zipName);
            
            Volatile.Write(ref _isPaused, false);
        }
#endif  

        public void Log(string txt, LogType type = LogType.Log, bool showLogStack = true, Exception e = null,
            Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            if (data != null)
                dataString = DataToString.DetailString(data);

            var frame = $"[{DateTime.UtcNow.ToString("HH:mm:ss.fff")}] thread: {Environment.CurrentManagedThreadId}";
            try
            {
                if (MAINTHREADID == Environment.CurrentManagedThreadId)
                    frame += $" frame: {Time.frameCount}";
            }
            catch
            {
                //there is something wrong with  Environment.CurrentManagedThreadId
            }

            StackTrace stack = null;
            if (showLogStack)
            {
#if ISEDITOR
                stack = new StackTrace(Console.StackDepth, true);
#else
                if (type == LogType.Error || type == LogType.Exception)
                    stack = new StackTrace(Console.StackDepth, true);
#endif
            }
            else
            {
#if ISEDITOR
                stack = new StackTrace(Console.StackDepth, false);
#else
                if (type == LogType.Error || type == LogType.Exception)
                    stack = new StackTrace(Console.StackDepth, false);
#endif
            }

            if (Volatile.Read(ref Console.batchLog) == true)
            {
                //todo: why am I not enqueuing here too and batching in another thread?
                var stackString = stack == null ? string.Empty : stack.GetFrame(0).ToString();
                var strArray = Encoding.UTF8.GetBytes(txt.FastConcat(stackString));
                var logHash = Murmur3.MurmurHash3_x86_32(strArray, SEED);

                //todo: this can be optimized
                if (_batchedErrorLogs.ContainsKey(logHash))
                {
                    _batchedErrorLogs[logHash] = new ErrorLogObject(_batchedErrorLogs[logHash]);
                }
                else
                {
                    _batchedErrorLogs[logHash] =
                        new ErrorLogObject(txt, stack, type, e, frame, showLogStack, dataString);
                }
            }
            else
            {
                _notBatchedQueue.Enqueue(new ErrorLogObject(txt, stack, type, e, frame, showLogStack, dataString, 0));
            }
        }

        static void OtherThreadFlushLogger()
        {
            if (_batchedErrorLogs.count + _notBatchedQueue.Count == 0)
                return;

            _logs.Clear();

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

                var log = ConsoleUtilityForUnity.LogFormatter(
                    instance.msg, instance.logType, instance.showStack,
                    instance.exception, instance.frame, instance.dataString, instance.stackT);

                if (intCount > 1)
                {
                    log = ReplaceFirstOccurrence(
                        log, "thread: ",
                        "Hit count: ".FastConcat(intCount).FastConcat(" thread: "));
                    LOG(log, instance.logType);
                }
                else
                    LOG(log, instance.logType);
            }
        }

        static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        static void LOG(string str, LogType instanceLOGType)
        {
#if !ISEDITOR
            str = System.Text.RegularExpressions.Regex.Replace(str, "</?[a-z](?:[^>\"']|\"[^\"]*\"|'[^']*')*>", "");
            System.Console.Write(str);
#else
            switch (instanceLOGType)
            {
                case LogType.Error:
                case LogType.Exception:
                    ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Error, null, str);
                    break;
                case LogType.Log:
                case LogType.LogDebug:
                    ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Log, null, str);
                    break;
                case LogType.Warning:
                    ConsoleUtilityForUnity.defaultLogHandler.LogFormat(UnityEngine.LogType.Warning, null, str);
                    break;
            }
#endif
        }

        static void StartQueue(object state)
        {
            Thread.Sleep(1000); //let's be sure that the first iteration doesn't happen right at the begin

            Thread.MemoryBarrier();
            while (Volatile.Read(ref _quitThread) == false)
            {
                if (Volatile.Read(ref _isPaused) == false)
                {
                    FlushToFile();
                }

                Thread.Sleep(1000);
                
            }

            FasterUnityLoggerUtility.Close();
        }

        static void FlushToFile()
        {
            OtherThreadFlushLogger();

            FasterUnityLoggerUtility.ForceFlush();
        }

        class FastMonoLogger: MonoBehaviour
        {
            void Awake()
            {
                SceneManager.sceneLoaded += FlushLogger;
            }

            void OnDestroy()
            {
                SceneManager.sceneLoaded -= FlushLogger;

                Volatile.Write(ref _quitThread, true);

                FlushToFile();
            }

            static void FlushLogger(Scene arg0, LoadSceneMode arg1)
            {
                FlushToFile();
            }
        }

        class ErrorComparer: IComparer<ErrorLogObject>
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

            public StackTrace stackT { get; }
            public readonly Exception exception;

            public ErrorLogObject(string msg, StackTrace stack, LogType logType, Exception exc, string frm, bool sstack,
                string dataString, ushort initialCount = 1)
            {
                count = initialCount;
                this.msg = msg;
                index = Interlocked.Increment(ref errorIndex);
                exception = exc;
                frame = frm;
                stackT = stack;
                this.logType = logType;
                showStack = sstack;
                this.dataString = dataString;
            }

            public ErrorLogObject(ErrorLogObject obj)
            {
                count = obj.count;
                count++;
                msg = obj.msg;
                stackT = obj.stackT;
                index = obj.index;
                exception = obj.exception;
                frame = obj.frame;
                stackT = obj.stackT;
                logType = obj.logType;
                showStack = obj.showStack;
                dataString = obj.dataString;
            }

            public readonly int index;
            public readonly ushort count;
            public readonly LogType logType;
            public readonly bool showStack;
            public readonly string frame;
            public readonly string dataString;
        }

        public static void Init()
        {
            Console.SetLogger(new FasterUnityLogger());
        }
    }
}
#endif