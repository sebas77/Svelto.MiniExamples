#if DEBUG && !PROFILE_SVELTO
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