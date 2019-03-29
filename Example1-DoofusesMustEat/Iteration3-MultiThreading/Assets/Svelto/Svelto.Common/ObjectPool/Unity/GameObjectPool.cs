#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class GameObjectPool : ObjectPool<GameObject>
    {
#if POOL_DEBUGGER
    public GameObjectPool()
    {
        GameObject poolDebugger = new GameObject("GameObjectPoolDebugger");

        poolDebugger.AddComponent<PoolDebugger>().SetPool(this);
    }
#endif
    }
}
#endif