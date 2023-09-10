#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class MonoBehaviourPool<T> : ThreadSafeObjectPool<T> where T:MonoBehaviour
    {
#if POOL_DEBUGGER
    public MonoBehaviourPool()
    {
        GameObject poolDebugger = new GameObject("MonoBehaviourPoolDebugger");

        poolDebugger.AddComponent<PoolDebugger>().SetPool(this);
    }
#endif
        protected override void OnDispose()
        {
            using (var recycledPoolsGetValues = _recycledPools.GetValues)
            {
                var values = recycledPoolsGetValues.GetValues(out var count);
                for (int i = 0; i < count; i++)                     
                {
                    using (var stacks = values[i].GetValues)
                    {
                        var stackValues = stacks.GetValues();
                        foreach (var obj in stackValues)
                            GameObject.Destroy(obj);
                    }
                }
            }
        }
    }
}
#endif