#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class GameObjectPool : ThreadSafeObjectPool<GameObject>
    {
#if POOL_DEBUGGER
    public GameObjectPool()
    {
        GameObject poolDebugger = new GameObject("GameObjectPoolDebugger");

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