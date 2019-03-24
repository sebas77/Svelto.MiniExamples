#if UNITY_5_3_OR_NEWER || UNITY_5
using System.Collections.Generic;

namespace Svelto.Utilities
{
    public class SlowUnityLogger : ILogger
    {
        public void Log(string txt, string stack = null, LogType type = LogType.Log, Dictionary<string, string> data = null)
        {
            var dataString = string.Empty;
            
            if (data != null)
                 dataString = DataToString.DetailString(data);
            
            switch (type)
            {
                case LogType.Log:
                    UnityEngine.Debug.Log(txt);
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(txt);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    UnityEngine.Debug.LogError(stack != null ? 
                                                   "<color=cyan> ".FastConcat(txt, "</color> ", stack) : 
                                                   "<color=cyan> ".FastConcat(txt, "</color> ", dataString));
                    break;
            }
        }
    }
}
#endif