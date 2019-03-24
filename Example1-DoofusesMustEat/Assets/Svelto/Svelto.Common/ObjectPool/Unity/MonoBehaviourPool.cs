#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class MonoBehaviourPool : ObjectPool<MonoBehaviour>
    {
#if POOL_DEBUGGER
    public MonoBehaviourPool()
    {
        GameObject poolDebugger = new GameObject("MonoBehaviourPoolDebugger");

        poolDebugger.AddComponent<PoolDebugger>().SetPool(this);
    }
#endif
    }
}
#endif