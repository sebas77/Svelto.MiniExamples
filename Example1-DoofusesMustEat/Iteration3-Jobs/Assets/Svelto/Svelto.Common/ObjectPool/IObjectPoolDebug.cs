#if DEBUG && !PROFILE
using System;
using System.Collections.Generic;

namespace Svelto.ObjectPool
{
    public interface IObjectPoolDebug
    {
        int GetNumberOfObjectsCreatedSinceLastTime();
        int GetNumberOfObjectsReusedSinceLastTime();
        int GetNumberOfObjectsRecycledSinceLastTime();

        List<ObjectPoolDebugStructureInt>    DebugPoolInfo(List<ObjectPoolDebugStructureInt>         debugInfo);
        List<ObjectPoolDebugStructureString> DebugNamedPoolInfo(List<ObjectPoolDebugStructureString> debugInfo);
    }

    [Serializable]
    public struct ObjectPoolDebugStructureString
    {
        public string key;
        public int    count;

        public ObjectPoolDebugStructureString(string key, int count) : this()
        {
            this.key   = key;
            this.count = count;
        }
    }

    [Serializable]
    public struct ObjectPoolDebugStructureInt
    {
        public int key;
        public int count;

        public ObjectPoolDebugStructureInt(int key, int count) : this()
        {
            this.key   = key;
            this.count = count;
        }
    }
}
#endif